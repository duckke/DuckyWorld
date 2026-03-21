#!/usr/bin/env bash
# 세션 종료 시 자동 실행 (Stop 훅)
# settings.json / statusline-command.sh 변경 감지 → 자동 커밋 & 푸시

PROJ_SYNC="${HOME}/Documents/ClaudeTest/.claude/skills/etc/claude-code-sync"
GIT_ROOT="${HOME}/Documents/ClaudeTest"
CHANGED=0

# username을 ~/로 정규화
normalize() {
  sed "s|${HOME}/|~/|g"
}

# settings.json 비교
CURRENT=$(normalize < ~/.claude/settings.json 2>/dev/null || echo "")
SAVED=$(cat "$PROJ_SYNC/settings.json" 2>/dev/null || echo "")

if [ "$CURRENT" != "$SAVED" ]; then
  normalize < ~/.claude/settings.json > "$PROJ_SYNC/settings.json"
  CHANGED=1
fi

# statusline-command.sh 비교
if ! diff -q ~/.claude/statusline-command.sh "$PROJ_SYNC/statusline-command.sh" &>/dev/null; then
  cp ~/.claude/statusline-command.sh "$PROJ_SYNC/statusline-command.sh"
  CHANGED=1
fi

if [ "$CHANGED" -eq 1 ]; then
  cd "$GIT_ROOT"
  git add .claude/skills/etc/claude-code-sync/
  git commit -m "기타. Claude Code 설정값 자동 업데이트"
  git push
fi
