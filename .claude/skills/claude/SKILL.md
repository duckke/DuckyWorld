---
name: claude-code-sync
description: 클로드 코드 동기화. "클로드 코드 동기화", "새 기기 설정" 등 요청 시 트리거.
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
- Claude가 알림을 보고 **사용자에게 커밋 여부를 물어볼 것**
- 동의 시: `/tmp/claude_pending_push.json` 내용 → `.claude/skills/claude/settings.json` 저장 → 커밋 & 푸시 → 파일 삭제
- 거부 시: `/tmp/claude_pending_push.json` 삭제
