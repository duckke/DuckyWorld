---
name: claude-code-sync
description: 클로드 코드 설정을 기기 간 동기화한다.
---

# Claude Code 동기화

## 핵심 원칙

**클로드 설정에 영향을 주는 모든 변경은 반드시 이 repo의 `.claude/skills/claude/` 안에 파일로 반영하고, 즉시 커밋 & 푸시한다.** 다른 기기에서 pull 받으면 자동으로 동기화되어야 한다.

동기화 대상 파일:
- `settings.json` — 클로드 설정값 (훅, 권한, 마켓플레이스 등)
- `statusline-command.sh` — 상태창 스크립트
- `sync.sh` — 동기화 실행 스크립트
- `check-changes.sh` — 변경 감지 스크립트
- `prompt-inject.sh` — 알림 주입 스크립트

## 설정 변경 시 행동 규칙

1. `~/.claude/` 하위 파일을 수정하면 → 대응하는 repo 파일도 즉시 업데이트
2. 새로운 설정 파일이 필요하면 → `.claude/skills/claude/`에 생성하고 `sync.sh`에 복사 로직 추가
3. 변경 후 **묻지 말고 바로** git-commit 스킬로 커밋 & 푸시
4. 사용자가 직접 설정을 바꾼 경우 → Stop 훅이 자동 감지 → AUTO-SYNC로 커밋

## 동기화 플로우

### 설정 변경 → 다른 기기에 전파

```
설정 변경 → Stop 훅(check-changes.sh) → 변경 감지 & 버전 +1 → pending 저장
→ 다음 메시지에서 AUTO-SYNC 알림 → settings.json 저장 → 커밋 & 푸시
→ 다른 기기에서 pull → post-merge 훅이 버전 비교 → sync.sh 자동 실행
```

### 새 기기 초기 설정

```bash
bash .claude/skills/claude/sync.sh
```

완료 후 Claude Code 재시작.

## 알림 동작

- `[AUTO-SYNC]`: 로컬 변경 감지 → 커밋 여부 질문 → 동의 시 커밋 & 푸시
- `[AUTO-APPLIED]`: pull 후 새 버전 감지 → 질문 없이 즉시 sync.sh 실행
