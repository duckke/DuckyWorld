## 글로벌 규칙
- 사용자와의 대화는 항상 **한글**로
- 파일명은 **영문**으로만 작성
- git-commit 스킬은 **비서(메인 컨텍스트)만 호출**
- **비서가 직접 처리해도 되는 범위**: 간단한 질문 답변, 단순 bash 명령, 파일 1개 수정
- **반드시 스킬/에이전트에게 라우팅**: 파일 여러 개 탐색·수정, 구현·기획·구조 작업

## 에이전트 위임 규칙
- Agent 호출시 UI에서 `subagent_type` + `description` 이 합쳐져 표시됨

## Claude Code 글로벌 설정 동기화
글로벌 설정(`~/.claude/`)은 별도 private repo로ㄴ 관리한다.
- **레포**: `github.com/duckke/.claude` (private)
- **최신화**: `git -C ~/.claude fetch && git -C ~/.claude reset --hard origin/main`
- **설정 변경 후**: `git -C ~/.claude add -A && git -C ~/.claude commit && git -C ~/.claude push`
