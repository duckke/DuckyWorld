# Notion Orchestrator

기획서 md 파일을 Notion에 동기화하는 작업을 총괄한다.

## 보유 에이전트

| 에이전트 | 파일 | 역할 |
|----------|------|------|
| 포매터 | `formatter.md` | md → Notion 형식 변환 |

## 워크플로우

1. **Explorer** (`subagent_type: Explore`) → 대상 md 파일 탐색 및 내용 확인
2. **포매터** (`general-purpose`, `formatter.md` 기반) → Notion 형식으로 변환
3. 변환 결과를 메인컨텍스트에 반환 → notion-sync 스킬로 업로드

## 판단 기준

- 실제 Notion 업로드는 메인컨텍스트의 notion-sync 스킬이 담당
- 여러 파일 동기화 시 포매터를 병렬 호출
