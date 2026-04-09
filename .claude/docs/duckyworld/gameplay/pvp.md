# PvP 대전

## 현재 상태

> **네트워크(PvP)는 추후 고도화 단계에서 진행 (Phase 13). 현재는 싱글 플레이 우선 구현.**

## PvP 개요

- 같은 미니게임을 두 명이 동시에 플레이
- Photon PUN2 기반 실시간 매칭

## 매칭 방식

| 방식 | 설명 |
|------|------|
| 랜덤 매칭 | Photon 매치메이킹 큐 활용 |
| 방 생성 대기 | 방 코드 생성 후 상대 입장 대기 |
| 친구 초대 | 상세: [gameplay/friends.md](friends.md) 참고 |

## 티어 시스템

- 티어 기반 매칭으로 비슷한 육성 수준끼리 대전
- PvP 격차 자연 완화 목적

> 구체적인 티어 구간, 승급/강등 조건 등은 추후 상세화 예정

## PvP 능력치 적용 방침

- **PvP에서도 능력치 / 장비 옵션 그대로 적용**

## PvP 동기화 구조 (확정)

- **결정론적 프레임 루프**: GameManager.gameTimer는 FRAME_TIME(0.0166667f) 단위로만 증가 → 양쪽 동일 결과 보장
- **입력 동기화**: InputData.frameNumber 필드로 프레임 단위 입력 동기화
- **InputData struct**: InputType(TouchDown/TouchMove/TouchUp/JoystickMove), fingerId, position, joystickValue, frameNumber
- 싱글 플레이에서는 frameNumber를 무시해도 무방

## 상대방 정보

- **PlayerSimpleInfo**: playerId, playerName, level, selectedCharacterName — PvP 상대방 최소 정보
- **PlayerInfo**: PlayerSimpleInfo 상속, 자기 자신 상세 정보 (xp, gold, ownedCharacters 등)

## 랭킹 시스템

- 게임 오버 시 유저 점수를 Firestore에 기록
- 랭킹 순위표는 5~10분 주기로 캐싱 / 갱신 (실시간 불필요)
- 능력치 적용 상태로 기록

> Firestore 컬렉션 구조 상세: [technology/backend_firebase.md](../technology/backend_firebase.md) 참고

## 시즌제

> 시즌 보상, 시즌 기간 등 구체적인 설계는 추후 상세화 예정

## PvP 네트워크

- 자세한 네트워크 구현은 [technology/networking_photon.md](../technology/networking_photon.md) 참고
