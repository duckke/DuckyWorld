#!/usr/bin/env bash
# 크론용 자동 업데이트 스크립트 - 버전 변경 시 notify 파일 생성

CLAUDE_BIN=$(which claude)
NOTIFY_FILE="/tmp/claude_update_notify.txt"

BEFORE=$("$CLAUDE_BIN" --version 2>/dev/null | grep -oE '[0-9]+\.[0-9]+\.[0-9]+' | head -1)

"$CLAUDE_BIN" update >> /tmp/claude_update.log 2>&1

AFTER=$("$CLAUDE_BIN" --version 2>/dev/null | grep -oE '[0-9]+\.[0-9]+\.[0-9]+' | head -1)

if [ "$BEFORE" != "$AFTER" ]; then
  echo "${BEFORE}→${AFTER}" > "$NOTIFY_FILE"
fi
