#!/bin/bash
# nlm_single.sh — 단일 기획서 인포그래픽 생성 스크립트 (bash 3.2 호환)
# Usage: ./nlm_single.sh <notebook_id> <notebook_name> <source_file> <output_dir>

set -euo pipefail

NOTEBOOK_ID="$1"
NOTEBOOK_NAME="$2"
SOURCE_FILE="$3"
OUTPUT_DIR="$4"

NLM="notebooklm"
POLL_INTERVAL=15
SOURCE_TIMEOUT=300
ARTIFACT_TIMEOUT=1800
TIMESTAMP=$(date +%Y%m%d%H%M%S)

mkdir -p "$OUTPUT_DIR"

echo "[${NOTEBOOK_NAME}] 소스 추가: $SOURCE_FILE"
SOURCE_RESULT=$($NLM source add "$SOURCE_FILE" --notebook "$NOTEBOOK_ID" --json 2>/dev/null)
SOURCE_ID=$(echo "$SOURCE_RESULT" | python3 -c "import sys,json; print(json.load(sys.stdin).get('source_id',''))" 2>/dev/null || echo "")
echo "[${NOTEBOOK_NAME}] 소스 ID: $SOURCE_ID"

# 소스 처리 대기
echo "[${NOTEBOOK_NAME}] 소스 처리 대기..."
ELAPSED=0
while true; do
  NOT_READY=$($NLM source list --notebook "$NOTEBOOK_ID" --json 2>/dev/null \
    | python3 -c "import sys,json; srcs=json.load(sys.stdin).get('sources',[]); print(sum(1 for s in srcs if s.get('status')!='ready'))" 2>/dev/null || echo "1")
  [ "$NOT_READY" -eq 0 ] && break
  [ $ELAPSED -ge $SOURCE_TIMEOUT ] && { echo "[${NOTEBOOK_NAME}] 소스 타임아웃"; exit 1; }
  sleep $POLL_INTERVAL
  ELAPSED=$((ELAPSED + POLL_INTERVAL))
done
echo "[${NOTEBOOK_NAME}] 소스 ready"

# 인포그래픽 생성
echo "[${NOTEBOOK_NAME}] 인포그래픽 생성 시작..."
GEN_RESULT=$($NLM generate infographic \
  --notebook "$NOTEBOOK_ID" \
  --orientation square \
  --style sketch-note \
  --language ko \
  --json 2>/dev/null)
TASK_ID=$(echo "$GEN_RESULT" | python3 -c "import sys,json; print(json.load(sys.stdin).get('task_id',''))" 2>/dev/null || echo "")
echo "[${NOTEBOOK_NAME}] 태스크 ID: $TASK_ID"

# 생성 완료 대기
echo "[${NOTEBOOK_NAME}] 인포그래픽 완료 대기..."
ELAPSED=0
while true; do
  IN_PROGRESS=$($NLM artifact list --notebook "$NOTEBOOK_ID" --json 2>/dev/null \
    | python3 -c "import sys,json; arts=json.load(sys.stdin).get('artifacts',[]); print(sum(1 for a in arts if a.get('status') in ('in_progress','pending')))" 2>/dev/null || echo "1")
  [ "$IN_PROGRESS" -eq 0 ] && break
  [ $ELAPSED -ge $ARTIFACT_TIMEOUT ] && { echo "[${NOTEBOOK_NAME}] 아티팩트 타임아웃"; break; }
  sleep 30
  ELAPSED=$((ELAPSED + 30))
done

# 다운로드
FILEPATH="${OUTPUT_DIR}/${NOTEBOOK_NAME}-infographic-${TIMESTAMP}.png"
echo "[${NOTEBOOK_NAME}] 다운로드: $FILEPATH"
$NLM download infographic "$FILEPATH" --notebook "$NOTEBOOK_ID" 2>/dev/null \
  && echo "[${NOTEBOOK_NAME}] 다운로드 완료: $FILEPATH" \
  || { echo "[${NOTEBOOK_NAME}] 다운로드 실패"; exit 1; }

# 5MB 초과 시 리사이즈
NOTION_SIZE_LIMIT=$((4800 * 1024))
SIZE=$(stat -f%z "$FILEPATH" 2>/dev/null || stat -c%s "$FILEPATH" 2>/dev/null || echo 0)
if [ "$SIZE" -gt "$NOTION_SIZE_LIMIT" ]; then
  WIDTH=$(sips -g pixelWidth "$FILEPATH" 2>/dev/null | awk '/pixelWidth/{print $2}')
  NEW_WIDTH=$(python3 -c "import math; print(int($WIDTH * math.sqrt($NOTION_SIZE_LIMIT / $SIZE) * 0.95))")
  echo "[${NOTEBOOK_NAME}] 리사이즈: ${WIDTH}px → ${NEW_WIDTH}px"
  sips -Z "$NEW_WIDTH" "$FILEPATH" --out "$FILEPATH" > /dev/null 2>&1
fi

echo "[${NOTEBOOK_NAME}] === 완료 ==="
echo "INFOGRAPHIC_PATH=$FILEPATH"
