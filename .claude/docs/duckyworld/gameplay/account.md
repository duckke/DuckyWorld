# 계정 시스템

## 로그인

| 방식 | 설명 |
|------|------|
| 게스트 로그인 | Firebase Auth 게스트 인증 |
| 구글 로그인 | Firebase Auth Google 연동 |
| 애플 로그인 | Firebase Auth Apple 연동 |

- 게스트 → 소셜 계정 전환 지원
- 크로스 디바이스 데이터 동기화 (Firestore 기반)

## 인증 흐름 (IntroModule)

```
1) Firebase 초기화
2) Firebase Auth 로그인 (게스트 / Google / Apple)
3) 번들 버전 체크 + 다운로드 + 로드 (BundleManager)
4) DataRepository에 Data 등록
5) 로비 씬 전환 (GameSceneLoader)
```

> 추후 상세화 예정
