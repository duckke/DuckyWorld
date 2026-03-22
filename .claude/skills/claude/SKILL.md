---
name: claude-code-sync
description: 클로드 코드 설정을 기기 간 동기화한다.
---

# Claude Code 동기화

## 핵심 원칙

**클로드 설정에 영향을 주는 모든 변경은 반드시 이 repo의 `settings/`에 반영되고, 즉시 커밋 & 푸시되어야 한다.** 다른 기기에서 pull 받으면 자동으로 동기화된다.

## 디렉토리 구조

```
.claude/skills/claude/
├── check-changes.sh   — Stop 훅: ~/.claude/ 변경 감지 & settings/ 갱신
├── prompt-inject.sh   — UserPromptSubmit 훅: AUTO-SYNC / AUTO-APPLY 주입
├── sync.sh            — settings/ → ~/.claude/ 적용 & post-merge 훅 설치
├── SKILL.md
└── settings/          — ~/.claude/ 동기화 대상 파일 저장소
    ├── settings.json      — ~/.claude/settings.json 미러 (경로 정규화됨)
    ├── settings.version   — 현재 버전 (예: 1.0.3)
    ├── statusline-command.sh
    └── (keybindings.json 등 자동 추가됨)
```

## 동기화 대상

`check-changes.sh`가 자동으로 감지하고 `settings/`에 추가:
- `~/.claude/settings.json`
- `~/.claude/*.sh` (신규 파일 포함)
- `~/.claude/keybindings.json`

## 설정 변경 시 행동 규칙 ⚠️ 반드시 준수

**Claude가 `~/.claude/` 관련 파일을 수정한 즉시 아래 순서를 실행한다. 절대 빠뜨리지 말 것.**

```bash
# 1. 현재 버전 읽기
CUR_VER=$(cat ~/.claude/settings.version 2>/dev/null || echo "1.0.0")

# 2. 변경 규모에 따라 버전 결정
# MAJOR (x.0.0): 동기화 구조, 훅 방식 등 전체 구조가 크게 바뀐 경우
# MINOR (x.y.0): 기능 추가, 중간 규모 구조 변경
# PATCH (x.y.z): 자잘한 수정, 버그 픽스, 스타일 변경
PATCH=$(echo "$CUR_VER" | awk -F. '{print $3+1}')
NEW_VER=$(echo "$CUR_VER" | awk -F. "{print \$1\".\"\$2\".\"$PATCH}")  # PATCH 예시

# 3. 로컬 버전 갱신
echo "$NEW_VER" > ~/.claude/settings.version

# 4. settings/ 전체 갱신
SKILL_DIR=".claude/skills/claude"
SETTINGS="$SKILL_DIR/settings"
normalize() { sed "s|$(pwd)/$SKILL_DIR|__SKILL_DIR__|g; s|$HOME/|~/|g"; }
normalize < ~/.claude/settings.json | jq -S '.' > "$SETTINGS/settings.json"
find "$HOME/.claude" -maxdepth 1 -type f -name "*.sh" -exec cp {} "$SETTINGS/" \;
echo "$NEW_VER" > "$SETTINGS/settings.version"

# 5. 커밋 & 푸시 (메시지에 버전 포함 필수)
git add "$SETTINGS/" && git commit -m "기타. claude sync. [변경내용] [v${NEW_VER}]" && git push
```

- **깃 버전보다 로컬이 높아야 다른 기기에 전파된다** — 버전 올리지 않으면 동기화 안 됨
- 사용자가 직접 설정을 바꾼 경우 → Stop 훅이 자동 감지 → AUTO-SYNC로 알림 → 동의 시 위 과정 실행

## 동기화 플로우

```
설정 변경
  → Stop 훅(check-changes.sh): 변경 감지 & 버전 +1 & settings/ 갱신 & pending 저장
  → 다음 대화에서 AUTO-SYNC 알림
  → 커밋 & 푸시 (메시지에 버전 포함)
  → 다른 기기에서 pull
  → post-merge 훅: 버전 비교 → sync.sh 자동 실행
  → ~/.claude/ 최신화 완료
```

## 알림 동작

- `[AUTO-SYNC]`: 로컬 변경 감지 → 커밋 여부 질문 → 동의 시 커밋 & 푸시
- `[AUTO-APPLIED]`: settings/ 버전이 로컬보다 높음 → 질문 없이 즉시 sync.sh 실행

## 새 기기 초기 설정

```bash
bash .claude/skills/claude/sync.sh
```

완료 후 Claude Code 재시작.
