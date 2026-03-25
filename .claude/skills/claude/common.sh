#!/usr/bin/env bash
# 공통 함수 모음 — sync.sh, check-changes.sh, prompt-inject.sh에서 source

# 버전 비교: ver_gt "1.0.2" "1.0.1" → true
ver_gt() {
  [ "$(printf '%s\n' "$1" "$2" | sort -V | tail -1)" = "$1" ] && [ "$1" != "$2" ]
}

# 파일명 마이그레이션 (settings.version → settings.version.json)
migrate_version_files() {
  local settings_dir="$1"
  [ -f ~/.claude/settings.version ] && [ ! -f ~/.claude/settings.version.json ] && mv ~/.claude/settings.version ~/.claude/settings.version.json || true
  [ -f "$settings_dir/settings.version" ] && [ ! -f "$settings_dir/settings.version.json" ] && mv "$settings_dir/settings.version" "$settings_dir/settings.version.json" || true
}
