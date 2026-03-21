---
name: claude-code-sync
description: Claude Code 환경을 현재 기기에 동기화. "클로드 코드 동기화", "Claude Code 세팅 맞춰줘", "상태바 설정", "새 기기 설정" 등 요청 시 트리거.
---

# Claude Code 환경 동기화 스킬

새 기기 또는 기존 기기에 Claude Code 환경(상태바, 훅, 자동업데이트)을 동기화한다.

## 포함 설정

| 항목 | 내용 |
|------|------|
| 상태바 | 모델명, 컨텍스트 게이지, DAILY/WEEKLY 이용량, 마지막 토큰 |
| 자동 업데이트 | 세션 시작 시 `claude update` 백그라운드 실행 |
| jq | 상태바 스크립트 의존성 |
| 심볼릭 링크 | nvm 환경에서 최신 Claude 버전으로 링크 자동 수정 |

## 실행 방법

```bash
bash .claude/skills/etc/claude-code-sync/sync.sh
```

## 워크플로우

1. sync.sh를 Bash 툴로 실행:
   ```bash
   bash /Users/$(whoami)/Documents/ClaudeTest/.claude/skills/etc/claude-code-sync/sync.sh
   ```
2. 완료 후 사용자에게 Claude Code 재시작 안내

## 스킬 파일 구조

```
claude-code-sync/
├── SKILL.md              ← 이 파일
├── sync.sh               ← 동기화 실행 스크립트
└── statusline-command.sh ← 상태바 스크립트 (정본)
```

## 설정 업데이트 시

상태바 스크립트나 settings 변경 시:
1. `statusline-command.sh` 또는 `sync.sh` 수정
2. 커밋 & 푸시
3. 다른 기기에서 `git pull` 후 "클로드 코드 동기화" 요청
