---
name: notion-sync
description: 기획서 md를 Notion 페이지에 동기화한다.
---

# Notion 동기화 스킬

기획서 md 파일을 Notion 포매터 에이전트로 가공한 뒤 Notion 페이지에 반영한다.

## 매핑 파일

`.claude/skills/notion-sync/notion_map.json` — md 파일 경로(`.claude/docs` 기준 상대경로) → Notion 페이지 ID 매핑

## 포매터 에이전트

`.claude/agents/notion/formatter.md` — Notion 업로드 전 반드시 이 에이전트를 통해 내용을 가공한다.

포매터가 하는 일:
- 이모지를 섹션 제목(H2)에만 적재적소에 배치
- 게임 용어에 괄호로 짧은 설명 추가 (모르는 사람도 이해 가능하게)
- 원본 내용과 구조는 절대 변경하지 않음

## 워크플로우

### 특정 파일 동기화

1. `notion_map.json` 읽어 Notion 페이지 ID 확인
2. md 파일 전체 내용 읽기
3. **포매터 에이전트 적용** — `agents/notion/formatter.md` 규칙에 따라 변환
4. `notion-update-page` (`replace_content` 커맨드)로 Notion 페이지 전체 교체

```
notion-update-page 호출:
- page_id: [매핑된 ID]
- command: "replace_content"
- new_str: [포매터 적용된 내용]
```

**주의 — 하위 페이지가 있는 경우:**
게임플레이, 시스템, 기술, 미니게임 등 카테고리 페이지는 자식 페이지가 있어서
`replace_content` 시 에러가 발생할 수 있다.
이 경우 `allow_deleting_content: false` 상태에서 자식 링크를 본문에 포함시키거나,
해당 페이지는 건너뛰고 하위 개별 파일만 업데이트한다.

### 전체 동기화

`notion_map.json`의 모든 항목 순서대로 업데이트.
카테고리 페이지(자식 페이지 포함)는 개별 파일 위주로 처리.

### 신규 기획서 추가 시

1. 새 md 파일 작성 완료 후
2. Notion 부모 페이지 하위에 `notion-create-pages`로 새 페이지 생성
3. 반환된 페이지 ID를 `notion_map.json`에 추가
4. 이후부터는 일반 동기화 프로세스 사용

### 기획서 분리 시 (1개 → 여러 개)

1. 새 md 파일들 각각 Notion 신규 페이지로 생성
2. `notion_map.json`에 새 매핑 추가
3. 기존 페이지는 "이 기획서는 분리되었습니다" 안내 내용으로 교체하거나 삭제
4. 사용자에게 기존 페이지 처리 방식 확인 후 진행
