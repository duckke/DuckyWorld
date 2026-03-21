#!/usr/bin/env bash
# UserPromptSubmit 훅 - settings 변경 감지 시 Claude에게 알림 주입

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJ_SETTINGS="$SKILL_DIR/settings.json"

# Case 1: 로컬 변경사항 있음 → 커밋 권유 (우선순위 높음)
if [ -f /tmp/claude_pending_push.json ]; then
  echo "[AUTO-SYNC] ~/.claude/settings.json이 변경되었습니다. 사용자에게 커밋 여부를 물어보세요."
  exit 0
fi

# Case 2: 리포지토리 설정이 로컬과 다름 → 적용 권유 (git pull 후 감지)
# jq 없으면 비교 스킵
command -v jq &>/dev/null || exit 0

normalize() {
  sed "s|${SKILL_DIR}|__SKILL_DIR__|g; s|${HOME}/|~/|g"
}

CURRENT=$(normalize < ~/.claude/settings.json 2>/dev/null | jq -S 'del(.model)' 2>/dev/null || echo "")
SAVED=$(jq -S . "$PROJ_SETTINGS" 2>/dev/null || echo "")

if [ "$CURRENT" != "$SAVED" ]; then
  echo "[AUTO-APPLY] 리포지토리의 settings.json이 현재 설정과 다릅니다. 사용자에게 새로운 설정을 적용할지 물어보세요."
fi
