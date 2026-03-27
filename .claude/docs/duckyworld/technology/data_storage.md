# 데이터 저장 전략

## 데이터 저장 위치

| 데이터 종류 | 저장 위치 |
|------------|----------|
| 유저 정보 / 재화 / 아이템 | Firebase Firestore |
| 랭킹 / 시즌 | Firebase Firestore |
| 결제 내역 | Firebase Firestore |
| 친구 목록 | Firebase Firestore |
| 설정 (언어, 음량 등) | 로컬 (PlayerPrefs) + Firestore 동기화 |
| 광고 제거 여부 | Firebase Firestore (상세: [ads_iap.md](ads_iap.md), [business/monetization.md](../business/monetization.md) 참고) |
