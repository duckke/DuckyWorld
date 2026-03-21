#!/usr/bin/env bash
# 새 기기에 Claude Code 환경 적용

set -e

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "🔄 Claude Code 환경 동기화 중..."

# jq 설치 확인
if ! command -v jq &>/dev/null; then
  echo "📦 jq 설치 중..."
  brew install jq
fi

# settings.json 적용 (__SKILL_DIR__ 및 ~/ 치환 후 기존 설정과 병합)
SETTINGS_JSON=$(sed "s|__SKILL_DIR__|${SKILL_DIR}|g; s|~/|${HOME}/|g" "$SKILL_DIR/settings.json")
if [ -f ~/.claude/settings.json ]; then
  echo "$SETTINGS_JSON" | jq --slurpfile cur ~/.claude/settings.json '($cur[0] // {}) * .' > /tmp/claude_settings_tmp.json
  mv /tmp/claude_settings_tmp.json ~/.claude/settings.json
else
  echo "$SETTINGS_JSON" > ~/.claude/settings.json
fi

# Claude Code 최신화 (nvm 환경)
NVM_CLAUDE=$(find "${HOME}/.nvm/versions" -name "claude" -path "*/bin/claude" 2>/dev/null | sort -V | tail -1)
LOCAL_CLAUDE="${HOME}/.local/bin/claude"
if [ -n "$NVM_CLAUDE" ] && [ -L "$LOCAL_CLAUDE" ]; then
  ln -sf "$NVM_CLAUDE" "$LOCAL_CLAUDE"
fi
claude update 2>/dev/null || true

echo "✨ 완료! Claude Code를 재시작해주세요."
