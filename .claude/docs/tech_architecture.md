# DuckyWorld - Technical Architecture

## 엔진 / 환경

| 항목 | 내용 |
|------|------|
| 엔진 | Unity |
| 렌더 파이프라인 | URP (Universal Render Pipeline) |
| 타겟 플랫폼 | iOS / Android / PC |

---

## 백엔드 - Firebase

### 사용 모듈
| 모듈 | 용도 |
|------|------|
| Firebase Auth | 게스트 / 구글 / 애플 로그인, 계정 전환 |
| Cloud Firestore | 유저 정보, 랭킹, 시즌 데이터, 친구 목록, 보유 아이템, 결제 내역 |
| Realtime Database | PvP 보조 데이터 (필요 시) |

### Firestore 주요 컬렉션 (초안)
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

### 랭킹 갱신
- 게임 오버 시 유저 점수를 Firestore에 기록
- 랭킹 순위표는 5~10분 주기로 캐싱 / 갱신 (실시간 불필요)

---

## PvP 네트워크 - Photon PUN2

### 선택 이유
- Unity 전용 SDK, 간단한 통합
- 방 생성 / 랜덤 매칭 / 친구 초대 모두 지원
- 무료 플랜: 동시접속 20명 (초기 운영에 충분)

### 구조
- **입력 동기화 방식**: 각 클라이언트의 터치 입력을 상대방에게 전송, 동일한 게임 로직을 양쪽에서 실행
- **방 구조**
  - 랜덤 매칭: Photon 매치메이킹 큐 활용
  - 방 생성 대기: 방 코드 생성 후 상대 입장 대기
  - 친구 초대: Firebase에서 친구 UID 조회 → Photon 방으로 초대

### 친구 초대 흐름
```
1. 친구 목록 (Firebase) 에서 친구 선택
2. 방 생성 (Photon) → 방 ID 발급
3. Firebase로 친구에게 초대 알림 전송
4. 친구가 수락 → Photon 방 입장
```

---

## Unity 씬 구조 (초안)

```
Scenes/
├── Boot          - 초기 로딩, Firebase 초기화, 로그인 체크
├── Lobby         - 로비, 캐릭터 뛰어노는 3D 씬
├── GameSelect    - 게임 선택 팝업 (Lobby 위에 오버레이 또는 별도 씬)
└── Games/
    ├── Ski       - 스키 미니게임 씬
    └── (추후 추가)
```

---

## 데이터 저장 전략

| 데이터 종류 | 저장 위치 |
|------------|----------|
| 유저 정보 / 재화 / 아이템 | Firebase Firestore |
| 랭킹 / 시즌 | Firebase Firestore |
| 결제 내역 | Firebase Firestore |
| 친구 목록 | Firebase Firestore |
| 설정 (언어, 음량 등) | 로컬 (PlayerPrefs) + Firestore 동기화 |
| 광고 제거 여부 | Firebase Firestore (크로스 디바이스 유지) |

---

## 로컬라이제이션

- Unity Localization 패키지 활용 예정
- 언어별 폰트 에셋 분리 (한국어, 영어, 일본어, 중국어 등)
- 언어 변경 시 앱 재시작으로 처리 (대용량 폰트 에셋 메모리 해제)

---

## 광고 / 인앱 결제

| 항목 | 내용 |
|------|------|
| 광고 | Unity Ads 또는 AdMob (추후 결정) |
| 인앱 결제 | Unity IAP |
| 광고 제거 | IAP 구매 후 Firestore에 기록, 모든 기기에서 적용 |
