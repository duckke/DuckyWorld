#!/usr/bin/env bash
# Stop 훅 - ~/.claude/ 변경 감지 → 버전 올려서 mirror 갱신 후 pending 저장

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
MIRROR="$SKILL_DIR/mirror"

command -v jq &>/dev/null || exit 0

normalize() {
  sed "s|${SKILL_DIR}|__SKILL_DIR__|g; s|${HOME}/|~/|g"
}

CHANGED=false

# ── 1. settings.json 비교 ─────────────────────────────────────────────────

CURRENT=$(normalize < ~/.claude/settings.json 2>/dev/null | jq -S 'del(.__version__)' 2>/dev/null || echo "")
SAVED=$(jq -S '.' "$MIRROR/settings.json" 2>/dev/null || echo "")

if [ "$CURRENT" != "$SAVED" ]; then
  CHANGED=true
fi

# ── 2. *.sh 파일 비교 ────────────────────────────────────────────────────

while IFS= read -r -d '' local_file; do
  filename="$(basename "$local_file")"
  repo_file="$MIRROR/$filename"

  if [ ! -f "$repo_file" ] || ! diff -q "$local_file" "$repo_file" &>/dev/null; then
    CHANGED=true
  fi
done < <(find "$HOME/.claude" -maxdepth 1 -type f -name "*.sh" -print0)

# ── 3. keybindings.json 비교 ─────────────────────────────────────────────

if [ -f ~/.claude/keybindings.json ]; then
  if ! diff -q ~/.claude/keybindings.json "$MIRROR/keybindings.json" &>/dev/null 2>&1; then
    CHANGED=true
  fi
fi

# ── 4. 변경 있으면 버전 올리고 mirror 갱신 ───────────────────────────────

if [ "$CHANGED" = true ]; then
  CUR_VER=$(cat ~/.claude/.version 2>/dev/null || echo "1.0.0")
  PATCH=$(echo "$CUR_VER" | awk -F. '{print $3+1}')
  NEW_VER=$(echo "$CUR_VER" | awk -F. "{print \$1\".\"\$2\".\"$PATCH}")

  # 버전 파일 갱신
  echo "$NEW_VER" > ~/.claude/.version
  echo "$NEW_VER" > "$MIRROR/.version"

  # settings.json 갱신
  normalize < ~/.claude/settings.json \
    | jq -S 'del(.__version__)' \
    > "$MIRROR/settings.json"

  # *.sh 파일 갱신
  find "$HOME/.claude" -maxdepth 1 -type f -name "*.sh" -exec cp {} "$MIRROR/" \;

  # keybindings.json 갱신
  [ -f ~/.claude/keybindings.json ] && cp ~/.claude/keybindings.json "$MIRROR/"

  # pending 마킹 (버전만 기록)
  echo "$NEW_VER" > /tmp/claude_pending_push.txt
fi
