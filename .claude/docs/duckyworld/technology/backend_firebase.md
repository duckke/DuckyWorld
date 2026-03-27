# Firebase Auth, Firestore

## 사용 모듈

| 모듈 | 용도 |
|------|------|
| Firebase Auth | 인증 모듈 (상세: [gameplay/account.md](../gameplay/account.md) 참고) |
| Cloud Firestore | 유저 정보, 랭킹, 시즌 데이터, 친구 목록, 보유 아이템, 결제 내역 |
| Realtime Database | PvP 보조 데이터 (필요 시) |

## Firestore 주요 컬렉션 (초안)

```
users/{uid}
  - nickname
  - level / xp
  - gold
  - selectedCharacter
  - ownedCharacters[]
  - settings (언어, 광고제거 여부 등)
  - friends[]

rankings/{gameId}/scores/{uid}
  - score
  - updatedAt

seasons/{seasonId}
  - startAt / endAt
  - rewards[]
  - topRankers[]
```

## 랭킹 갱신

> 상세: [gameplay/pvp.md](../gameplay/pvp.md) 참고
