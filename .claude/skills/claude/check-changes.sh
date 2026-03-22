#!/usr/bin/env bash
# Stop 훅 - settings.json 변경 감지 → 버전 올려서 pending 저장

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJ_SETTINGS="$SKILL_DIR/settings.json"

command -v jq &>/dev/null || exit 0

normalize() {
  sed "s|${SKILL_DIR}|__SKILL_DIR__|g; s|${HOME}/|~/|g"
}

EXCLUDE='del(.model, .statusLine, .__version__)'

CURRENT=$(normalize < ~/.claude/settings.json 2>/dev/null | jq -S "$EXCLUDE" 2>/dev/null || echo "")
SAVED=$(jq -S "$EXCLUDE" "$PROJ_SETTINGS" 2>/dev/null || echo "")

if [ "$CURRENT" != "$SAVED" ]; then
  REPO_VER=$(jq -r '.__version__ // "1.0.0"' "$PROJ_SETTINGS" 2>/dev/null)
  PATCH=$(echo "$REPO_VER" | awk -F. '{print $3+1}')
  NEW_VER=$(echo "$REPO_VER" | awk -F. "{print \$1\".\"\$2\".\"$PATCH}")

  normalize < ~/.claude/settings.json 2>/dev/null \
    | jq -S --arg v "$NEW_VER" "$EXCLUDE | . + {__version__: \$v}" \
    > /tmp/claude_pending_push.json
fi
