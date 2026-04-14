---
name: pm-work
description: 에이전트·스킬 구조 설계, CLAUDE.md 정비, 업무 프로세스 개선 등 PM 작업을 처리한다.
---

# PM 작업 (pm-work)

## 실행 순서

### 1. 사전 구체화 (비서 필수)

작업 내용이 명확한지 확인. 불명확하면 아래 항목 중 부족한 것을 사용자에게 질문.
- 목표: 무엇을 설계·수정·정비하는지
- 기대 산출물: 어떤 파일·문서가 결과물인지
- 제약: 건드리면 안 되는 것, 커밋 여부 등

### 2. 계획 수신

"깔끔이(PM팀장)에게 계획 요청합니다"라고 명시 후, Agent 툴을 `subagent_type: "general-purpose"`로 호출.

```
/Users/duck/Documents/Work/DuckyWorld/.claude/agents/pm/leader.md 를 읽고 계획 모드로 동작할 것.

목표: [한 문장]
기대 산출물: [구체적 결과물]
제약: [있으면 명시, 없으면 생략]
```

### 3. 계획 출력

받은 계획서를 사용자에게 그대로 출력한다.

### 4. 단계별 실행

계획서의 각 Step을 순서대로 실행. 각 Step 시작 전 "현재 작업 중: [담당] — [작업내용]..." 출력.

담당별 호출 방법:
- **PM-뒤적이** → Agent 툴, `subagent_type: "pm-뒤적이"`
- **깔끔이** → Agent 툴, `subagent_type: "general-purpose"`, 아래 형식으로 호출:
  ```
  /Users/duck/Documents/Work/DuckyWorld/.claude/agents/pm/leader.md 를 읽고 실행 모드로 동작할 것.
  실행할 단계: [단계 내용]
  참고 정보: [이전 탐색 결과 등]
  ```

Step 완료 시 체크박스를 `[x]`로 업데이트해서 출력.

### 5. 결과 보고

- 수정/생성 파일 목록 (경로 + 변경 사유)
- 결과 요약 3줄 이내
