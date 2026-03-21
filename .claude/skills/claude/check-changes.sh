#!/usr/bin/env bash
# Stop 훅 - settings.json 변경 감지 후 pending 파일 생성

PROJ_SETTINGS="$(cd "$(dirname "$0")" && pwd)/settings.json"

normalize() { sed "s|${HOME}/|~/|g"; }

CURRENT=$(normalize < ~/.claude/settings.json 2>/dev/null || echo "")
SAVED=$(cat "$PROJ_SETTINGS" 2>/dev/null || echo "")

if [ "$CURRENT" != "$SAVED" ]; then
  echo "$CURRENT" > /tmp/claude_pending_push.json
fi
