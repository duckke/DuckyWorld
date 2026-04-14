---
name: dev-work
description: Unity·C# 코드 작성, 구현, 디버깅, 리팩토링 등 개발 작업을 처리한다.
---

# 개발 작업 (dev-work)

## 비서 실행 순서

### 1. 사전 구체화 (비서 직접)

작업 내용이 명확한지 확인. 불명확하면 아래 항목 중 부족한 것을 사용자에게 질문.
- 목표: 무엇을 만들거나 고치는지
- 기대 산출물: 어떤 파일·기능이 결과물인지
- 제약: 건드리면 안 되는 것, 의존 조건 등

### 2. 계획 수신

"뚝딱이(개발팀장)에게 계획 요청합니다"라고 명시 후, Agent 툴을 `subagent_type: "general-purpose"`로 호출.

```
/Users/duck/Documents/Work/DuckyWorld/.claude/agents/develop/leader.md 를 읽고 해당 워크플로우를 따를 것.

목표: [한 문장]
기대 산출물: [구체적 결과물]
제약: [있으면 명시, 없으면 생략]
```

계획서를 받으면 각 Step을 **TaskCreate**로 생성한다.
- `subject`: "Step N: [작업내용]"
- `description`: "담당: [에이전트명]"
- `activeForm`: "[작업 중 표시 텍스트]"

### 3. 단계별 실행

계획서의 각 Step을 순서대로 실행.

- Step 시작 전 → **TaskUpdate**(status: in_progress)
- Step 완료 후 → **TaskUpdate**(status: completed)

담당 에이전트별 호출:
- **개발-뒤적이** → Agent 툴, `subagent_type: "개발-뒤적이"`
- **개발-코더** → Agent 툴, `subagent_type: "개발-코더"`
- **개발-리팩터** → Agent 툴, `subagent_type: "개발-리팩터"`
- **개발-유니티** → Agent 툴, `subagent_type: "개발-유니티"`

### 4. 결과 보고

- 수정/생성 파일 목록 (경로 + 변경 사유)
- 결과 요약 3줄 이내
