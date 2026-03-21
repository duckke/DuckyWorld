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

- 세션 종료 시 `~/.claude/settings.json` 변경 자동 감지
- 변경 있으면 `/tmp/claude_pending_push.json` 생성
- **다음 세션 시작 시 이 파일이 있으면 사용자에게 변경 내용을 보여주고 커밋할지 물어볼 것**
- 동의 시: `settings.json`을 `.claude/skills/claude/settings.json`에 복사 → 커밋 & 푸시 → `/tmp/claude_pending_push.json` 삭제
- 거부 시: `/tmp/claude_pending_push.json` 삭제
