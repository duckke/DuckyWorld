#!/usr/bin/env bash
# Claude Code 환경 동기화 스크립트
# 새 기기에서 실행하면 모든 설정이 자동 적용됨

set -e

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
USERNAME=$(whoami)
CLAUDE_DIR="/Users/$USERNAME/.claude"

echo "🔄 Claude Code 환경 동기화 시작..."

# 1. jq 설치 확인
if ! command -v jq &>/dev/null; then
  echo "📦 jq 설치 중..."
  brew install jq
else
  echo "✅ jq 이미 설치됨"
fi

# 2. statusline 스크립트 설치
echo "📊 상태바 스크립트 설치 중..."
cp "$SKILL_DIR/statusline-command.sh" "$CLAUDE_DIR/statusline-command.sh"
chmod +x "$CLAUDE_DIR/statusline-command.sh"
echo "✅ 상태바 스크립트 설치 완료"

# 3. settings.json 업데이트
echo "⚙️  settings.json 업데이트 중..."
SETTINGS_FILE="$CLAUDE_DIR/settings.json"

DESIRED_SETTINGS=$(cat <<EOF
{
  "statusLine": {
    "type": "command",
    "command": "bash /Users/$USERNAME/.claude/statusline-command.sh"
  },
  "hooks": {
    "SessionStart": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "claude update 2>/dev/null || true",
            "async": true
          }
        ]
      }
    ]
  }
}
EOF
)

if [ -f "$SETTINGS_FILE" ]; then
  # 기존 파일에 병합 (statusLine, hooks만 덮어씀, 나머지는 유지)
  MERGED=$(jq --argjson new "$DESIRED_SETTINGS" '. * $new' "$SETTINGS_FILE")
  echo "$MERGED" > "$SETTINGS_FILE"
else
  echo "$DESIRED_SETTINGS" > "$SETTINGS_FILE"
fi
echo "✅ settings.json 업데이트 완료"

# 4. Claude Code 버전 최신화 (nvm 환경인 경우)
echo "🚀 Claude Code 업데이트 확인 중..."
if command -v nvm &>/dev/null || [ -d "$HOME/.nvm" ]; then
  # nvm 환경: 링크가 구버전을 가리키면 최신으로 교체
  NVM_CLAUDE=$(find "$HOME/.nvm/versions" -name "claude" -path "*/bin/claude" 2>/dev/null | sort -V | tail -1)
  LOCAL_CLAUDE="$HOME/.local/bin/claude"
  if [ -n "$NVM_CLAUDE" ] && [ -L "$LOCAL_CLAUDE" ]; then
    ln -sf "$NVM_CLAUDE" "$LOCAL_CLAUDE"
    echo "✅ Claude 심볼릭 링크 최신화 완료: $NVM_CLAUDE"
  fi
fi
claude update 2>/dev/null || true
echo "✅ Claude Code 업데이트 완료"

echo ""
echo "✨ 동기화 완료! Claude Code를 재시작해주세요."
