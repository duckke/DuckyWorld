---
name: claude-code-sync
description: 새 기기에 Claude Code 환경 적용. "클로드 코드 동기화", "상태바 설정", "새 기기 설정" 등 요청 시 트리거.
---

# Claude Code 환경 동기화

새 기기에서 `sync.sh`를 실행하면 모든 설정이 자동 적용된다.
설정 변경 시에는 세션 종료 시 자동으로 감지해서 커밋 & 푸시된다.

## 새 기기 적용

```bash
bash .claude/skills/etc/claude-code-sync/sync.sh
```

완료 후 Claude Code 재시작 안내.

## 자동 푸시 동작

- `~/.claude/settings.json` 또는 `~/.claude/statusline-command.sh` 변경 감지
- 세션 종료(Stop 훅) 시 변경 내용 자동 커밋 & 푸시
- 별도 요청 불필요

## 파일 구조

```
claude-code-sync/
├── SKILL.md              ← 이 파일
├── settings.json         ← settings.json 정본
├── statusline-command.sh ← 상태바 스크립트 정본
├── sync.sh               ← 새 기기 적용 스크립트
└── auto-push.sh          ← Stop 훅 실행 스크립트 (자동 감지 & 푸시)
```
