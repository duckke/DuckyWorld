#!/usr/bin/env bash
# Stop 훅 - settings.json 변경 감지 후 pending 파일 생성

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJ_SETTINGS="$SKILL_DIR/settings.json"

# jq 없으면 비교 스킵
command -v jq &>/dev/null || exit 0

# 절대경로 → 템플릿 형식으로 정규화 (SKILL_DIR 먼저, HOME 나중)
normalize() {
  sed "s|${SKILL_DIR}|__SKILL_DIR__|g; s|${HOME}/|~/|g"
}

CURRENT=$(normalize < ~/.claude/settings.json 2>/dev/null | jq -S 'del(.model)' 2>/dev/null || echo "")
SAVED=$(jq -S . "$PROJ_SETTINGS" 2>/dev/null || echo "")

if [ "$CURRENT" != "$SAVED" ]; then
  echo "$CURRENT" > /tmp/claude_pending_push.json
fi
