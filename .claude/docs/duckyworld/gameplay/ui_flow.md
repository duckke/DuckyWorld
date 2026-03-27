# UI 흐름 (로비~게임오버)

## 화면 흐름

```
Boot (로딩/로그인)
  ↓
Lobby (로비 - 캐릭터 뛰어노는 3D 씬)
  ↓
GameSelect (게임 선택 팝업)
  ↓
InGame (미니게임 플레이)
  ↓
GameOver (결과 화면 - 스코어, 랭킹 갱신 여부)
  ↓
Lobby 또는 GameSelect로 복귀
```

## 로비

- 캐릭터가 뛰어노는 3D 씬
- 메인 캐릭터 선택 가능
- UI 컬러 테마 상세: [art/art_style.md](../art/art_style.md) 참고

## 게임 선택

- Lobby 위에 오버레이 또는 별도 씬

> 추후 상세화 예정
