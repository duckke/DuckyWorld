---
name: claude-code-sync
description: Claude Code 환경 동기화. "클로드 코드 동기화", "상태바 설정", "새 기기 설정", "설정 저장", "설정 업데이트 푸시" 등 요청 시 트리거.
---

# Claude Code 환경 동기화 스킬

두 가지 방향으로 동작한다.

---

## ① 기기 → 적용 (pull 방향)

다른 기기에서 "클로드 코드 동기화" 요청 시:

1. sync.sh 실행:
   ```bash
   bash .claude/skills/etc/claude-code-sync/sync.sh
   ```
   - jq 설치
   - `~/.claude/statusline-command.sh` 설치
   - `~/.claude/settings.json` 업데이트 (statusLine + SessionStart 훅)
   - Claude Code 최신 버전 링크 및 업데이트

2. 완료 후 Claude Code 재시작 안내

---

## ② 설정 변경 → 저장 & 푸시 (push 방향)

`~/.claude/settings.json` 또는 `~/.claude/statusline-command.sh`를 수정한 후
"설정 저장", "설정 업데이트 푸시" 요청 시:

1. 변경된 설정을 프로젝트에 반영:
   - `~/.claude/settings.json`의 변경 내용 → `sync.sh` 내 설정 블록 업데이트
   - `~/.claude/statusline-command.sh` 변경 시 → `statusline-command.sh` 덮어쓰기:
     ```bash
     cp ~/.claude/statusline-command.sh .claude/skills/etc/claude-code-sync/statusline-command.sh
     ```

2. 커밋 & 푸시 (git-commit 스킬 사용)

---

## 포함 설정

| 항목 | 내용 |
|------|------|
| 상태바 | 모델명, 컨텍스트 게이지, DAILY/WEEKLY 이용량, 마지막 토큰 |
| 자동 업데이트 | 세션 시작 시 `claude update` 백그라운드 실행 |
| jq | 상태바 스크립트 의존성 |
| 심볼릭 링크 | nvm 환경에서 최신 Claude 버전으로 링크 자동 수정 |

---

## 파일 구조

```
claude-code-sync/
├── SKILL.md              ← 이 파일
├── sync.sh               ← 기기에 적용하는 스크립트 (정본)
└── statusline-command.sh ← 상태바 스크립트 (정본)
```
