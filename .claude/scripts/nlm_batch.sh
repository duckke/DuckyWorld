#!/usr/bin/env bash
# nlm_batch.sh — NotebookLM 소스 일괄 추가 + 아티팩트 병렬 생성 + 자동 다운로드
# bash 3.2 호환 (declare -A 미사용)
#
# 인터페이스:
#   --notebook-name "slug"       폴더명/파일명 슬러그 (필수), notebooks.json 조회 키
#   --notebook-title "제목"      노트북 새로 생성 시 제목 (없으면 slug 사용)
#   --output-dir "경로"          다운로드 기본 경로 (기본: .claude/docs/notebooklm)
#   --TYPE-sources "s1,s2"       TYPE 아티팩트에 사용할 소스 (쉼표 구분)
#   --TYPE-opts    "플래그들"     TYPE generate 커맨드에 추가할 CLI 옵션
#   --TYPE-prompt  "지시사항"     TYPE generate에 전달할 자연어 지시
#
# 동작:
#   - notebooks.json에서 slug로 노트북 조회 → 없으면 생성 후 JSON 갱신
#   - 기존 소스 전체 삭제 후 새 소스 추가
#   - 파일명 규칙: {slug}-{type}-{YYYYMMDDHHmmss}.{ext}
#   - 보관 규칙: 타입별 최대 3개, 초과 시 오래된 것부터 삭제

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
NLM="notebooklm"
POLL_INTERVAL=10
SOURCE_TIMEOUT=300
ARTIFACT_TIMEOUT=1800
MAX_FILES_PER_TYPE=3

# bash 3.2 호환 associative array 대체용 temp 디렉토리
WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

NOTEBOOK_NAME=""
NOTEBOOK_TITLE=""
OUTPUT_BASE="${PROJECT_ROOT}/.claude/docs/notebooklm"

# ── 인수 파싱 ──────────────────────────────────────────────
while [[ $# -gt 0 ]]; do
  key="$1"
  case "$key" in
    --notebook-name)  NOTEBOOK_NAME="$2";  shift 2 ;;
    --notebook-title) NOTEBOOK_TITLE="$2"; shift 2 ;;
    --output-dir)     OUTPUT_BASE="$2";    shift 2 ;;
    --*-sources)
      type="${key#--}"; type="${type%-sources}"
      echo "$2" > "$WORK/src_${type}"
      echo "$type" >> "$WORK/types"
      shift 2 ;;
    --*-opts)
      type="${key#--}"; type="${type%-opts}"
      printf '%s' "$2" > "$WORK/opts_${type}"
      shift 2 ;;
    --*-prompt)
      type="${key#--}"; type="${type%-prompt}"
      printf '%s' "$2" > "$WORK/prompt_${type}"
      shift 2 ;;
    *) echo "알 수 없는 옵션: $key" >&2; exit 1 ;;
  esac
done

# ── 검증 ──────────────────────────────────────────────────
[[ -z "$NOTEBOOK_NAME" ]] && { echo "오류: --notebook-name 필요" >&2; exit 1; }
[[ ! -f "$WORK/types" ]] && { echo "오류: 아티팩트 타입 최소 1개 필요 (예: --infographic-sources)" >&2; exit 1; }

sort -u "$WORK/types" -o "$WORK/types"
[[ -z "$NOTEBOOK_TITLE" ]] && NOTEBOOK_TITLE="$NOTEBOOK_NAME"

OUTPUT_DIR="${OUTPUT_BASE}/${NOTEBOOK_NAME}"
NB_JSON="${OUTPUT_BASE}/notebooks.json"
mkdir -p "$OUTPUT_DIR"

# ── 확장자 함수 ────────────────────────────────────────────
get_ext() {
  case "$1" in
    audio)                 echo "mp3"  ;;
    video|cinematic-video) echo "mp4"  ;;
    infographic)           echo "png"  ;;
    slide-deck)            echo "pdf"  ;;
    report)                echo "md"   ;;
    mind-map)              echo "json" ;;
    data-table)            echo "csv"  ;;
    quiz|flashcards)       echo "json" ;;
    *)                     echo "bin"  ;;
  esac
}

