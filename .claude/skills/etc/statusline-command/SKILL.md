---
name: statusline-command
description: Claude Code 상태바 설정을 현재 기기에 자동 적용. "상태바 설정", "statusline 적용" 등 요청 시 트리거.
---

# Statusline Setup

이 스킬을 실행하면 현재 기기에 Claude Code 상태바를 자동으로 설치한다.

## 실행 절차

다음 단계를 순서대로 자동 실행한다:

### 1. sh 파일 복사

```bash
cp .claude/skills/etc/statusline-command/statusline-command.sh ~/.claude/statusline-command.sh
```

### 2. 현재 유저명으로 settings.json 업데이트

`whoami`로 유저명을 확인한 뒤, **반드시 절대경로**로 command를 설정한다.

```bash
# 기존 settings.json에 statusLine 블록이 있으면 절대경로로 교체
sed -i '' "s|bash ~/.claude/statusline-command.sh|bash /Users/$(whoami)/.claude/statusline-command.sh|" ~/.claude/settings.json
```

settings.json에 statusLine 블록이 없으면 jq로 추가한다:

```bash
USERNAME=$(whoami)
jq --arg cmd "bash /Users/$USERNAME/.claude/statusline-command.sh" \
  '.statusLine = {"type": "command", "command": $cmd}' \
  ~/.claude/settings.json > /tmp/settings_tmp.json && mv /tmp/settings_tmp.json ~/.claude/settings.json
```

- `~/.claude/...` 상대경로는 동작하지 않으므로 반드시 절대경로 사용
- `settings.json`이 없으면 새로 생성:
  ```bash
  echo "{\"statusLine\":{\"type\":\"command\",\"command\":\"bash /Users/$(whoami)/.claude/statusline-command.sh\"}}" > ~/.claude/settings.json
  ```

### 2-1. 적용 확인

```bash
cat ~/.claude/settings.json | grep command
```

`/Users/실제유저명/.claude/statusline-command.sh` 형태로 절대경로가 들어있는지 확인한다.

### 3. 완료 안내

설치가 완료되면 사용자에게 Claude Code 재시작을 안내한다.
