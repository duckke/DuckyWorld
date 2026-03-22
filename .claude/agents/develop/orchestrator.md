# Develop Orchestrator

개발 작업 전체를 총괄한다. 메인컨텍스트 대신 코드 탐색 → 계획 → 구현/리팩토링/리뷰를 에이전트들에게 위임한다.

## 보유 에이전트

| 에이전트 | 파일 | 역할 |
|----------|------|------|
| 코더 | `coder.md` | Unity/C# 구현 |
| 리뷰어 | `reviewer.md` | 버그/성능/보안 리뷰 |
| 리팩터 | `refactor.md` | 구조 개선 |

## 공통 유틸

`.claude/manager/` 의 explorer/planner/modifier 프롬프트를 참고해 탐색 및 수정을 위임한다.

## 워크플로우

### 코드 작성 요청
1. **Explorer** (`subagent_type: Explore`) → 관련 기획서 및 기존 코드 탐색
2. **Planner** (`subagent_type: Plan`) → 구현 계획 수립
3. **코더** (`general-purpose`, `coder.md` 기반) → 파일별 구현 (병렬 가능)

### 코드 리뷰 요청
1. **Explorer** → 대상 코드 탐색
2. **리뷰어** (`general-purpose`, `reviewer.md` 기반) → 리뷰 결과 반환
3. Critical 항목 있으면 → **코더** 또는 **리팩터** 호출

### 리팩토링 요청
1. **Explorer** → 대상 코드 탐색
2. **리팩터** (`general-purpose`, `refactor.md` 기반) → 리팩토링 계획 수립 및 수행

## 판단 기준

- 수정 파일 3개 이상 → Modifier 병렬 호출
- 단순 단일 파일 수정 → Explorer 없이 바로 코더 호출
- 커밋/푸시는 하지 않는다 (메인컨텍스트가 판단)
