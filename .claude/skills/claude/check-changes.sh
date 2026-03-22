#!/usr/bin/env bash
# Stop 훅 - settings.json 변경 감지 → 버전 올려서 pending 저장

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJ_SETTINGS="$SKILL_DIR/settings.json"

command -v jq &>/dev/null || exit 0

normalize() {
  sed "s|${SKILL_DIR}|__SKILL_DIR__|g; s|${HOME}/|~/|g"
}

# statusLine, model, __version__ 제외하고 비교
CURRENT=$(normalize < ~/.claude/settings.json 2>/dev/null | jq -S 'del(.model, .statusLine, .__version__)' 2>/dev/null || echo "")
SAVED=$(jq -S 'del(.__version__)' "$PROJ_SETTINGS" 2>/dev/null || echo "")

if [ "$CURRENT" != "$SAVED" ]; then
  # 현재 버전에서 +1
  LOCAL_VER=$(jq -r '.__version__ // "1.0.0"' ~/.claude/settings.json 2>/dev/null)
  PATCH=$(echo "$LOCAL_VER" | awk -F. '{print $3+1}')
  NEW_VER=$(echo "$LOCAL_VER" | awk -F. "{print \$1\".\"\$2\".\"$PATCH}")

  # 버전 포함한 정규화 설정을 pending에 저장
  normalize < ~/.claude/settings.json 2>/dev/null \
    | jq -S --arg v "$NEW_VER" 'del(.model, .statusLine) | . + {__version__: $v}' \
    > /tmp/claude_pending_push.json
fi