# ── PNG 5MB 초과 시 리사이즈 ──────────────────────────────
NOTION_SIZE_LIMIT=$((4800 * 1024))
resize_if_needed() {
  local file="$1" size width new_width
  size=$(stat -f%z "$file" 2>/dev/null || stat -c%s "$file")
  [[ $size -le $NOTION_SIZE_LIMIT ]] && return 0
  width=$(sips -g pixelWidth "$file" 2>/dev/null | awk '/pixelWidth/{print $2}')
  new_width=$(python3 -c "import math; print(int($width * math.sqrt($NOTION_SIZE_LIMIT / $size) * 0.95))")
  echo "  리사이즈: ${width}px → ${new_width}px"
  sips -Z "$new_width" "$file" --out "$file" > /dev/null 2>&1
}

# ── 파일 로테이션 ──────────────────────────────────────────
rotate_files() {
  local dir="$1" name="$2" type="$3" ext="$4" count
  local files_str
  files_str=$(ls -t "${dir}/${name}-${type}"-*.${ext} 2>/dev/null || true)
  [[ -z "$files_str" ]] && return 0
  count=$(echo "$files_str" | wc -l | tr -d ' ')
  if [[ $count -ge $MAX_FILES_PER_TYPE ]]; then
    local delete_count=$(( count - MAX_FILES_PER_TYPE + 1 ))
    echo "$files_str" | tail -n "$delete_count" | while IFS= read -r f; do
      echo "  삭제 (오래된 파일): $(basename "$f")"
      rm -f "$f"
    done
  fi
}

# ── [1/6] 인증 확인 ───────────────────────────────────────
echo "[1/6] 인증 확인..."
if ! $NLM auth check > /dev/null 2>&1; then
  echo "오류: 인증 실패. 'notebooklm login' 실행 후 재시도하세요." >&2
  exit 1
fi
echo "  OK"

# ── [2/6] 노트북 조회 또는 생성 ───────────────────────────
echo "[2/6] 노트북 확인..."
NOTEBOOK_ID=""

if [[ -f "$NB_JSON" ]]; then
  NOTEBOOK_ID=$(python3 - "$NOTEBOOK_NAME" "$NB_JSON" <<'PYEOF'
import json, sys

def find_id(obj, name):
    if isinstance(obj, dict):
        if name in obj:
            v = obj[name]
            if isinstance(v, str): return v
            if isinstance(v, dict) and 'id' in v: return v['id']
        for v in obj.values():
            r = find_id(v, name)
            if r: return r
    return None

name, path = sys.argv[1], sys.argv[2]
data = json.load(open(path))
print(find_id(data, name) or '')
PYEOF
)
fi

if [[ -z "$NOTEBOOK_ID" ]]; then
  echo "  노트북 없음 → 생성: '$NOTEBOOK_TITLE'"
  CREATE_RESULT=$($NLM create "$NOTEBOOK_TITLE" --json)
  NOTEBOOK_ID=$(echo "$CREATE_RESULT" | python3 -c "import sys,json; print(json.load(sys.stdin)['id'])")
  [[ -z "$NOTEBOOK_ID" ]] && { echo "오류: 노트북 생성 실패" >&2; exit 1; }
  echo "  생성됨: $NOTEBOOK_ID"

  python3 - "$NOTEBOOK_NAME" "$NOTEBOOK_ID" "$NB_JSON" <<'PYEOF'
import json, sys, os
name, nb_id, path = sys.argv[1], sys.argv[2], sys.argv[3]
data = json.load(open(path)) if os.path.exists(path) else {}
data[name] = nb_id
json.dump(data, open(path, 'w'), ensure_ascii=False, indent=2)
PYEOF
  echo "  notebooks.json 갱신됨"
else
  echo "  기존 노트북 사용: $NOTEBOOK_ID"
fi

