# 스킬 전체 실전 테스트 계획

실제 파일 수정까지 포함해서 진짜 작업처럼 테스트. 완료 후 변경된 파일 전체 롤백.

---

## 테스트 순서 및 작업 내용

### Phase 1: 개발 스킬

#### 1. `dev-work` 스킬
- **테스트 작업**: Assets/Scripts 탐색 후 간단한 파일에 주석 1줄 추가
- **TaskCreate 기대**: 뚝딱이가 계획서 반환 → 비서가 2개 Step TaskCreate 생성
  - Step 1: Assets/Scripts 탐색 (개발-뒤적이)
  - Step 2: 파일 클래스 상단에 주석 추가 (개발-코더)
- **검증 포인트**: 뚝딱이가 계획서만 반환하고 직접 실행하지 않는지

#### 2. `dev` 스킬
- **테스트 작업**: "Assets/Scripts/Systems 폴더 파악해줘" 단순 위임
- **기대 동작**: 뚝딱이 직접 처리 (단순 탐색)
- **검증 포인트**: 단순 위임 스킬이 정상 호출되는지

---

### Phase 2: PM/기획 스킬

#### 3. `pm-work` 스킬
- **테스트 작업**: CLAUDE.md 개선점 파악 후 실제 수정 적용
- **TaskCreate 기대**: 깔끔이 계획 수신 → 동적으로 Step TaskCreate 생성
  - Step 1: CLAUDE.md 탐색 (PM-뒤적이)
  - Step 2: 개선점 분석 및 수정 (깔끔이)
- **검증 포인트**: `PM-뒤적이` 대문자 subagent_type으로 정상 호출되는지

#### 4. `pm` 스킬
- **테스트 작업**: "에이전트 구조 현황을 한 페이지 문서로 정리해줘" 단순 위임
- **기대 동작**: 깔끔이 직접 처리
- **검증 포인트**: 단순 위임 스킬 정상 동작

#### 5. `design-work` 스킬
- **테스트 작업**: 기존 미니게임 기획서 중 하나에 "## 개선 이력" 섹션 추가
- **TaskCreate 기대**: 꼼꼼이 계획 수신 → 동적으로 Step TaskCreate 생성
  - Step 1: 기획서 탐색 (기획-뒤적이)
  - Step 2: 섹션 추가 (꼼꼼이)
- **검증 포인트**: 꼼꼼이가 계획서 반환 후 비서가 TaskCreate 생성하는지

---

### Phase 3: 미니게임 디자인

#### 6. `minigame-design` 스킬
- **테스트 작업**: 퍼덕퍼덕 게임의 난이도 밸런스에 대해 토론 + 기획서 반영
- **TaskCreate 기대**: 고정 4개 즉시 생성
  - Step 1: 사전 구체화
  - Step 2: 기획서 탐색 (기획-뒤적이)
  - Step 3: 기획 토론 세션 (꼼꼼이)
  - Step 4: 유저 확인 및 기획서 업데이트
- **검증 포인트**: 토론 후 기획서 파일이 실제 수정되는지

---

### Phase 4: 외부 서비스 스킬

#### 7. `duck-notebooklm` 스킬
- **테스트 작업**: 미니게임 기획서 소스로 인포그래픽 생성
  - 소스: `.claude/docs/duckyworld/minigames/02_flap_flap.md`
  - 아티팩트: infographic
- **TaskCreate 기대**: 고정 2개 즉시 생성
  - Step 1: 작업 내용 구체화
  - Step 2: 에레미 호출
- **검증 포인트**: 에레미 정상 호출 여부

#### 8. `notion-sync` 스킬
- **테스트 작업**: 기획서 → Notion 업로드
  - 소스: `.claude/docs/duckyworld/minigames/02_flap_flap.md`
- **TaskCreate 기대**: 고정 4개 즉시 생성
  - Step 1: 입력 유형 결정
  - Step 2: 노트북 확인
  - Step 3: 에레미 호출
  - Step 4: 노셔니 호출
- **검증 포인트**: 에레미+노셔니 순차 호출 여부

---

### Phase 5: 환경 관리 + 마무리

#### 9. `claude-env` 스킬
- **테스트 작업**: 수정 모드로 환경 파일에서 개선점 찾아 실제 수정
- **TaskCreate 기대**: 수정 모드이므로 동적 생성
  - Step 1: 탐색 (PM-뒤적이)
  - Step 2~N: 계획에 따라
- **검증 포인트**: 수정 모드 전체 흐름 (탐색 → 계획 → 수정)

#### 10. `git-commit` 스킬
- **테스트 작업**: 테스트 중 수정된 파일들 커밋
- **기대 동작**: 변경사항 파악 → 커밋 메시지 구성 → 커밋+푸시
- **검증 포인트**: 커밋 메시지 형식이 DuckyWorld 규칙에 맞는지

---

## 롤백 방법

테스트 완료 후 (미커밋 변경사항만 있는 경우):
```bash
git -C /Users/duck/Documents/Work/DuckyWorld checkout -- .
git -C /Users/duck/Documents/Work/DuckyWorld clean -fd
```

커밋까지 한 경우:
```bash
git -C /Users/duck/Documents/Work/DuckyWorld reset --soft HEAD~1
git -C /Users/duck/Documents/Work/DuckyWorld checkout -- .
```

---

## 진행 현황

### Phase 1 완료 (2026-04-15)
- [x] dev-work (탐색+코드추가): 뚝딱이 계획서 반환 ✅ / TaskCreate 동적 2개 ✅ / 개발-뒤적이→개발-코더 ✅
- [x] dev-work (리팩토링): 뚝딱이 계획서 반환 ✅ / TaskCreate 동적 2개 ✅ / 개발-뒤적이→개발-리팩터 ✅
- [x] dev-work (Systems 폴더 파악): 뚝딱이 계획서 반환 ✅ / 개발-뒤적이 ✅

### Phase 2~5 미진행

---

## 검증 체크리스트

- [x] dev-work: 뚝딱이가 계획서만 반환 (직접 실행 안 함)
- [ ] pm-work: PM-뒤적이 대문자 호출 정상
- [ ] design-work: 꼼꼼이 계획서 반환 후 비서가 TaskCreate 생성
- [ ] minigame-design: 기획서 실제 파일 수정됨
- [ ] claude-env: 수정 모드 전체 흐름 완료
- [ ] 고정 TaskCreate 스킬(minigame-design, duck-notebooklm, notion-sync): 즉시 전체 생성
- [ ] 동적 TaskCreate 스킬(dev-work, pm-work, design-work, claude-env): 계획 후 생성
