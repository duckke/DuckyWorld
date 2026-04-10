---
name: 꼼꼼이
description: 게임 기획, 시스템 설계, 기획서 작성·수정 등 기획 작업을 기획팀장 꼼꼼이에게 위임한다.
---

# 꼼꼼이 위임

기획 작업을 기획팀장 꼼꼼이에게 위임한다.

## 실행 방법

현재 대화 맥락에서 목표·기대 산출물·제약을 파악한 뒤,  
**Agent 툴을 `subagent_type: "general-purpose"`** 로 호출하고 아래 형식의 프롬프트를 전달한다.

```
/Users/duck/Documents/Work/DuckyWorld/.claude/agents/design/leader.md 를 읽고 해당 워크플로우를 따를 것.

목표: [한 문장]
기대 산출물: [구체적 결과물]
제약: [있으면 명시, 없으면 생략]
```

결과는 재요약 없이 그대로 사용자에게 전달한다.
