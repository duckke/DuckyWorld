# 싱글 랭킹, 시즌제

## 랭킹 시스템

- 게임 오버 시 유저 점수를 Firestore에 기록
- 랭킹 순위표는 5~10분 주기로 캐싱 / 갱신 (실시간 불필요)
- 능력치 적용 상태로 기록 (상세: [gameplay/pvp.md](pvp.md) 참고)

> Firestore 컬렉션 구조 상세: [technology/backend_firebase.md](../technology/backend_firebase.md) 참고

## 시즌제

> 시즌 보상, 시즌 기간 등 구체적인 설계는 추후 상세화 예정
