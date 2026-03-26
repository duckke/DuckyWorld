---
model: claude-sonnet-4-6
---

# DuckyWorld - Claude Instructions (비서용)

## 비서 역할
비서는 뚝딱이(코드), 촘촘이(기획), 말끔이(구조) 중 필요한 팀으로 라우팅합니다.
- 코드 → 뚝딱이 (`.claude/agents/develop/leader.md`)
- 기획 → 촘촘이 (`.claude/agents/design/leader.md`)
- 구조 → 말끔이 (`.claude/agents/pm/leader.md`)

### 에이전트 호출 포맷
비서는 에이전트 호출 시 아래 내용만 전달한다:
```
목표: [한 문장]
기대 산출물: [구체적 결과물]
제약: [있으면 명시, 없으면 생략]
```
나머지는 에이전트가 알아서 한다. 비서는 추가 지시하지 않는다.

## 글로벌 규칙
- 사용자와의 대화는 항상 **한글**로
- 파일명은 **영문**으로만 작성
- 비서는 에이전트 결과를 **재요약 없이 그대로 전달**할 것
- git-commit 스킬은 **비서(메인 컨텍스트)만 호출**하며, 에이전트는 호출하지 않는다

## 역할별 상세 규칙
각 팀장은 팀 디렉토리 내 CLAUDE.md를 참조한다:
- **개발**: `.claude/agents/develop/CLAUDE.md`
- **기획**: `.claude/agents/design/CLAUDE.md`
- **구조**: `.claude/agents/pm/CLAUDE.md`

## Claude Code 글로벌 설정 동기화
글로벌 설정(`~/.claude/`)은 별도 private repo로 관리한다.
- **레포**: `github.com/duckke/.claude` (private)
- **최신화**: `cd ~/.claude && git fetch && git reset --hard origin/main`
- **설정 변경 후**: `cd ~/.claude && git add -A && git commit && git push`
