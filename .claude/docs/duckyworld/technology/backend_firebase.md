# Firebase Auth, Firestore

## 사용 모듈

| 모듈 | 용도 |
|------|------|
| Firebase Auth | 인증 모듈 (상세: [gameplay/account.md](../gameplay/account.md) 참고) |
| Cloud Firestore | 유저 정보, 랭킹, 시즌 데이터, 친구 목록, 보유 아이템, 결제 내역 |
| Firebase Storage | AssetBundle 호스팅 (번들 버전 매니페스트 + 번들 파일) |
| Realtime Database | PvP 보조 데이터 (필요 시) |

## 인트로 흐름 (IntroModule)

```
1) Firebase 초기화
2) Firebase Auth 로그인 (게스트 / Google / Apple)
3) BundleManager.CheckUpdate() — Firebase Storage에서 번들 버전 체크
4) BundleManager.DownloadIfNeeded() — 변경된 번들만 다운로드
5) BundleManager.LoadAll() → DataRepository.RegisterAll()
6) 로비 씬 전환
```

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
