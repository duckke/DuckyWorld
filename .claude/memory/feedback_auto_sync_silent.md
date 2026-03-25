---
name: AUTO-SYNC 변경사항 없을 때 무시
description: AUTO-SYNC 훅이 떴지만 git status에 변경사항이 없으면 아무 메시지도 출력하지 않는다
type: feedback
---

AUTO-SYNC 훅 메시지가 주입되어도 `git status`에 변경사항이 없으면 **아무 말 없이 무시**한다.

**Why:** 이미 처리 완료된 상태에서 "변경사항 없음" 메시지가 반복되면 노이즈가 된다.
**How to apply:** AUTO-SYNC 감지 → git status 확인 → 변경사항 없으면 언급 자체를 하지 않는다.
