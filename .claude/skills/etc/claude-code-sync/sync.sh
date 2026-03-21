#!/usr/bin/env bash
# 새 기기에 Claude Code 환경 적용

set -e

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "🔄 Claude Code 환경 동기화 중..."

# 1. jq 설치 확인
if ! command -v jq &>/dev/null; then
  echo "📦 jq 설치 중..."
  brew install jq
fi

# 2. statusline 스크립트 설치
cp "$SKILL_DIR/statusline-command.sh" ~/.claude/statusline-command.sh
chmod +x ~/.claude/statusline-command.sh

# 3. settings.json 적용 (~/를 실제 경로로 치환, 기존 설정과 병합)
SETTINGS_JSON=$(sed "s|~/|${HOME}/|g" "$SKILL_DIR/settings.json")
if [ -f ~/.claude/settings.json ]; then
  echo "$SETTINGS_JSON" | jq --slurpfile cur ~/.claude/settings.json '($cur[0] // {}) * .' > /tmp/claude_settings_tmp.json
  mv /tmp/claude_settings_tmp.json ~/.claude/settings.json
else
  echo "$SETTINGS_JSON" > ~/.claude/settings.json
fi

# 4. Claude Code 최신화 (nvm 환경)
NVM_CLAUDE=$(find "${HOME}/.nvm/versions" -name "claude" -path "*/bin/claude" 2>/dev/null | sort -V | tail -1)
LOCAL_CLAUDE="${HOME}/.local/bin/claude"
if [ -n "$NVM_CLAUDE" ] && [ -L "$LOCAL_CLAUDE" ]; then
  ln -sf "$NVM_CLAUDE" "$LOCAL_CLAUDE"
fi
claude update 2>/dev/null || true

echo "✨ 완료! Claude Code를 재시작해주세요."
