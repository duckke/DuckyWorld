---
name: duck-notebooklm
description: NotebookLM 작업을 노트북이 에이전트와 대화하며 진행한다.
---

# NotebookLM 스킬

에레미를 호출한다. Agent 툴을 `subagent_type: "에레미"`로 호출한다.

## 진입 방식에 따른 동작

**오케스트레이터에서 위임된 경우**: 소스·타입·옵션이 모두 결정된 상태 → 바로 실행

**사용자가 직접 호출한 경우**: 아래 순서로 대화하며 결정
1. 어떤 노트북을 쓸지 (기존 목록 확인 or 새로 생성)
2. 소스 (파일 경로 / URL / YouTube 등)
3. 만들 아티팩트 타입
4. 타입별 옵션 (`notebooklm generate <type> --help` 확인 후 제안)
5. 소스와 아티팩트 매핑 (어떤 소스로 어떤 아티팩트를 만들지)
6. 확정 후 실행