# ── [3/6] 기존 소스 삭제 ──────────────────────────────────
echo "[3/6] 기존 소스 초기화..."
EXISTING_SOURCES=$($NLM source list -n "$NOTEBOOK_ID" --json 2>/dev/null \
  | python3 -c "
import sys, json
sources = json.load(sys.stdin).get('sources', [])
for s in sources:
    print(s['id'])
" 2>/dev/null || true)

if [[ -n "$EXISTING_SOURCES" ]]; then
  SRC_COUNT=$(echo "$EXISTING_SOURCES" | grep -c . || true)
  echo "  기존 소스 ${SRC_COUNT}개 삭제..."
  echo "$EXISTING_SOURCES" | while IFS= read -r sid; do
    [[ -z "$sid" ]] && continue
    $NLM source delete "$sid" -n "$NOTEBOOK_ID" -y > /dev/null 2>&1 \
      && echo "  삭제됨: $sid" \
      || echo "  삭제 실패 (무시): $sid"
  done
else
  echo "  기존 소스 없음"
fi

# ── 소스 고유 목록 수집 ────────────────────────────────────
> "$WORK/all_sources"
while IFS= read -r type; do
  src_file="$WORK/src_${type}"
  [[ ! -f "$src_file" ]] && continue
  IFS=',' read -ra items <<< "$(cat "$src_file")"
  for item in "${items[@]}"; do
    item="${item#"${item%%[![:space:]]*}"}"
    item="${item%"${item##*[![:space:]]}"}"
    [[ -z "$item" ]] && continue
    grep -qxF "$item" "$WORK/all_sources" 2>/dev/null || echo "$item" >> "$WORK/all_sources"
  done
done < "$WORK/types"

SRC_TOTAL=$(grep -c . "$WORK/all_sources" 2>/dev/null || echo 0)

# ── [4/6] 소스 병렬 추가 ──────────────────────────────────
echo "[4/6] 소스 추가 (${SRC_TOTAL}개)..."
mkdir -p "$WORK/src_ids"

while IFS= read -r src; do
  (
    result=$($NLM source add "$src" --notebook "$NOTEBOOK_ID" --json 2>/dev/null)
    sid=$(echo "$result" | python3 -c "import sys,json; print(json.load(sys.stdin).get('source_id',''))" 2>/dev/null || echo "")
    safe_key=$(python3 -c "import sys; s=sys.argv[1]; print(''.join(c if c.isalnum() else '_' for c in s))" "$src")
    echo "$src=$sid" > "$WORK/src_ids/${safe_key}"
    if [[ -z "$sid" ]]; then
      echo "  경고: 소스 추가 실패 → $(basename "$src")" >&2
    else
      echo "  추가됨: $(basename "$src") → $sid"
    fi
  ) &
done < "$WORK/all_sources"
wait

# 소스 ready 대기
echo "  소스 처리 대기..."
ELAPSED=0
while true; do
  NOT_READY=$($NLM source list -n "$NOTEBOOK_ID" --json 2>/dev/null \
    | python3 -c "
import sys, json
srcs = json.load(sys.stdin).get('sources', [])
print(sum(1 for s in srcs if s.get('status') != 'ready'))
" 2>/dev/null || echo "1")
  if [[ "$NOT_READY" -eq 0 ]]; then
    READY_COUNT=$($NLM source list -n "$NOTEBOOK_ID" --json 2>/dev/null \
      | python3 -c "import sys,json; print(len(json.load(sys.stdin).get('sources',[])))" 2>/dev/null || echo "0")
    if [[ "$READY_COUNT" -lt "$SRC_TOTAL" ]]; then
      echo "  경고: 소스 ${SRC_TOTAL}개 중 ${READY_COUNT}개만 등록됨" >&2
    else
      echo "  모든 소스 ready (${READY_COUNT}개)"
    fi
    break
  fi
  [[ $ELAPSED -ge $SOURCE_TIMEOUT ]] && { echo "오류: 소스 처리 타임아웃 (${SOURCE_TIMEOUT}s)" >&2; exit 1; }
  sleep $POLL_INTERVAL
  ELAPSED=$((ELAPSED + POLL_INTERVAL))
done

# ── [5/6] 아티팩트 병렬 생성 ──────────────────────────────
echo "[5/6] 아티팩트 생성..."
mkdir -p "$WORK/task_ids"

while IFS= read -r type; do
  (
    src_flags=""
    src_file="$WORK/src_${type}"
    if [[ -f "$src_file" ]]; then
      IFS=',' read -ra srcs <<< "$(cat "$src_file")"
      for s in "${srcs[@]}"; do
        s="${s#"${s%%[![:space:]]*}"}"; s="${s%"${s##*[![:space:]]}"}"
        safe_key=$(python3 -c "import sys; s=sys.argv[1]; print(''.join(c if c.isalnum() else '_' for c in s))" "$s")
        id_file="$WORK/src_ids/${safe_key}"
        if [[ -f "$id_file" ]]; then
          sid=$(cat "$id_file" | cut -d= -f2-)
          [[ -n "$sid" ]] && src_flags="$src_flags -s $sid"
        fi
      done
    fi

    extra_opts=""
    opts_file="$WORK/opts_${type}"
    [[ -f "$opts_file" ]] && extra_opts=$(cat "$opts_file")
    [[ "$type" == "infographic" && -z "$extra_opts" ]] && extra_opts="--orientation square --style sketch-note --language ko"

    # 명령어 배열 구성 (eval 없이)
    cmd=("$NLM" generate "$type" --notebook "$NOTEBOOK_ID" --json)
    [[ -n "$src_flags" ]] && cmd+=($src_flags)
    [[ -n "$extra_opts" ]] && cmd+=($extra_opts)

    prompt_file="$WORK/prompt_${type}"
    [[ -f "$prompt_file" ]] && cmd+=(--prompt "$(cat "$prompt_file")")

    result=$("${cmd[@]}" 2>/dev/null)
    task_id=$(echo "$result" | python3 -c "import sys,json; print(json.load(sys.stdin).get('task_id',''))" 2>/dev/null || echo "")
    echo "$task_id" > "$WORK/task_ids/${type}"
    echo "  생성 시작: $type → task:$task_id"
  ) &
done < "$WORK/types"
wait

# 아티팩트 완료 대기 (타입별 필터링)
echo "  완료 대기 중 (최대 ${ARTIFACT_TIMEOUT}s)..."
ELAPSED=0
while true; do
  TOTAL_PENDING=0
  while IFS= read -r type; do
    PENDING=$($NLM artifact list -n "$NOTEBOOK_ID" --type "$type" --json 2>/dev/null \
      | python3 -c "
import sys, json
arts = json.load(sys.stdin).get('artifacts', [])
print(sum(1 for a in arts if a.get('status') in ('in_progress', 'pending')))
" 2>/dev/null || echo "0")
    TOTAL_PENDING=$((TOTAL_PENDING + PENDING))
  done < "$WORK/types"

  [[ $TOTAL_PENDING -eq 0 ]] && break
  [[ $ELAPSED -ge $ARTIFACT_TIMEOUT ]] && { echo "경고: 타임아웃. 부분 다운로드 시도." >&2; break; }
  sleep 30
  ELAPSED=$((ELAPSED + 30))
done

# ── [6/6] 다운로드 ────────────────────────────────────────
echo "[6/6] 다운로드..."
TIMESTAMP=$(date +%Y%m%d%H%M%S)

while IFS= read -r type; do
  ext=$(get_ext "$type")
  filepath="${OUTPUT_DIR}/${NOTEBOOK_NAME}-${type}-${TIMESTAMP}.${ext}"

  rotate_files "$OUTPUT_DIR" "$NOTEBOOK_NAME" "$type" "$ext"

  $NLM download "$type" "$filepath" -n "$NOTEBOOK_ID" --latest --force > /dev/null 2>&1 \
    && echo "  완료: $(basename "$filepath")" \
    || echo "  경고: $type 다운로드 실패"

  [[ "$ext" == "png" && -f "$filepath" ]] && resize_if_needed "$filepath"
done < "$WORK/types"

# ── 결과 출력 ──────────────────────────────────────────────
echo ""
echo "=== 완료 ==="
echo "노트북 ID : $NOTEBOOK_ID"
echo "저장 위치 : $OUTPUT_DIR"
echo ""
echo "최신 파일:"
while IFS= read -r type; do
  ext=$(get_ext "$type")
  latest=$(ls -t "${OUTPUT_DIR}/${NOTEBOOK_NAME}-${type}"-*.${ext} 2>/dev/null | head -1 || true)
  [[ -n "$latest" ]] && echo "  [$type] $latest"
done < "$WORK/types"
