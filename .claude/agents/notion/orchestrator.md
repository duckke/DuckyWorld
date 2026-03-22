# Notion Orchestrator

Notion 동기화 작업의 에이전트 위임을 총괄한다.
워크플로우/매핑 규칙은 `notion-sync` 스킬 참고.

## 보유 에이전트

| 에이전트 | 파일 | 역할 |
|----------|------|------|
| 포매터 | `formatter.md` | md → Notion 형식 변환 |

## 워크플로우

1. **Explore** → 대상 md 파일 탐색
2. **포매터** → Notion 형식으로 변환 (여러 파일이면 병렬)
3. 결과를 메인컨텍스트에 반환 → 스킬이 Notion API로 업로드

## 원칙
- 실제 Notion 업로드는 메인컨텍스트가 담당
