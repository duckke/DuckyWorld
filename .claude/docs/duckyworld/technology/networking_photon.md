# Photon PUN2, 입력 동기화

## 현재 상태

> **네트워크(PvP)는 추후 고도화 단계에서 진행. 현재는 싱글 플레이 우선 구현.** (Phase 13)

- `Network/` 폴더(NetworkManager, InputSync, PhotonManager)는 폴더 구조에만 예약
- 구현은 후순위
- 현재 InputData의 `frameNumber` 필드만 PvP 대비로 미리 포함
- 싱글 플레이에서는 `frameNumber`를 무시해도 무방

## PvP 네트워크 - Photon PUN2

### 선택 이유
- Unity 전용 SDK, 간단한 통합
- 방 생성 / 랜덤 매칭 / 친구 초대 모두 지원
- 무료 플랜: 동시접속 20명 (초기 운영에 충분)

### 구조
- **입력 동기화 방식**: 각 클라이언트의 터치 입력을 상대방에게 전송, 동일한 게임 로직을 양쪽에서 실행
- **결정론적 프레임 루프**: GameManager.gameTimer는 FRAME_TIME 단위로만 증가 → 양쪽 동일 결과 보장
- **방 구조**: Photon Room 기반 (매칭 방식 상세: [gameplay/pvp.md](../gameplay/pvp.md) 참고)
- **친구 초대 흐름**: [gameplay/friends.md](../gameplay/friends.md) 참고
