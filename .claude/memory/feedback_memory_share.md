---
name: 기억은 항상 Git 공유 포함
description: 사용자가 기억하라고 하면 로컬 메모리 + Git 공유 메모리(.claude/memory/) 양쪽 모두 저장
type: feedback
---

기억을 저장할 때 로컬(`~/.claude/projects/.../memory/`)뿐 아니라 **Git 추적 경로(`.claude/memory/`)에도 동일하게 저장**한다.

**Why:** 다른 PC에서도 같은 메모리를 공유해야 하므로, 기억 = 기억공유가 기본이다.
**How to apply:** 메모리 파일 생성/수정 시 → 로컬 경로 + `.claude/memory/` 양쪽에 쓰기. 커밋 대상에 `.claude/memory/` 포함.
