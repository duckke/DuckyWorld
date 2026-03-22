#!/usr/bin/env bash
# UserPromptSubmit 훅 - 버전 비교 후 AUTO-SYNC / AUTO-APPLY 주입

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
SETTINGS="$SKILL_DIR/settings"

ver_gt() {
  [ "$(printf '%s\n' "$1" "$2" | sort -V | tail -1)" = "$1" ] && [ "$1" != "$2" ]
}

# Case 1: 로컬 변경사항 pending → AUTO-SYNC
if [ -f /tmp/claude_pending_push.txt ]; then
  NEW_VER=$(cat /tmp/claude_pending_push.txt)
  echo "[AUTO-SYNC] settings이 변경되었습니다 (→ v${NEW_VER}). 사용자에게 커밋 여부를 물어보세요. 동의 시: git add & commit & push. 거부 시: /tmp/claude_pending_push.txt 삭제."
  exit 0
fi

# Case 2: settings/ 버전이 로컬보다 높음 → 즉시 적용
SETTINGS_VER=$(cat "$SETTINGS/settings.version" 2>/dev/null || echo "0")
LOCAL_VER=$(cat ~/.claude/settings.version 2>/dev/null || echo "0")

if ver_gt "$SETTINGS_VER" "$LOCAL_VER"; then
  bash "$SKILL_DIR/sync.sh" 2>/dev/null
  echo "[AUTO-APPLIED] Claude 설정이 자동 적용되었습니다 (v${LOCAL_VER} → v${SETTINGS_VER}). Claude Code 재시작이 필요할 수 있습니다."
fi
