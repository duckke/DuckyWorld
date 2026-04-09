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
| 번들 캐시 매니페스트 | 로컬 (`Application.persistentDataPath/bundles/local_manifest.json`) |
| 번들 버전 해시 | PlayerPrefs (`BundleVersionHash`) |

## Data / Info 분리

| 구분 | 폴더 | 특성 | 예시 |
|------|------|------|------|
| Data | Data/ | 테이블 원본, 불변, 로딩 시 1회 생성 | CharacterData (기본 스탯, 이름) |
| Info | Info/ | 런타임 가공, 변경 가능 | CharacterInfo (현재 레벨, 장비 합산 스탯) |

모든 시스템에 적용: Character, Equipment, Map, MiniGame, Obstacle 등

## 데이터 파이프라인 (번들)

```
[에디터 타임]
Google Drive → Google Sheet (원본 데이터 테이블)
    ↓ (에디터 툴로 파싱)
ScriptableObject 에셋 생성 (CharacterData, MapData 등)
    ↓ (빌드)
AssetBundle로 패킹
    ↓ (배포)
서버(Firebase Storage 등)에 업로드
```

## 번들 로딩 (IntroModule에서 실행)

```
[런타임 — Intro 씬]
1) BundleManager.CheckUpdate() — 서버 매니페스트(bundle_version.json) vs 로컬 매니페스트 해시 비교
2) BundleManager.DownloadIfNeeded() — 변경된 번들만 선별 다운로드 (최대 3회 재시도)
3) BundleManager.LoadAll() — 디바이스에 저장된 번들 → AssetBundle.LoadFromFileAsync → 메모리
4) DataRepository.RegisterAll() — Data 객체 등록 (CharacterData, EquipmentData, MapData 등)
5) 완료 → 로비 씬 전환
```

## DataRepository

- MonoSingleton
- 번들에서 로드한 ScriptableObject를 파싱하여 Dictionary에 등록
- 런타임 어디서든 `DataRepository.Instance.GetCharacter(id)` 등으로 조회
- 테이블 종류: characterTable, equipmentTable, miniGameTable, mapTable, obstacleTable

## BundleManager

| 단계 | 방식 | 세부 |
|------|------|------|
| 버전 비교 | 서버 `bundle_version.json` vs 로컬 `local_manifest.json` | 번들별 해시값 비교 |
| 캐시 전략 | 해시 기반 — 해시가 같으면 스킵, 다르면 재다운로드 | `Application.persistentDataPath/bundles/`에 저장 |
| 다운로드 | `UnityWebRequest`로 변경된 번들만 선별 다운로드 | 실패 시 최대 3회 재시도 |
| 로드 | `AssetBundle.LoadFromFileAsync`로 메모리에 올림 | 비동기, 프로그레스 콜백 제공 |
