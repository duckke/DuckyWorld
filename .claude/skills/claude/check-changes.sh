#!/usr/bin/env bash
# Stop 훅 - ~/.claude/ 변경 감지 → 버전 올려서 pending 저장

SKILL_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJ_SETTINGS="$SKILL_DIR/settings.json"
FILES_DIR="$SKILL_DIR/files"

command -v jq &>/dev/null || exit 0

CHANGED=false

# ── 1. settings.json 비교 ─────────────────────────────────────────────────

normalize() {
  sed "s|${SKILL_DIR}|__SKILL_DIR__|g; s|${HOME}/|~/|g"
}

EXCLUDE='del(.model, .__version__)'

CURRENT=$(normalize < ~/.claude/settings.json 2>/dev/null | jq -S "$EXCLUDE" 2>/dev/null || echo "")
SAVED=$(jq -S "$EXCLUDE" "$PROJ_SETTINGS" 2>/dev/null || echo "")

if [ "$CURRENT" != "$SAVED" ]; then
  CHANGED=true
fi

# ── 2. files/ 디렉토리 — 기존 파일 변경 감지 ─────────────────────────────

mkdir -p "$FILES_DIR"

while IFS= read -r -d '' repo_file; do
  rel="${repo_file#$FILES_DIR/}"
  local_file="$HOME/.claude/$rel"
  if [ -f "$local_file" ]; then
    if ! diff -q "$local_file" "$repo_file" &>/dev/null; then
      cp "$local_file" "$repo_file"
      CHANGED=true
    fi
  fi
done < <(find "$FILES_DIR" -type f -print0)

# ── 3. ~/.claude/ — 신규 파일 자동 감지 (*.sh, keybindings.json) ──────────

while IFS= read -r -d '' local_file; do
  rel="${local_file#$HOME/.claude/}"
  repo_file="$FILES_DIR/$rel"
  if [ ! -f "$repo_file" ]; then
    mkdir -p "$(dirname "$repo_file")"
    cp "$local_file" "$repo_file"
    CHANGED=true
  fi
done < <(find "$HOME/.claude" -maxdepth 1 -type f \( -name "*.sh" -o -name "keybindings.json" \) -print0)

# ── 4. 변경 있으면 버전 올려서 pending 저장 ──────────────────────────────

if [ "$CHANGED" = true ]; then
  REPO_VER=$(jq -r '.__version__ // "1.0.0"' "$PROJ_SETTINGS" 2>/dev/null)
  PATCH=$(echo "$REPO_VER" | awk -F. '{print $3+1}')
  NEW_VER=$(echo "$REPO_VER" | awk -F. "{print \$1\".\"\$2\".\"$PATCH}")

  normalize < ~/.claude/settings.json 2>/dev/null \
    | jq -S --arg v "$NEW_VER" "$EXCLUDE | . + {__version__: \$v}" \
    > /tmp/claude_pending_push.json
fi
