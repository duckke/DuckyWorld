---
name: claude-code-sync
description: 클로드 코드 설정을 기기 간 동기화한다.
---

# Claude Code 동기화

## 새 기기 적용

```bash
bash .claude/skills/claude/sync.sh
```

완료 후 Claude Code 재시작 안내.

## 설정 변경 알림

- 응답 완료 시마다 Stop 훅 → `check-changes.sh`가 `~/.claude/settings.json` 변경 감지
- 변경 있으면 정규화된 내용을 `/tmp/claude_pending_push.json`에 저장
- 다음 메시지 수신 시 UserPromptSubmit 훅 → `prompt-inject.sh`가 `[AUTO-SYNC]` 알림 자동 주입
- `[AUTO-SYNC]`: 로컬 변경 → 사용자에게 커밋 여부 질문 → 동의 시 pending 내용 저장 후 커밋 & 푸시
- `[AUTO-APPLY]`: 리포 설정이 로컬과 다름 → 사용자에게 적용 여부 질문 → 동의 시 `sync.sh` 실행
