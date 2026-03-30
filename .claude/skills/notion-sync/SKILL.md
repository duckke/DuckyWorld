---
name: notion-sync
description: 기획서를 Notion 페이지에 동기화한다.
---

# Notion 동기화 스킬

기획서 md 파일을 인포그래픽과 함께 Notion 페이지에 동기화한다.

## 워크플로우

반드시 아래 순서로 진행한다:

**[1단계] 기획-에레미** — 인포그래픽 생성
- 대상 md 파일로 인포그래픽 생성 + `.claude/docs/notebooklm/` 에 다운로드
- 노트북 ID는 `.claude/docs/notebooklm/notebooks.json` 에서 slug로 조회
- 없으면 자동 생성 후 JSON 갱신

**[2단계] 기획-노셔니** — Notion 업로드
- 1단계에서 생성된 PNG를 페이지 상단에 삽입
- 본문 md를 Notion 형식으로 변환 후 업로드
- 페이지 매핑: `.claude/docs/notion/notion_map.json`

## 업로드 규칙

- 페이지 업데이트: `notion-update-page` (`replace_content`)
- 카테고리 페이지(하위 페이지가 있는 경우): `replace_content` 에러 가능 → 해당 페이지 건너뛰고 하위 개별 파일만 업데이트
- 신규 페이지 생성 후 반환된 페이지 ID를 `notion_map.json`에 추가
- 기획서 분리(1개 → 여러 개) 시 기존 페이지 처리 방식은 사용자에게 확인
