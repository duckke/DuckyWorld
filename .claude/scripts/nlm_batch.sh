#!/usr/bin/env bash
# nlm_batch.sh — NotebookLM 소스 일괄 추가 + 아티팩트 병렬 생성 + 자동 다운로드
#
# 인터페이스:
#   --notebook "제목"            노트북 새로 생성
#   --notebook-id "abc123"       기존 노트북 사용
#   --notebook-name "flap_flap"  폴더명/파일명에 쓸 슬러그 (필수)
#   --output-dir "경로"          다운로드 기본 경로 (기본: docs/notebooklm)
#   --TYPE-sources "s1,s2"       TYPE 아티팩트에 사용할 소스 (쉼표 구분)
#   --TYPE-opts    "플래그들"     TYPE generate 커맨드에 추가할 CLI 옵션
#   --TYPE-prompt  "지시사항"     TYPE generate에 전달할 자연어 지시
#
# 파일명 규칙: {notebook-name}-{type}-{YYYYMMDDHHmmss}.{ext}
# 보관 규칙:   타입별 최대 3개, 초과 시 오래된 것부터 삭제

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

NLM="notebooklm"
NOTEBOOK_TITLE=""
NOTEBOOK_ID=""
NOTEBOOK_NAME=""
OUTPUT_BASE="${PROJECT_ROOT}/.claude/docs/notebooklm"
POLL_INTERVAL=10
SOURCE_TIMEOUT=300
ARTIFACT_TIMEOUT=1800
MAX_FILES_PER_TYPE=3

declare -A TYPE_SOURCES
declare -A TYPE_OPTS
declare -A TYPE_PROMPTS

# 타입 → 확장자 매핑
declare -A TYPE_EXT=(
  ["audio"]="mp3"
  ["video"]="mp4"
  ["cinematic-video"]="mp4"
  ["infographic"]="png"
  ["slide-deck"]="pdf"
  ["report"]="md"
  ["mind-map"]="json"
  ["data-table"]="csv"
  ["quiz"]="json"
  ["flashcards"]="json"
)

# ── 인수 파싱 ──────────────────────────────────────────────
while [[ $# -gt 0 ]]; do
  case "$1" in
    --notebook)      NOTEBOOK_TITLE="$2"; shift 2 ;;
    --notebook-id)   NOTEBOOK_ID="$2";    shift 2 ;;
    --notebook-name) NOTEBOOK_NAME="$2";  shift 2 ;;
    --output-dir)    OUTPUT_BASE="$2";    shift 2 ;;
    --*-sources)
      type="${1#--}"; type="${type%-sources}"
      TYPE_SOURCES["$type"]="$2"; shift 2 ;;
    --*-opts)
      type="${1#--}"; type="${type%-opts}"
      TYPE_OPTS["$type"]="$2"; shift 2 ;;
    --*-prompt)
      type="${1#--}"; type="${type%-prompt}"
      TYPE_PROMPTS["$type"]="$2"; shift 2 ;;
    *) echo "알 수 없는 옵션: $1" >&2; exit 1 ;;
  esac
done

