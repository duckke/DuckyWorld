# 오리 획득, 스태미나, 스탯

## 캐릭터 (오리) 시스템

### 획득
- **뽑기**: 알을 깨고 랜덤으로 오리 등장
  - 뽑기 비용: 골드 (구체적인 수치는 추후 결정)

### 스태미나
- 오리마다 개별 스태미나 보유
- 게임 플레이 시 스태미나 소모
- 스태미나 소진 시 해당 오리로 플레이 불가 (다른 오리로는 가능)
- 오리가 1마리뿐이고 스태미나 소진 → 게임 불가 → 뽑기 유도
- **회복 방식**
  - 시간 경과 → 자동 회복
  - 골드 소비 → 즉시 회복

### 캐릭터 고유 스탯
- 오리마다 기본 스탯 보유 (예: 스키 특화 오리는 기본 이동속도 낮음)
- 스탯은 장비로 추가 커스터마이징 가능

### 캐릭터 경험치 / 레벨
- 게임 플레이 시 경험치 획득
- 레벨업 시 새로운 게임 또는 컨텐츠 오픈

## 코드 구조 (v4 기준)

### Data / Info 분리
- **CharacterData** (Data/ 폴더): 데이터 테이블 원본, 불변 — 기본 스탯, 이름 등
- **CharacterInfo** (Info/ 폴더): 런타임 가공 정보 — 현재 레벨, 장비 합산 스탯, xp, 스태미나 등

### 오브젝트 계층
- **CharacterBase** → ActiveObject 상속
  - `characterInfo: CharacterInfo` (레벨, xp, 스태미나 등)
  - 상태머신, Appendage(버프/디버프), Equipment 보유
  - `moveSpeed`, `jumpForce`, `attackPower` 등 스탯
  - `ApplyInput(InputData data)` — 입력 적용

### 플레이어 정보
- **PlayerInfo** → PlayerSimpleInfo 상속
  - `ownedCharacters: List<CharacterInfo>` — 보유 캐릭터
  - `selectedCharacter: CharacterInfo` — 선택된 캐릭터
