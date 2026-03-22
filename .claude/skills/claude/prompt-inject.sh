#!/usr/bin/env bash
# UserPromptSubmit 훅 - 버전 비교 후 AUTO-SYNC / AUTO-APPLY 주입

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
SETTINGS="$SKILL_DIR/settings"

ver_gt() {
  [ "$(printf '%s\n' "$1" "$2" | sort -V | tail -1)" = "$1" ] && [ "$1" != "$2" ]
}

# 파일명 마이그레이션 (settings.version → settings.version.json)
[ -f ~/.claude/settings.version ] && [ ! -f ~/.claude/settings.version.json ] && mv ~/.claude/settings.version ~/.claude/settings.version.json
[ -f "$SETTINGS/settings.version.json" ] && [ ! -f "$SETTINGS/settings.version.json" ] && mv "$SETTINGS/settings.version.json" "$SETTINGS/settings.version.json"

# Case 0: Claude Code 업데이트 알림
if [ -f /tmp/claude_update_notify.txt ]; then
  VERSIONS=$(cat /tmp/claude_update_notify.txt)
  rm /tmp/claude_update_notify.txt
  echo "[UPDATE] Claude Code가 업데이트되었습니다 (v${VERSIONS}). 새 버전을 적용하려면 세션을 재시작하세요."
  exit 0
fi

# Case 1: 로컬 변경사항 pending → AUTO-SYNC
if [ -f /tmp/claude_pending_push.txt ]; then
  NEW_VER=$(cat /tmp/claude_pending_push.txt)
  echo "[AUTO-SYNC] settings이 변경되었습니다 (→ v${NEW_VER}). 사용자에게 커밋 여부를 물어보세요. 동의 시: git add & commit & push. 거부 시: /tmp/claude_pending_push.txt 삭제."
  exit 0
fi

# Case 2: settings/ 버전이 로컬보다 높음 → 즉시 적용
# sync.sh 실행 중이면 버전 비교 건너뜀 (lock 파일 확인)
if [ -f /tmp/claude_sync_running.txt ]; then
  exit 0
fi

SETTINGS_VER=$(cat "$SETTINGS/settings.version.json" 2>/dev/null || echo "0")
LOCAL_VER=$(cat ~/.claude/settings.version.json 2>/dev/null || echo "0")

if ver_gt "$SETTINGS_VER" "$LOCAL_VER"; then
  bash "$SKILL_DIR/sync.sh" 2>/dev/null
  echo "[AUTO-APPLIED] Claude 설정이 자동 적용되었습니다 (v${LOCAL_VER} → v${SETTINGS_VER}). Claude Code 재시작이 필요할 수 있습니다."
fi
