#!/usr/bin/env bash
# 새 기기에 Claude Code 환경 적용

set -e

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_DIR="$(cd "$SKILL_DIR/../../.." && pwd)"
FILES_DIR="$SKILL_DIR/files"

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

# files/ 디렉토리 전체를 ~/.claude/ 에 복사
if [ -d "$FILES_DIR" ]; then
  while IFS= read -r -d '' repo_file; do
    rel="${repo_file#$FILES_DIR/}"
    dest="$HOME/.claude/$rel"
    mkdir -p "$(dirname "$dest")"
    cp "$repo_file" "$dest"
  done < <(find "$FILES_DIR" -type f -print0)
  echo "✅ files/ 동기화 완료"
fi

# post-merge 훅 설치 - git pull 후 자동 버전 체크 & 적용
HOOK_FILE="$REPO_DIR/.git/hooks/post-merge"
cat > "$HOOK_FILE" << HOOK
#!/usr/bin/env bash
# git pull 후 Claude Code settings 버전 체크 & 자동 적용

command -v jq &>/dev/null || exit 0

SKILL_DIR="${SKILL_DIR}"
GIT_VER=\$(jq -r '.__version__ // "0"' "\$SKILL_DIR/settings.json" 2>/dev/null)
LOCAL_VER=\$(jq -r '.__version__ // "0"' ~/.claude/settings.json 2>/dev/null)

ver_gt() { [ "\$(printf '%s\n' "\$1" "\$2" | sort -V | tail -1)" = "\$1" ] && [ "\$1" != "\$2" ]; }

if ver_gt "\$GIT_VER" "\$LOCAL_VER"; then
  echo "🔄 Claude settings 업데이트 감지 (v\${LOCAL_VER} → v\${GIT_VER}), 적용 중..."
  bash "\$SKILL_DIR/sync.sh"
fi
HOOK
chmod +x "$HOOK_FILE"
echo "✅ post-merge 훅 설치 완료"

# Claude Code 최신화 (nvm 환경)
NVM_CLAUDE=$(find "${HOME}/.nvm/versions" -name "claude" -path "*/bin/claude" 2>/dev/null | sort -V | tail -1)
LOCAL_CLAUDE="${HOME}/.local/bin/claude"
if [ -n "$NVM_CLAUDE" ] && [ -L "$LOCAL_CLAUDE" ]; then
  ln -sf "$NVM_CLAUDE" "$LOCAL_CLAUDE"
fi
claude update 2>/dev/null || true

echo "✨ 완료! Claude Code를 재시작해주세요."
