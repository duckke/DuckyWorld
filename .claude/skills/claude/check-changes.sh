#!/usr/bin/env bash
# Stop 훅 - settings.json 변경 감지 후 pending 파일 생성

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJ_SETTINGS="$SKILL_DIR/settings.json"

# 절대경로 → 템플릿 형식으로 정규화 (비교 및 저장용)
normalize() {
  sed "s|${HOME}/|~/|g; s|${SKILL_DIR}|__SKILL_DIR__|g"
}

CURRENT=$(normalize < ~/.claude/settings.json 2>/dev/null || echo "")
SAVED=$(cat "$PROJ_SETTINGS" 2>/dev/null || echo "")

if [ "$CURRENT" != "$SAVED" ]; then
  echo "$CURRENT" > /tmp/claude_pending_push.json
fi
