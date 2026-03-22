#!/usr/bin/env bash
# UserPromptSubmit 훅 - 버전 비교 후 AUTO-SYNC / AUTO-APPLY 주입

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJ_SETTINGS="$SKILL_DIR/settings.json"

command -v jq &>/dev/null || exit 0

ver_gt() {
  [ "$(printf '%s\n' "$1" "$2" | sort -V | tail -1)" = "$1" ] && [ "$1" != "$2" ]
}

# Case 1: 로컬 변경사항 pending → AUTO-SYNC
if [ -f /tmp/claude_pending_push.json ]; then
  PENDING_VER=$(jq -r '.__version__ // "0"' /tmp/claude_pending_push.json)
  echo "[AUTO-SYNC] settings.json이 변경되었습니다 (→ v${PENDING_VER}). 사용자에게 커밋 여부를 물어보세요. 동의 시: pending 내용을 .claude/skills/claude/settings.json에 저장 후 git commit & push. 거부 시: /tmp/claude_pending_push.json 삭제."
  exit 0
fi

# Case 2: 깃 버전이 로컬보다 높음 → AUTO-APPLY
GIT_VER=$(jq -r '.__version__ // "0"' "$PROJ_SETTINGS" 2>/dev/null)
LOCAL_VER=$(jq -r '.__version__ // "0"' ~/.claude/settings.json 2>/dev/null)

if ver_gt "$GIT_VER" "$LOCAL_VER"; then
  echo "[AUTO-APPLY] 깃 settings.json이 더 최신입니다 (로컬 v${LOCAL_VER} → 깃 v${GIT_VER}). 사용자에게 적용 여부를 물어보세요. 동의 시: bash ${SKILL_DIR}/sync.sh 실행."
fi
