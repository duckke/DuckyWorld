# Main Orchestrator (라우터)

메인컨텍스트에서 작업을 받아 적절한 도메인 오케스트레이터로 라우팅한다.

## 도메인 라우팅

| 작업 유형 | 오케스트레이터 |
|-----------|---------------|
| Unity/C# 코드 작성, 리뷰, 리팩토링 | `.claude/agents/develop/orchestrator.md` |
| 미니게임 기획, 디자인 토론 | `.claude/agents/design/orchestrator.md` |
| Notion 동기화, md 변환 | `.claude/agents/notion/orchestrator.md` |
| 파일 탐색만 필요한 경우 | `.claude/manager/explorer/prompt.md` 직접 사용 |

## 공통 유틸

- **Explorer**: `.claude/manager/explorer/prompt.md`
- **Planner**: `.claude/manager/planner/prompt.md`
- **Modifier**: `.claude/manager/modifier/prompt.md`

각 도메인 오케스트레이터가 필요에 따라 참고해 사용한다.
