---
name: notion-sync
description: 기획서를 Notion 페이지에 동기화한다.
---

# Notion 동기화 스킬

기획서 md 파일을 포매터 에이전트로 가공한 뒤 Notion 페이지에 반영한다.
에이전트 위임 및 워크플로우는 **notion-editor** (`.claude/agents/design/members/notion-editor.md`) 참고.

## 매핑 파일

`.claude/docs/notion/notion_map.json` — md 파일 경로 → Notion 페이지 ID 매핑

## 업로드 규칙

- 페이지 업데이트: `notion-update-page` (`replace_content`)
- 카테고리 페이지(하위 페이지가 있는 경우): `replace_content` 에러 가능 → 해당 페이지 건너뛰고 하위 개별 파일만 업데이트
- 신규 페이지 생성 후 반환된 페이지 ID를 `notion_map.json`에 추가
- 기획서 분리(1개 → 여러 개) 시 기존 페이지 처리 방식은 사용자에게 확인
