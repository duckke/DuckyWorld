#!/usr/bin/env bash
# UserPromptSubmit 훅 - settings 변경 감지 시 Claude에게 알림 주입

if [ -f /tmp/claude_pending_push.json ]; then
  echo "[AUTO-SYNC] ~/.claude/settings.json이 변경되었습니다. 사용자에게 커밋 여부를 물어보세요."
fi
