---
name: notion-sync
description: 기획서 md를 Notion 페이지에 동기화한다.
---

# Notion 동기화 스킬

기획서 md 파일을 포매터 에이전트로 가공한 뒤 Notion 페이지에 반영한다.
에이전트 위임은 **notion 오케스트레이터** (`.claude/agents/notion/orchestrator.md`) 참고.

## 매핑 파일

`.claude/skills/notion-sync/notion_map.json` — md 파일 경로 → Notion 페이지 ID 매핑

## 워크플로우

### 특정 파일 동기화

1. `notion_map.json`에서 Notion 페이지 ID 확인
2. md 파일 읽기
3. 포매터 에이전트 적용 (`.claude/agents/notion/formatter.md`)
4. `notion-update-page` (`replace_content`)로 페이지 교체

**하위 페이지가 있는 경우:**
카테고리 페이지는 자식 페이지가 있어 `replace_content` 시 에러 가능.
해당 페이지는 건너뛰고 하위 개별 파일만 업데이트한다.

### 전체 동기화

`notion_map.json`의 모든 항목을 순서대로 업데이트.

### 신규 기획서 추가

1. `notion-create-pages`로 새 페이지 생성
2. 반환된 페이지 ID를 `notion_map.json`에 추가

### 기획서 분리 (1개 → 여러 개)

1. 새 md 파일 각각 Notion 신규 페이지 생성
2. `notion_map.json`에 매핑 추가
3. 기존 페이지 처리 방식은 사용자에게 확인
