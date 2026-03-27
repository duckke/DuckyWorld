# 개발팀 규칙

## 사고 방식: 이중 자아
고민이 필요한 문제를 만나면 두 관점으로 혼자 토론하고 결론을 낸다.
- **설계자**: "구조가 깔끔한가? 확장성과 유지보수는?"
- **실용주의자**: "지금 동작하는 가장 단순한 방법은? 과도한 추상화 아닌가?"

충돌 시 **단순함 우선**. 설계자가 치명적 구조 문제를 지적하면 수용한다.

## 프로젝트 컨텍스트
- **엔진**: Unity 2022+ (URP)
- **언어**: C#
- **네트워크**: Photon PUN2
- **백엔드**: Firebase
- **플랫폼**: iOS / Android / PC
- **네임스페이스**: `namespace DuckyWorld.[Module]`

## 코딩 원칙
1. **기획서 우선**: `.claude/docs/duckyworld/` 관련 문서를 확인 후 구현
2. **단순하게**: 과도한 추상화 금지. 지금 필요한 만큼만
3. **모바일 최적화**: Update 내 new·LINQ·GetComponent 금지, 필드 캐싱
4. **읽기 쉽게**: 다음 사람이 바로 이해할 수 있는 코드
5. **동작 보존 리팩토링**: 리팩토링 전후 기능 동일, 모든 변경에 "왜 더 나은지" 설명

## 리뷰 체크리스트

**정확성**
- 로직 버그, null 참조, 상태 전이 누락
- Firebase 비동기 처리 누락 (try-catch)

**성능**
- Update() 내 new·LINQ·GetComponent 반복 사용
- Instantiate/Destroy 빈번 → 오브젝트 풀링 여부

**네트워크 (PvP)**
- `photonView.IsMine` 분기·MasterClient 권한 처리
- 로컬 vs 리모트 플레이어 로직 분리

**보안**
- Firebase 규칙으로 클라이언트 권한 제한 여부
- 클라이언트에서 최종 점수 계산 시 조작 가능성

## 주요 리팩토링 패턴
- 매직 넘버 → ScriptableObject/const
- God MonoBehaviour → 역할별 컴포넌트 분리
- Update 폴링 → 이벤트 기반
- 중복 코드 (3회+) → 공통 메서드 추출
- 깊은 if 중첩 → early return
- 불필요한 public → private/[SerializeField]
