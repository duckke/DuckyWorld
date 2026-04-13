---
name: claude-env
description: Claude Code 환경(스킬/에이전트/설정)을 검토하고 개선한다.
---

# Claude Code 환경 관리

Claude Code 환경 관련 파일(스킬, 에이전트, 설정, 훅 스크립트)을 검토하거나 개선한다.

## 비서 실행 순서

### 1. 사전 구체화

검토만인지 수정까지인지 확인. 불명확하면 사용자에게 질문.
- **검토만**: "확인해줘", "어떤지 봐줘", "문제 없는지" 등
- **수정까지**: "고쳐줘", "수정해줘", "개선해줘" 등

### 2. 탐색

"PM-뒤적이 — 환경 파일 탐색 중..." 출력 후 Agent 호출.

Agent 툴, `subagent_type: "pm-뒤적이"`:
```
.claude/ 디렉토리 하위의 스킬·에이전트·설정 파일을 탐색하고 현황을 요약해줘.
탐색 대상: [사용자가 요청한 범위]
```

### 3. 분석·수정

탐색 결과를 바탕으로 다음 단계 실행.

**검토만인 경우** — "깔끔이 — 환경 분석 중..." 출력 후 Agent 호출.

Agent 툴, `subagent_type: "general-purpose"`:
```
/Users/duck/Documents/Work/DuckyWorld/.claude/agents/pm/leader.md 를 읽고 실행 모드로 동작할 것.
실행할 단계: 탐색 결과 분석 및 문제점·개선점 보고서 작성
참고 정보: [탐색 결과]
```

**수정까지인 경우** — 깔끔이에게 계획 수신 후 단계별 실행.

1. "깔끔이(PM팀장)에게 계획 요청합니다" 출력 후 Agent 호출.

   Agent 툴, `subagent_type: "general-purpose"`:
   ```
   /Users/duck/Documents/Work/DuckyWorld/.claude/agents/pm/leader.md 를 읽고 계획 모드로 동작할 것.

   목표: [한 문장]
   기대 산출물: [구체적 결과물]
   제약: [있으면 명시, 없으면 생략]
   ```

2. 계획서를 사용자에게 출력.

3. 각 Step을 순서대로 실행. Step 시작 전 "현재 작업 중: [담당] — [작업내용]..." 출력.

   담당별 호출:
   - **PM-뒤적이** → Agent 툴, `subagent_type: "pm-뒤적이"`
   - **깔끔이** → Agent 툴, `subagent_type: "general-purpose"`, 실행 모드로 호출:
     ```
     /Users/duck/Documents/Work/DuckyWorld/.claude/agents/pm/leader.md 를 읽고 실행 모드로 동작할 것.
     실행할 단계: [단계 내용]
     참고 정보: [이전 탐색 결과 등]
     ```

### 4. 결과 보고

- 수정/생성 파일 목록 (경로 + 변경 사유)
- 결과 요약 3줄 이내
