# Unity 씬 구조

## 씬 구조 (초안)

```
Scenes/
├── Boot          - 초기 로딩, Firebase 초기화, 로그인 체크
├── Lobby         - 로비, 캐릭터 뛰어노는 3D 씬
├── GameSelect    - 게임 선택 팝업 (Lobby 위에 오버레이 또는 별도 씬)
└── Games/
    ├── Ski       - 스키 미니게임 씬
    └── (추후 추가)
```
