# 친구 시스템

## 친구 목록

- Firebase Firestore에 친구 목록 저장

> Firestore 컬렉션 구조 상세: [technology/backend_firebase.md](../technology/backend_firebase.md) 참고

## 친구 초대 흐름

```
1. 친구 목록 (Firebase) 에서 친구 선택
2. 방 생성 (Photon) → 방 ID 발급
3. Firebase로 친구에게 초대 알림 전송
4. 친구가 수락 → Photon 방 입장
```

> 친구 추가/삭제 등 상세 흐름은 추후 상세화 예정
