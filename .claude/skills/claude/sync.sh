#!/usr/bin/env bash
# 새 기기에 Claude Code 환경 적용

set -e

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_DIR="$(cd "$SKILL_DIR/../../.." && pwd)"
SETTINGS="$SKILL_DIR/settings"

source "$SKILL_DIR/common.sh"

# lock 파일 생성 (sync 실행 중 표시)
echo $$ > /tmp/claude_sync_running.txt
trap 'rm -f /tmp/claude_sync_running.txt' EXIT

# 파일명 마이그레이션
migrate_version_files "$SETTINGS"

# 버전 비교 (완료 메시지 판단용)
SETTINGS_VER=$(cat "$SETTINGS/settings.version.json" 2>/dev/null || echo "0")
LOCAL_VER=$(cat ~/.claude/settings.version.json 2>/dev/null || echo "0")
if ver_gt "$SETTINGS_VER" "$LOCAL_VER"; then
  HAS_UPDATE=1
else
  HAS_UPDATE=0
fi

# jq 설치 확인
if ! command -v jq &>/dev/null; then
  echo "📦 jq 설치 중..."
  brew install jq
fi

# settings.json 적용 (__SKILL_DIR__ 및 ~/ 치환 후 기존 설정과 병합, model 키 보존)
SETTINGS_JSON=$(sed "s|__SKILL_DIR__|${SKILL_DIR}|g; s|~/|${HOME}/|g" "$SETTINGS/settings.json")
if [ -f ~/.claude/settings.json ]; then
  LOCAL_MODEL=$(jq -r '.model // empty' ~/.claude/settings.json 2>/dev/null)
  echo "$SETTINGS_JSON" | jq --slurpfile cur ~/.claude/settings.json '($cur[0] // {}) * .' > /tmp/claude_settings_tmp.json
  # 로컬 model 복원
  if [ -n "$LOCAL_MODEL" ]; then
    jq --arg m "$LOCAL_MODEL" '.model = $m' /tmp/claude_settings_tmp.json > /tmp/claude_settings_tmp2.json
    mv /tmp/claude_settings_tmp2.json /tmp/claude_settings_tmp.json
  fi
  mv /tmp/claude_settings_tmp.json ~/.claude/settings.json
else
  echo "$SETTINGS_JSON" > ~/.claude/settings.json
fi

# settings/ 의 나머지 파일들 복사 (settings.json, crontab 제외)
find "$SETTINGS" -maxdepth 1 -type f ! -name "settings.json" ! -name "crontab" | while read -r f; do
  cp "$f" ~/.claude/
done

# 메모리 동기화: .claude/memory/ → 로컬 메모리 경로
REPO_MEMORY="$REPO_DIR/.claude/memory"
LOCAL_MEMORY="$HOME/.claude/projects/$(echo "$REPO_DIR" | sed 's|/|-|g')/memory"
if [ -d "$REPO_MEMORY" ]; then
  mkdir -p "$LOCAL_MEMORY"
  # Git 메모리 → 로컬 메모리로 동기화
  find "$REPO_MEMORY" -maxdepth 1 -type f -exec cp {} "$LOCAL_MEMORY/" \;
fi

# post-merge 훅 설치 - git pull 후 자동 버전 체크 & 적용
HOOK_FILE="$REPO_DIR/.git/hooks/post-merge"
cat > "$HOOK_FILE" << 'HOOK'
#!/usr/bin/env bash
# git pull 후 Claude Code settings 버전 체크 & 메모리 동기화

REPO_DIR="$(cd "$(dirname "$0")/../.." && pwd)"
SKILL_DIR="$REPO_DIR/.claude/skills/claude"
SETTINGS="$SKILL_DIR/settings"

source "$SKILL_DIR/common.sh"

NEED_SYNC=0

# settings 버전 체크
SETTINGS_VER=$(cat "$SETTINGS/settings.version.json" 2>/dev/null || echo "0")
LOCAL_VER=$(cat ~/.claude/settings.version.json 2>/dev/null || echo "0")
if ver_gt "$SETTINGS_VER" "$LOCAL_VER"; then
  NEED_SYNC=1
fi

# 메모리 변경 체크 (pull로 .claude/memory/ 가 바뀌었는지)
REPO_MEMORY="$REPO_DIR/.claude/memory"
LOCAL_MEMORY="$HOME/.claude/projects/$(echo "$REPO_DIR" | sed 's|/|-|g')/memory"
if [ -d "$REPO_MEMORY" ]; then
  for f in "$REPO_MEMORY"/*; do
    [ -f "$f" ] || continue
    fname="$(basename "$f")"
    if [ ! -f "$LOCAL_MEMORY/$fname" ] || ! diff -q "$f" "$LOCAL_MEMORY/$fname" &>/dev/null; then
      NEED_SYNC=1
      break
    fi
  done
fi

if [ "$NEED_SYNC" -eq 1 ]; then
  bash "$SKILL_DIR/sync.sh"
fi
HOOK
chmod +x "$HOOK_FILE"

# crontab 적용 (settings/crontab 기반)
CRONTAB_FILE="$SETTINGS/crontab"
if [ -f "$CRONTAB_FILE" ]; then
  MANAGED=$(sed "s|__SKILL_DIR__|${SKILL_DIR}|g; s|~/|${HOME}/|g" "$CRONTAB_FILE")
  CURRENT=$(crontab -l 2>/dev/null || true)
  # 기존 관리 블록 제거
  CLEANED=$(echo "$CURRENT" | awk '/# BEGIN claude-managed/{found=1} /# END claude-managed/{found=0; next} !found')
  # 빈 crontab이면 블록 추가하지 않음
  if [ -n "$(echo "$MANAGED" | grep -v '^$')" ]; then
    NEW_CRONTAB="${CLEANED}
# BEGIN claude-managed
${MANAGED}
# END claude-managed"
    echo "$NEW_CRONTAB" | grep -v '^$' | crontab - || true
  else
    # 관리 블록 제거만 적용
    if [ -n "$(echo "$CLEANED" | grep -v '^$')" ]; then
      echo "$CLEANED" | grep -v '^$' | crontab - || true
    else
      crontab -r 2>/dev/null || true
    fi
  fi
fi

# Claude Code 최신화 (nvm 환경)
NVM_CLAUDE=$(find "${HOME}/.nvm/versions" -name "claude" -path "*/bin/claude" 2>/dev/null | sort -V | tail -1)
LOCAL_CLAUDE="${HOME}/.local/bin/claude"
if [ -n "$NVM_CLAUDE" ] && [ -L "$LOCAL_CLAUDE" ]; then
  ln -sf "$NVM_CLAUDE" "$LOCAL_CLAUDE"
fi
claude update 2>/dev/null || true

# 완료 메시지
if [ "$HAS_UPDATE" -eq 1 ]; then
  echo "클로드 설정값을 최신화 했어요!"
else
  echo "클로드 설정값이 최신 버전입니다."
fi
