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

```bash
whoami  # 유저명 확인
```

`~/.claude/settings.json`을 읽어 `statusLine` 블록을 추가하거나 덮어쓴다.
`command` 경로의 유저명은 `whoami` 결과값으로 자동 치환한다:

```json
"statusLine": {
  "type": "command",
  "command": "bash /Users/CURRENT_USERNAME/.claude/statusline-command.sh"
}
```

- `settings.json`이 이미 있으면 `statusLine` 블록만 병합 (나머지 설정 유지)
- `settings.json`이 없으면 새로 생성

### 3. 완료 안내

설치가 완료되면 사용자에게 Claude Code 재시작을 안내한다.
