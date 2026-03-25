#!/usr/bin/env bash
# Stop 훅 - ~/.claude/ 변경 감지 → 버전 올려서 settings/ 갱신 후 pending 저장

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
SETTINGS="$SKILL_DIR/settings"

command -v jq &>/dev/null || exit 0

source "$SKILL_DIR/common.sh"

# 파일명 마이그레이션
migrate_version_files "$SETTINGS"

normalize() {
  sed "s|${SKILL_DIR}|__SKILL_DIR__|g; s|${HOME}/|~/|g"
}

CHANGED=false

# ── 1. settings.json 비교 ────────────────────────────────────────────────

# model 키는 기기별 설정이므로 비교에서 제외
CURRENT=$(normalize < ~/.claude/settings.json 2>/dev/null | jq -S 'del(.model)' 2>/dev/null || echo "")
SAVED=$(jq -S 'del(.model)' "$SETTINGS/settings.json" 2>/dev/null || echo "")

if [ "$CURRENT" != "$SAVED" ]; then
  CHANGED=true
fi

# ── 2. *.sh 파일 비교 ────────────────────────────────────────────────────

while IFS= read -r -d '' local_file; do
  filename="$(basename "$local_file")"
  repo_file="$SETTINGS/$filename"

  if [ ! -f "$repo_file" ] || ! diff -q "$local_file" "$repo_file" &>/dev/null; then
    CHANGED=true
  fi
done < <(find "$HOME/.claude" -maxdepth 1 -type f -name "*.sh" -print0)

# ── 3. keybindings.json 비교 ─────────────────────────────────────────────

if [ -f ~/.claude/keybindings.json ]; then
  if ! diff -q ~/.claude/keybindings.json "$SETTINGS/keybindings.json" &>/dev/null 2>&1; then
    CHANGED=true
  fi
fi

# ── 4. crontab 비교 ──────────────────────────────────────────────────────

if [ -f "$SETTINGS/crontab" ]; then
  CURRENT_CRON=$(crontab -l 2>/dev/null | awk '/# BEGIN claude-managed/{found=1; next} /# END claude-managed/{found=0} found' | grep -v '^#' | grep -v '^$' || true)
  SAVED_CRON=$(grep -v '^#' "$SETTINGS/crontab" | grep -v '^$' || true)
  if [ "$CURRENT_CRON" != "$SAVED_CRON" ]; then
    CHANGED=true
    # 현재 관리 블록을 settings/crontab에 반영
    crontab -l 2>/dev/null | awk '/# BEGIN claude-managed/{found=1; next} /# END claude-managed/{found=0} found' > "$SETTINGS/crontab"
  fi
fi

# ── 5. 메모리 파일 비교 ───────────────────────────────────────────────────

REPO_DIR="$(cd "$SKILL_DIR/../../.." && pwd)"
REPO_MEMORY="$REPO_DIR/.claude/memory"
# 로컬 메모리 경로: ~/.claude/projects/-경로-형식/memory/
LOCAL_MEMORY="$HOME/.claude/projects/$(echo "$REPO_DIR" | sed 's|/|-|g')/memory"

MEMORY_CHANGED=false
if [ -d "$LOCAL_MEMORY" ]; then
  # 로컬 메모리 → Git 메모리 비교
  mkdir -p "$REPO_MEMORY"
  while IFS= read -r -d '' local_file; do
    filename="$(basename "$local_file")"
    repo_file="$REPO_MEMORY/$filename"
    if [ ! -f "$repo_file" ] || ! diff -q "$local_file" "$repo_file" &>/dev/null; then
      MEMORY_CHANGED=true
      break
    fi
  done < <(find "$LOCAL_MEMORY" -maxdepth 1 -type f -print0)

  # Git 메모리에만 있고 로컬에 없는 파일 체크 (삭제 감지)
  if [ "$MEMORY_CHANGED" = false ] && [ -d "$REPO_MEMORY" ]; then
    while IFS= read -r -d '' repo_file; do
      filename="$(basename "$repo_file")"
      if [ ! -f "$LOCAL_MEMORY/$filename" ]; then
        MEMORY_CHANGED=true
        break
      fi
    done < <(find "$REPO_MEMORY" -maxdepth 1 -type f -print0)
  fi
fi

if [ "$MEMORY_CHANGED" = true ]; then
  CHANGED=true
  # 로컬 메모리 → Git 메모리로 동기화 (삭제 포함)
  mkdir -p "$REPO_MEMORY"
  # 로컬에 실제 파일이 있을 때만 Git 메모리를 비우고 복사
  LOCAL_FILE_COUNT=$(find "$LOCAL_MEMORY" -maxdepth 1 -type f | wc -l | tr -d ' ')
  if [ "$LOCAL_FILE_COUNT" -gt 0 ]; then
    rm -f "$REPO_MEMORY"/*
    find "$LOCAL_MEMORY" -maxdepth 1 -type f -exec cp {} "$REPO_MEMORY/" \;
  fi
fi

# ── 6. 변경 있으면 버전 올리고 settings/ 갱신 ────────────────────────────

if [ "$CHANGED" = true ]; then
  CUR_VER=$(cat ~/.claude/settings.version.json 2>/dev/null || echo "1.0.0")
  PATCH=$(echo "$CUR_VER" | awk -F. '{print $3+1}')
  NEW_VER=$(echo "$CUR_VER" | awk -F. "{print \$1\".\"\$2\".\"$PATCH}")

  # 버전 파일 갱신
  echo "$NEW_VER" > ~/.claude/settings.version.json
  echo "$NEW_VER" > "$SETTINGS/settings.version.json"

  # settings.json 갱신 (model 키 제외)
  normalize < ~/.claude/settings.json | jq -S 'del(.model)' > "$SETTINGS/settings.json"

  # *.sh 파일 갱신
  find "$HOME/.claude" -maxdepth 1 -type f -name "*.sh" -exec cp {} "$SETTINGS/" \;

  # keybindings.json 갱신
  [ -f ~/.claude/keybindings.json ] && cp ~/.claude/keybindings.json "$SETTINGS/"

  # pending 마킹
  echo "$NEW_VER" > /tmp/claude_pending_push.txt
fi
