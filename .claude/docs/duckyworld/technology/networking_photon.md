# Photon PUN2, 입력 동기화

## PvP 네트워크 - Photon PUN2

### 선택 이유
- Unity 전용 SDK, 간단한 통합
- 방 생성 / 랜덤 매칭 / 친구 초대 모두 지원
- 무료 플랜: 동시접속 20명 (초기 운영에 충분)

### 구조
- **입력 동기화 방식**: 각 클라이언트의 터치 입력을 상대방에게 전송, 동일한 게임 로직을 양쪽에서 실행
- **방 구조**: Photon Room 기반 (매칭 방식 상세: [gameplay/pvp.md](../gameplay/pvp.md) 참고)
- **친구 초대 흐름**: [gameplay/friends.md](../gameplay/friends.md) 참고
