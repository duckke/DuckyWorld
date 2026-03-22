#!/usr/bin/env bash
# Stop 훅 - ~/.claude/ 변경 감지 → 버전 올려서 settings/ 갱신 후 pending 저장

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
SETTINGS="$SKILL_DIR/settings"

command -v jq &>/dev/null || exit 0

normalize() {
  sed "s|${SKILL_DIR}|__SKILL_DIR__|g; s|${HOME}/|~/|g"
}

CHANGED=false

# ── 1. settings.json 비교 ────────────────────────────────────────────────

CURRENT=$(normalize < ~/.claude/settings.json 2>/dev/null | jq -S '.' 2>/dev/null || echo "")
SAVED=$(jq -S '.' "$SETTINGS/settings.json" 2>/dev/null || echo "")

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

# ── 5. 변경 있으면 버전 올리고 settings/ 갱신 ────────────────────────────

if [ "$CHANGED" = true ]; then
  CUR_VER=$(cat ~/.claude/settings.version 2>/dev/null || echo "1.0.0")
  PATCH=$(echo "$CUR_VER" | awk -F. '{print $3+1}')
  NEW_VER=$(echo "$CUR_VER" | awk -F. "{print \$1\".\"\$2\".\"$PATCH}")

  # 버전 파일 갱신
  echo "$NEW_VER" > ~/.claude/settings.version
  echo "$NEW_VER" > "$SETTINGS/settings.version"

  # settings.json 갱신
  normalize < ~/.claude/settings.json | jq -S '.' > "$SETTINGS/settings.json"

  # *.sh 파일 갱신
  find "$HOME/.claude" -maxdepth 1 -type f -name "*.sh" -exec cp {} "$SETTINGS/" \;

  # keybindings.json 갱신
  [ -f ~/.claude/keybindings.json ] && cp ~/.claude/keybindings.json "$SETTINGS/"

  # pending 마킹
  echo "$NEW_VER" > /tmp/claude_pending_push.txt
fi