# ── 검증 ──────────────────────────────────────────────────
[[ -z "$NOTEBOOK_TITLE" && -z "$NOTEBOOK_ID" ]] && { echo "오류: --notebook 또는 --notebook-id 필요" >&2; exit 1; }
[[ -z "$NOTEBOOK_NAME" ]] && { echo "오류: --notebook-name 필요" >&2; exit 1; }
[[ ${#TYPE_SOURCES[@]} -eq 0 ]] && { echo "오류: 아티팩트 타입 최소 1개 필요" >&2; exit 1; }

OUTPUT_DIR="${OUTPUT_BASE}/${NOTEBOOK_NAME}"
mkdir -p "$OUTPUT_DIR"

# ── 5MB 초과 시 리사이즈 (원본 덮어쓰기) ──────────────────
NOTION_SIZE_LIMIT=$((4800 * 1024))
resize_if_needed() {
  local file="$1"
  local size
  size=$(stat -f%z "$file" 2>/dev/null || stat -c%s "$file")
  [[ $size -le $NOTION_SIZE_LIMIT ]] && return 0

  local width
  width=$(sips -g pixelWidth "$file" 2>/dev/null | awk '/pixelWidth/{print $2}')
  local new_width
  new_width=$(python3 -c "import math; print(int($width * math.sqrt($NOTION_SIZE_LIMIT / $size) * 0.95))")

  echo "  리사이즈: ${width}px → ${new_width}px ($(( size / 1024 / 1024 ))MB → ~5MB 이하)"
  sips -Z "$new_width" "$file" --out "$file" > /dev/null 2>&1
}

# ── 파일 로테이션 함수 ─────────────────────────────────────
# 같은 타입 파일이 MAX_FILES_PER_TYPE 이상이면 오래된 것 삭제
rotate_files() {
  local dir="$1"
  local name="$2"   # notebook-name
  local type="$3"
  local ext="$4"

  # 최신순 정렬 후 MAX 초과분 삭제 (bash 3.2 호환)
  local files count i delete_count
  files=( $(ls -t "${dir}/${name}-${type}"-*.${ext} 2>/dev/null) )
  count=${#files[@]}
  if [[ $count -ge $MAX_FILES_PER_TYPE ]]; then
    delete_count=$(( count - MAX_FILES_PER_TYPE + 1 ))
    for (( i=count-1; i>=count-delete_count; i-- )); do
      echo "  삭제 (오래된 파일): ${files[$i]}"
      rm -f "${files[$i]}"
    done
  fi
}

# ── 최신 파일 경로 반환 함수 ──────────────────────────────
latest_file() {
  local dir="$1"
  local name="$2"
  local type="$3"
  local ext="$4"
  ls -t "${dir}/${name}-${type}"-*.${ext} 2>/dev/null | head -1
}

# ── 인증 확인 ─────────────────────────────────────────────
echo "[1/5] 인증 확인..."
if ! $NLM auth check --json 2>/dev/null | python3 -c "import sys,json; d=json.load(sys.stdin); exit(0 if d.get('checks',{}).get('sid_cookie') else 1)" 2>/dev/null; then
  echo "오류: 인증 실패. 'notebooklm login' 실행 후 재시도하세요." >&2; exit 1
fi

# ── 노트북 생성 또는 선택 ───────────────────────────────────
if [[ -z "$NOTEBOOK_ID" ]]; then
  echo "[2/5] 노트북 생성: '$NOTEBOOK_TITLE'"
  NOTEBOOK_ID=$($NLM create "$NOTEBOOK_TITLE" --json | python3 -c "import sys,json; print(json.load(sys.stdin)['id'])")
  [[ -z "$NOTEBOOK_ID" ]] && { echo "오류: 노트북 생성 실패" >&2; exit 1; }
  echo "  노트북 ID: $NOTEBOOK_ID"

  # notebooks.json 업데이트
  NB_JSON="${OUTPUT_BASE}/notebooks.json"
  if [[ -f "$NB_JSON" ]]; then
    python3 -c "
import json, sys
data = json.load(open('$NB_JSON'))
data['$NOTEBOOK_NAME'] = '$NOTEBOOK_ID'
json.dump(data, open('$NB_JSON','w'), ensure_ascii=False, indent=2)
"
  else
    echo "{\"$NOTEBOOK_NAME\": \"$NOTEBOOK_ID\"}" | python3 -c "import json,sys; json.dump(json.load(sys.stdin), open('$NB_JSON','w'), ensure_ascii=False, indent=2)"
  fi
  echo "  notebooks.json 업데이트됨"
else
  echo "[2/5] 노트북 ID 사용: $NOTEBOOK_ID"
fi

# ── 소스 수집 (중복 제거) ──────────────────────────────────
declare -a ALL_SOURCES_ARR=()
declare -A SEEN_SOURCES=()
for type in "${!TYPE_SOURCES[@]}"; do
  IFS=',' read -ra items <<< "${TYPE_SOURCES[$type]}"
  for item in "${items[@]}"; do
    item=$(echo "$item" | xargs)
    if [[ -n "$item" && -z "${SEEN_SOURCES[$item]:-}" ]]; then
      ALL_SOURCES_ARR+=("$item")
      SEEN_SOURCES["$item"]=1
    fi
  done
done

# ── 소스 병렬 추가 ─────────────────────────────────────────
echo "[3/5] 소스 병렬 추가 (${#ALL_SOURCES_ARR[@]}개)..."
TMPDIR_SRC=$(mktemp -d)

for src in "${ALL_SOURCES_ARR[@]}"; do
  (
    result=$($NLM source add "$src" --notebook "$NOTEBOOK_ID" --json 2>/dev/null)
    sid=$(echo "$result" | python3 -c "import sys,json; print(json.load(sys.stdin).get('source_id',''))" 2>/dev/null || echo "")
    safe_key=$(echo "$src" | tr '/: ' '___')
    echo "${src}=${sid}" > "$TMPDIR_SRC/${safe_key}.id"
    echo "  추가됨: $(basename "$src") → $sid"
  ) &
done
wait

declare -A SRC_TO_ID=()
for f in "$TMPDIR_SRC"/*.id; do
  [[ -f "$f" ]] || continue
  line=$(cat "$f")
  SRC_TO_ID["${line%%=*}"]="${line##*=}"
done
rm -rf "$TMPDIR_SRC"

# ── 소스 처리 완료 대기 ────────────────────────────────────
echo "  소스 처리 대기 중..."
ELAPSED=0
while true; do
  NOT_READY=$($NLM source list --notebook "$NOTEBOOK_ID" --json 2>/dev/null \
    | python3 -c "import sys,json; srcs=json.load(sys.stdin).get('sources',[]); print(sum(1 for s in srcs if s.get('status')!='ready'))")
  [[ "$NOT_READY" -eq 0 ]] && { echo "  모든 소스 ready"; break; }
  [[ $ELAPSED -ge $SOURCE_TIMEOUT ]] && { echo "오류: 소스 처리 타임아웃" >&2; exit 1; }
  sleep $POLL_INTERVAL
  ELAPSED=$((ELAPSED + POLL_INTERVAL))
done

# ── 아티팩트 병렬 생성 ─────────────────────────────────────
echo "[4/5] 아티팩트 병렬 생성..."
TMPDIR_ART=$(mktemp -d)

for type in "${!TYPE_SOURCES[@]}"; do
  (
    src_flags=""
    IFS=',' read -ra srcs <<< "${TYPE_SOURCES[$type]}"
    for s in "${srcs[@]}"; do
      s=$(echo "$s" | xargs)
      sid="${SRC_TO_ID[$s]:-}"
      [[ -n "$sid" ]] && src_flags="$src_flags -s $sid"
    done

    extra_opts="${TYPE_OPTS[$type]:-}"
    # 인포그래픽 기본 옵션: 정사각형 + 스케치노트 스타일 + 한국어
    [[ "$type" == "infographic" && -z "$extra_opts" ]] && extra_opts="--orientation square --style sketch-note --language ko"
    prompt_arg="${TYPE_PROMPTS[$type]:-}"
    cmd="$NLM generate $type --notebook $NOTEBOOK_ID $src_flags $extra_opts --json"
    [[ -n "$prompt_arg" ]] && cmd="$cmd \"$prompt_arg\""

    result=$(eval $cmd 2>/dev/null)
    task_id=$(echo "$result" | python3 -c "import sys,json; print(json.load(sys.stdin).get('task_id',''))" 2>/dev/null || echo "")
    echo "$type=$task_id" > "$TMPDIR_ART/${type}.tid"
    echo "  생성 시작: $type → $task_id"
  ) &
done
wait

# ── 아티팩트 완료 대기 ─────────────────────────────────────
echo "  아티팩트 완료 대기 중 (최대 ${ARTIFACT_TIMEOUT}s)..."
ELAPSED=0
while true; do
  IN_PROGRESS=$($NLM artifact list --notebook "$NOTEBOOK_ID" --json 2>/dev/null \
    | python3 -c "import sys,json; arts=json.load(sys.stdin).get('artifacts',[]); print(sum(1 for a in arts if a.get('status') in ('in_progress','pending')))")
  [[ "$IN_PROGRESS" -eq 0 ]] && break
  [[ $ELAPSED -ge $ARTIFACT_TIMEOUT ]] && { echo "경고: 타임아웃. 확인: notebooklm artifact list --notebook $NOTEBOOK_ID" >&2; break; }
  sleep 30
  ELAPSED=$((ELAPSED + 30))
done

# ── 다운로드 (타임스탬프 파일명 + 로테이션) ────────────────
echo "[5/5] 다운로드..."
TIMESTAMP=$(date +%Y%m%d%H%M%S)
declare -a DOWNLOADED=()

# 완료된 아티팩트 목록 조회
ART_JSON=$($NLM artifact list --notebook "$NOTEBOOK_ID" --json 2>/dev/null)

for type in "${!TYPE_SOURCES[@]}"; do
  ext="${TYPE_EXT[$type]:-bin}"
  filename="${NOTEBOOK_NAME}-${type}-${TIMESTAMP}.${ext}"
  filepath="${OUTPUT_DIR}/${filename}"

  # artifact ID 조회 (해당 타입 중 가장 최신 completed)
  artifact_id=$(echo "$ART_JSON" | python3 -c "
import sys, json
arts = json.load(sys.stdin).get('artifacts', [])
matched = [a for a in arts if a.get('type','').lower().replace(' ','') in ('$type'.replace('-','').lower(), '$type'.lower()) and a.get('status') == 'completed']
matched.sort(key=lambda x: x.get('created_at',''), reverse=True)
print(matched[0]['id'] if matched else '')
" 2>/dev/null || echo "")

  if [[ -z "$artifact_id" ]]; then
    echo "  경고: $type 완료된 아티팩트 없음, 건너뜀"
    continue
  fi

  # 로테이션: 기존 파일 MAX 초과 시 오래된 것 삭제
  rotate_files "$OUTPUT_DIR" "$NOTEBOOK_NAME" "$type" "$ext"

  # 다운로드
  $NLM download "$type" "$filepath" -a "$artifact_id" -n "$NOTEBOOK_ID" 2>/dev/null \
    && echo "  다운로드 완료: $filename" \
    || echo "  경고: $type 다운로드 실패"

  # PNG인 경우 5MB 초과 시 리사이즈 (원본 덮어쓰기)
  [[ "$ext" == "png" ]] && resize_if_needed "$filepath"

  DOWNLOADED+=("$filepath")
done

rm -rf "$TMPDIR_ART"

# ── 결과 출력 ──────────────────────────────────────────────
echo ""
echo "=== 완료 ==="
echo "노트북 ID : $NOTEBOOK_ID"
echo "저장 위치 : $OUTPUT_DIR"
echo ""
echo "생성된 파일:"
for f in "${DOWNLOADED[@]}"; do
  echo "  $f"
done

echo ""
echo "최신 파일 경로 (참조용):"
for type in "${!TYPE_SOURCES[@]}"; do
  ext="${TYPE_EXT[$type]:-bin}"
  latest=$(latest_file "$OUTPUT_DIR" "$NOTEBOOK_NAME" "$type" "$ext")
  [[ -n "$latest" ]] && echo "  [$type] $latest"
done
