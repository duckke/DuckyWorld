# DuckyWorld 코드 구조 설계안 v4 (최종)

## Context

Unity 6000.4.0f1 + URP 빈 프로젝트. New Input System 활성화.
미니게임 7개, Firebase Auth/Firestore, Photon PUN2 PvP.
핵심 원칙: **mainProc 체인 (pre→main→post)**, **Logic-View 완전 분리**, **Factory + Pool**, **모듈 기반 흐름**, **GC 제로 충돌 시스템**

### 명명 규칙
- 네임스페이스 없음, DW 접두사 없음 (DWTimer만 예외)
- **Info**: 코드에서 가공/조합된 런타임 정보 (CharacterInfo, EquipmentInfo 등)
- **Data**: 데이터 테이블에서 읽어들인 원본 데이터 (CharacterData, EquipmentData 등)
- 파일명은 영문

---

## 1. 폴더 구조

```
Assets/Scripts/
│
├── Core/                              — 게임 전체 흐름
│   ├── MonoSingleton.cs                   MonoBehaviour 싱글톤 베이스
│   ├── GameManager.cs                     메인 루프, 타이머, proc 체인
│   ├── ModuleBase.cs                      모듈 추상 (오브젝트 관리)
│   ├── GameSceneLoader.cs                 씬 전환 + 로딩
│   └── ViewObjectManager.cs              View 전체 관리 (on/off 토글)
│
├── Object/                            — 오브젝트 계층
│   ├── Base/
│   │   ├── ObjectBase.cs                  최상위 Logic (DWTimer, Pool)
│   │   ├── ObjectView.cs                  View 레이어 (실제 GameObject)
│   │   ├── DrawOnly.cs                    충돌 없는 이펙트/연출
│   │   ├── CollisionObject.cs             충돌 + HP + LogicCollider
│   │   ├── PassiveObject.cs               조작X, 자체이동O (미사일 등)
│   │   ├── ActiveObject.cs                상태머신, 조작O, Appendage
│   │   └── Appendage.cs                   버프/디버프
│   │
│   ├── Character/
│   │   └── CharacterBase.cs               ActiveObject 상속
│   ├── Monster/
│   │   └── MonsterBase.cs                 ActiveObject 상속
│   └── Obstacle/
│       └── ObstacleBase.cs                CollisionObject 상속
│
├── Collision/                         — 충돌 시스템
│   ├── LogicColliderData.cs               충돌체 데이터 (struct)
│   ├── AnimationColliderData.cs           애니메이션별 프레임별 충돌체 매핑
│   ├── CollisionLayer.cs                  충돌 레이어 + 마스크
│   ├── CollisionManager.cs                BroadPhase + NarrowPhase
│   ├── CollisionResult.cs                 충돌 결과 (struct)
│   └── CollisionHelper.cs                 AABB/Circle/Capsule 오버랩 수학 함수
│
├── Player/                            — 플레이어 정보
│   ├── PlayerSimpleInfo.cs                PvP: 상대방 최소 정보
│   └── PlayerInfo.cs                      싱글: PlayerSimpleInfo 상속, 상세 정보
│
├── MiniGame/                          — 미니게임
│   ├── Base/
│   │   ├── MiniGameBase.cs                미니게임 추상
│   │   ├── MiniGameFactory.cs             GameType enum + 팩토리
│   │   ├── MiniGameModule.cs              미니게임 전용 모듈
│   │   └── MapManagerBase.cs              맵 생성/관리 추상
│   │
│   ├── ThumpThumpSlope/                   두근두근 슬로프
│   ├── FlutterFlutter/                    퍼덕퍼덕
│   ├── WaddleSprint/                      뒤뚱뒤뚱 질주
│   ├── HoppyForest/                       폴짝폴짝 포레스트
│   ├── SlideRun/                          슬라이드런
│   ├── NarrowPath/                        아슬아슬 좁은길
│   └── DodgeVillain/                      피해피해 악당오리
│
├── Module/                            — 씬/목적별 모듈
│   ├── IntroModule.cs                     인트로
│   └── LobbyModule.cs                    로비
│
├── Input/                             — 입력 시스템
│   ├── InputManager.cs                    입력 큐, preProc에서 처리
│   ├── IGameInput.cs                      인터페이스
│   ├── TapInput.cs
│   ├── SwipeInput.cs
│   ├── RapidTapInput.cs
│   └── JoystickInput.cs
│
├── Factory/                           — 생성 + 풀링
│   ├── ObjectFactory.cs                   오브젝트 생성
│   └── ObjectPool.cs                     ObjectType enum 키 풀링
│
├── Network/                           — PvP
│   ├── NetworkManager.cs
│   ├── InputSync.cs
│   └── PhotonManager.cs
│
├── Backend/                           — Firebase
│   ├── FirebaseService.cs
│   └── DataRepository.cs
│
├── Economy/                           — 재화/뽑기
│   ├── CurrencyManager.cs
│   ├── GachaSystem.cs
│   └── RewardManager.cs
│
├── Data/                              — 데이터 테이블 (불변 원본)
│   ├── CharacterData.cs
│   ├── EquipmentData.cs
│   ├── MiniGameData.cs
│   ├── MapData.cs
│   └── ObstacleData.cs
│
├── Info/                              — 런타임 가공 정보
│   ├── CharacterInfo.cs
│   ├── EquipmentInfo.cs
│   └── MiniGameInfo.cs
│
├── UI/                                — 단일 Canvas 스택
│   ├── UIManager.cs                       Canvas 1개, 스택 push/pop
│   ├── UIBase.cs                          UI 최상위 베이스
│   ├── Screens/
│   │   ├── UILobby.cs
│   │   ├── UIThumpThumpSlope.cs
│   │   └── ...
│   └── Popup/
│       ├── UIPopupGameOver.cs
│       └── ...
│
├── Editor/                            — 에디터 전용 툴
│   └── ColliderExtractor.cs              프리팹 콜라이더 → 데이터 추출 툴
│
└── Utils/
    ├── DWTimer.cs                         타이머
    ├── CommonFunc.cs                      공통 함수
    └── CommonConsts.cs                   공통 상수
```

---

## 2. GameManager — 프레임 루프 상세

### 핵심: 고정 프레임 타임 기반 업데이트

```csharp
public class GameManager : MonoSingleton<GameManager>
{
    public const float FRAME_TIME = 0.0166667f; // 60fps
    
    public ModuleBase CurrentModule { get; private set; }
    
    float timer;          // Time.deltaTime 누적기
    float gameTimer;      // 게임 로직 시간 (FRAME_TIME 단위로만 증가)
    
    // 예약 큐
    Queue<Action> spawnQueue;
    Queue<ObjectBase> despawnQueue;
    
    void Update()
    {
        timer += Time.deltaTime;
        
        while (timer >= FRAME_TIME)
        {
            timer -= FRAME_TIME;
            gameTimer += FRAME_TIME;
            
            DoUpdate(FRAME_TIME);
        }
    }
    
    void DoUpdate(float dt)
    {
        // === Logic Proc ===
        // 1) pre — 시작되는 것들
        ProcessSpawnQueue();
        ProcessDespawnQueue();
        InputManager.Instance?.ProcessQueue();
        CurrentModule?.preProc(dt);
        
        // 2) main — 실제 로직
        CurrentModule?.mainProc(dt);
        
        // 3) post — 끝나는 것들, 다음 프레임 예약
        CurrentModule?.postProc(dt);
        
        // === Non-Logic Proc ===
        // 4) View 동기화
        ViewObjectManager.Instance?.SyncAll();
        
        // 5) UI 업데이트
        UIManager.Instance?.Proc(dt);
    }
}
```

**포인트:**
- `timer`는 `Time.deltaTime`을 누적, `FRAME_TIME` 넘을 때마다 한 프레임 처리
- `gameTimer`는 `FRAME_TIME` 단위로만 증가 → 결정론적 (PvP 동기화 안전)
- 한 Update()에서 여러 프레임 처리 가능 (프레임 드랍 보정)
- Logic Proc과 Non-Logic Proc(View, UI) 명확 분리

---

## 3. ModuleBase — 모듈이 오브젝트 리스트 관리

```csharp
public abstract class ModuleBase
{
    protected List<ObjectBase> objects;
    
    public abstract void OnEnter();
    public abstract void OnExit();
    
    // 역순 반복 (중간 삭제 안전)
    public virtual void preProc(float dt)
    {
        for (int i = objects.Count - 1; i >= 0; i--)
            objects[i].preProc(dt);
    }
    public virtual void mainProc(float dt)
    {
        for (int i = objects.Count - 1; i >= 0; i--)
            objects[i].mainProc(dt);
    }
    public virtual void postProc(float dt)
    {
        for (int i = objects.Count - 1; i >= 0; i--)
            objects[i].postProc(dt);
    }
    
    public void Register(ObjectBase obj) { objects.Add(obj); }
    public void Unregister(ObjectBase obj) { objects.Remove(obj); }
}
```

| 모듈 | 역할 | 씬 |
|------|------|----|
| IntroModule | Firebase 초기화, 로그인 | Intro.unity |
| LobbyModule | 캐릭터 관리, 게임 선택 | Lobby.unity |
| MiniGameModule | 미니게임 진행 | Games/*.unity |

---

## 4. 오브젝트 계층

```
ObjectBase (Logic 전용, 하이어라키 X, 순수 C#)
│  - objectId: int (static 카운터)
│  - objectName: string
│  - position: Vector3, rotation: Quaternion (Logic 좌표)
│  - objectTimer: DWTimer
│  - viewId: int (-1 = View 없음)
│  - poolKey: ObjectType (enum)
│  - logicAnimator: LogicAnimator (현재 애니메이션/프레임 관리)
│  - preProc(dt), mainProc(dt), postProc(dt)
│  - OnSpawn(), OnDespawn()
│
├── DrawOnly
│   - 충돌 없음, 이펙트/연출
│   - duration 만료 시 자동 회수 예약
│
└── CollisionObject
    - hp: int (0 = 파괴 불가)
    - maxHp: int
    - collisionLayer: CollisionLayer (비트 마스크)
    - collisionMask: int (어떤 레이어와 충돌?)
    - colliderDataRef: AnimationColliderData (사전 로드, 불변)
    - activeColliderIndex: int, activeColliderCount: int
      → 애니메이션/프레임 변경 시 인덱스만 교체 (GC 제로)
    - facingDirection: int (1=우, -1=좌 → offset.x에 곱해 미러링)
    - OnCollision(CollisionObject other)
    - TakeDamage(int amount) → hp 감소, 0 이하면 OnDeath()
    - OnDeath()
    │
    ├── ObstacleBase (장애물)
    │   - obstacleEffectType: enum (Instant, Stun, SlowDown)
    │   - effectValue: float
    │   - hp=0 기본 (파괴 불가)
    │
    ├── PassiveObject (조작X, 자체 이동O)
    │   - velocity: float
    │   - direction: Vector3
    │   - 상태머신 X
    │   - mainProc: position += direction * velocity * dt
    │
    └── ActiveObject (상태머신, 조작O)
        - moveSpeed, jumpForce, attackPower: float
        - currentState: int
        - appendages: Dictionary<int, List<Appendage>>
        - equipment: Dictionary<EquipPartType, EquipmentInfo>
        - AddAppendage(app), RemoveAllById(id)
        - pre/main/postProc에서 appendage 체인 호출
        - ApplyInput(InputData data)
        │
        ├── CharacterBase
        │   - characterInfo: CharacterInfo (레벨, xp, 스태미나 등)
        │
        └── MonsterBase
            - aiPattern, attackPattern
```

### LogicAnimator (ObjectBase 내장)
```csharp
public class LogicAnimator
{
    int currentAnimId;
    int currentFrame;
    float frameTimer;
    float frameDuration;    // 1/fps (애니메이션 데이터에서)
    int totalFrames;
    bool isLooping;
    
    // 프레임 진행 (ObjectBase.preProc에서 호출)
    public bool UpdateFrame(float dt)
    {
        frameTimer += dt;
        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            currentFrame++;
            if (currentFrame >= totalFrames)
                currentFrame = isLooping ? 0 : totalFrames - 1;
            return true;  // 프레임 변경됨 → collider 인덱스 갱신 필요
        }
        return false;
    }
    
    public void Play(int animId, AnimationData data) { ... }
}
```

### Appendage
```csharp
public class Appendage
{
    public int appendageId;     // 그룹 ID
    public AppendageType type;  // SpeedUp, Invincible, Stun ...
    public float duration, elapsed, value;
    
    public void preProc(float dt) { }
    public bool mainProc(float dt) { elapsed += dt; return elapsed < duration; }
    public void postProc(float dt) { }
    public void OnAttach(ActiveObject target) { }
    public void OnDetach(ActiveObject target) { }
}
```

---

## 5. 충돌 시스템 (Logic 레벨, GC 제로)

### 5-1. 데이터 구조 (전부 struct 또는 사전 할당)

```csharp
// 충돌체 하나의 데이터 (struct, 불변)
public struct LogicColliderData
{
    public ColliderBoxType boxType;   // Attack, Damage, Guard
    public ColliderShape shape;       // AABB, Circle, Capsule
    public Vector3 offset;            // 오브젝트 기준 상대 좌표
    public Vector3 size;              // 반크기 (AABB) 또는 x=반지름 (Circle)
    public int value;                 // 공격력, 가드 수치 등
}

public enum ColliderBoxType { Attack, Damage, Guard }
public enum ColliderShape { AABB, Circle, Capsule }
```

```csharp
// 애니메이션별 프레임별 충돌체 매핑 (로딩 시 1회 생성, 불변)
public class AnimationColliderData
{
    // 전체 충돌체 배열 (flat, 모든 애니메이션의 모든 프레임 데이터)
    public LogicColliderData[] allColliders;
    
    // 인덱스 룩업: [animId][frameIndex] → (startIndex, count)
    public ColliderRange[][] frameLookup;
}

public struct ColliderRange
{
    public int startIndex;
    public int count;
}
```

**GC 제로 원리:**
- `allColliders[]`는 게임 시작 시 한 번만 할당
- 프레임 변경 → `frameLookup[animId][frame]`에서 `startIndex + count` 읽기
- CollisionObject에 `activeColliderIndex`, `activeColliderCount` int 2개만 갱신
- **배열 복사 없음, new 없음, GC 없음**

### 5-2. 충돌 레이어 + 마스크

```csharp
[System.Flags]
public enum CollisionLayer
{
    None       = 0,
    Character  = 1 << 0,
    Monster    = 1 << 1,
    Obstacle   = 1 << 2,
    Projectile = 1 << 3,
    Item       = 1 << 4,
}

// CollisionObject 내부:
// collisionLayer = CollisionLayer.Character;
// collisionMask  = CollisionLayer.Obstacle | CollisionLayer.Monster;
// → 이 캐릭터는 장애물, 몬스터와만 충돌 검사
```

### 5-3. CollisionManager

```csharp
public class CollisionManager
{
    // 사전 할당 결과 버퍼 (GC 제로)
    CollisionResult[] hitResults;   // 시작 시 new[256]
    int hitCount;
    
    // mainProc에서 호출 — 감지만, 처리는 안 함
    public void CheckCollisions(List<ObjectBase> objects)
    {
        hitCount = 0;
        
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] is not CollisionObject a) continue;
            
            for (int j = i + 1; j < objects.Count; j++)
            {
                if (objects[j] is not CollisionObject b) continue;
                
                // 레이어 마스크 필터
                if ((a.collisionMask & (int)b.collisionLayer) == 0 &&
                    (b.collisionMask & (int)a.collisionLayer) == 0)
                    continue;
                
                // NarrowPhase 판정
                if (CheckOverlap(a, b, out var result))
                {
                    hitResults[hitCount++] = result;
                    if (hitCount >= hitResults.Length) return; // 버퍼 초과 방지
                }
            }
        }
    }
    
    // postProc에서 호출 — 실제 결과 처리
    public void ProcessResults()
    {
        for (int i = 0; i < hitCount; i++)
        {
            var r = hitResults[i];
            r.objectA.OnCollision(r.objectB);
            r.objectB.OnCollision(r.objectA);
        }
    }
    
    bool CheckOverlap(CollisionObject a, CollisionObject b, out CollisionResult result)
    {
        // a의 활성 충돌체들 vs b의 활성 충돌체들
        // AnimationColliderData에서 인덱스 범위로 접근
        // CollisionHelper의 AABB/Circle 오버랩 함수 사용
        // facingDirection에 따라 offset.x 미러링
        ...
    }
}
```

```csharp
// 충돌 결과 (struct)
public struct CollisionResult
{
    public CollisionObject objectA;
    public CollisionObject objectB;
    public ColliderBoxType boxTypeA;  // A의 어떤 박스가 맞았는지
    public ColliderBoxType boxTypeB;
    public int valueA;                // A의 공격력 등
    public int valueB;
}
```

### 5-4. CollisionHelper (순수 수학)

```csharp
public static class CollisionHelper
{
    public static bool OverlapAABB(Vector3 posA, Vector3 sizeA, 
                                    Vector3 posB, Vector3 sizeB)
    {
        return Mathf.Abs(posA.x - posB.x) < sizeA.x + sizeB.x &&
               Mathf.Abs(posA.y - posB.y) < sizeA.y + sizeB.y &&
               Mathf.Abs(posA.z - posB.z) < sizeA.z + sizeB.z;
    }
    
    public static bool OverlapCircle(Vector3 posA, float radiusA,
                                      Vector3 posB, float radiusB)
    {
        float dx = posA.x - posB.x;
        float dy = posA.y - posB.y;
        float r = radiusA + radiusB;
        return (dx * dx + dy * dy) < (r * r); // sqrt 안 씀
    }
    
    public static bool OverlapAABBvsCircle(...) { ... }
    public static bool OverlapCapsule(...) { ... }
    // 조합별 약 6~8개 함수
}
```

### 5-5. 방향 미러링

```
데이터에는 기본 방향(우측) 기준으로 offset 저장
런타임: worldOffset.x = data.offset.x * facingDirection
```

### 5-6. 충돌 처리 타이밍 (proc 체인 내 위치)

```
preProc:
  ├── LogicAnimator.UpdateFrame(dt)
  ├── 프레임 변경 → activeColliderIndex/Count 갱신
  └── Appendage.preProc

mainProc:
  ├── 오브젝트 이동/상태 로직
  ├── CollisionManager.CheckCollisions()   ← 감지만
  └── Appendage.mainProc

postProc:
  ├── CollisionManager.ProcessResults()    ← 데미지, 사망 처리
  ├── 오브젝트 생성/삭제 예약
  ├── 입력 예약
  └── Appendage.postProc
```

---

## 6. ViewObjectManager (Logic-View 분리)

```csharp
public class ViewObjectManager : MonoSingleton<ViewObjectManager>
{
    Dictionary<int, ObjectView> activeViews;
    bool isEnabled = true;
    
    public int CreateView(ObjectBase owner, string prefabKey) { ... }
    public void SyncAll()  // Logic → View 동기화 (isEnabled일 때만)
    public void SetEnabled(bool enabled)  // ML 모드 토글
    public void DestroyView(int objectId) { ... }
}
```

`SetEnabled(false)` → 게임 로직 100% 동일, 화면만 꺼짐. ML 초고속 시뮬 가능.

---

## 7. 입력 시스템 (PvP 대비)

```
유저 터치 → InputManager.Enqueue()
  ├─ 싱글: inputQueue에 저장
  └─ PvP:  InputSync.Send() → 서버 → 동기화 → inputQueue

postProc → 입력 예약
preProc  → InputManager.ProcessQueue() → 오브젝트 상태 반영
```

InputData는 struct (GC 없음).

---

## 8. 미니게임 구조

### MiniGameModule
```csharp
public class MiniGameModule : ModuleBase
{
    public GameType CurrentGameType { get; private set; }
    public MiniGameBase CurrentGame { get; private set; }
    public MapManagerBase MapManager { get; private set; }
    
    // MiniGameFactory가 CurrentGameType에 맞는 클래스 생성
    // 재정의 클래스 있으면 → ThumpThumpSlope.cs
    // 없으면 → MiniGameBase 기본 동작
}
```

### MiniGameBase
```csharp
public abstract class MiniGameBase
{
    public abstract GameType GameType { get; }
    public abstract void OnReady(), OnPlay(), OnPause(), OnResume(), OnEnd(), OnRestart();
    public virtual void mainProc(float dt) { }
    public abstract float GetScore();
    public abstract IGameInput CreateInput();
    public abstract MapManagerBase CreateMapManager();
}
```

### GameType enum
ThumpThumpSlope, FlutterFlutter, WaddleSprint, HoppyForest, SlideRun, NarrowPath, DodgeVillain

---

## 9. Player 시스템

```csharp
public class PlayerSimpleInfo     // PvP 상대방 최소 정보
{
    public string playerId;
    public string playerName;
    public int level;
    public string selectedCharacterName;
}

public class PlayerInfo : PlayerSimpleInfo    // 나 자신 상세 정보
{
    public int xp;
    public int gold;
    public List<CharacterInfo> ownedCharacters;
    public CharacterInfo selectedCharacter;
    public List<GameType> ownedGames;
    public PlayerSettings settings;
}
```

---

## 10. 팩토리 + 풀링

### ObjectType enum (풀 키)
```csharp
public enum ObjectType
{
    Character_Duck_Default,
    Character_Duck_Ski,
    Monster_VillainDuck,
    Obstacle_Tree,
    Obstacle_Snowman,
    Obstacle_Balloon,
    Projectile_Snowball,
    Effect_Explosion,
    Effect_ScorePopup,
    // ... 게임별로 추가
}
```

### ObjectPool
```csharp
public class ObjectPool
{
    Dictionary<ObjectType, Queue<ObjectBase>> pools;
    
    // 게임 시작(로딩) 시 프리로드
    public void PreWarm(ObjectType type, int count) { ... }
    
    public T Get<T>(ObjectType type) where T : ObjectBase, new()
    {
        if (pools[type].Count > 0)
        {
            var obj = (T)pools[type].Dequeue();
            obj.OnSpawn();
            return obj;
        }
        return ObjectFactory.Create<T>(type);
    }
    
    public void Return(ObjectBase obj)
    {
        obj.OnDespawn();
        pools[obj.poolKey].Enqueue(obj);
    }
}
```

**GC 최소화:**
- PreWarm으로 로딩 시 충분한 풀 생성
- OnSpawn/OnDespawn에서 필드 초기화만 (new 없음)
- 컬렉션은 Clear() (capacity 유지)
- InputData, CollisionResult는 struct
- Appendage도 풀링

---

## 11. UI 시스템

```csharp
public abstract class UIBase : MonoBehaviour
{
    public virtual void OnShow() { }
    public virtual void OnHide() { }
    public virtual void OnFocus() { }
}

public class UIManager : MonoSingleton<UIManager>
{
    Canvas mainCanvas;
    Stack<UIBase> screenStack;
    
    public void Push<T>() where T : UIBase { }
    public void Pop() { }
    public void Replace<T>() where T : UIBase { }
    public void Proc(float dt) { }  // GameManager에서 NonLogicProc으로 호출
}
```

- HUD 폴더 없음 — 각 씬의 최하단 UI를 HUD로 사용 (닫지 못하게)
- UI도 GameManager의 NonLogicProc에서 갱신됨

UI 명명: `UILobby`, `UIThumpThumpSlope`, `UIPopupGameOver` 등

---

## 12. Data / Info 분리

| 구분 | 폴더 | 특성 | 예시 |
|------|------|------|------|
| Data | Data/ | 테이블 원본, 불변, 로딩 시 1회 생성 | CharacterData (기본 스탯, 이름) |
| Info | Info/ | 런타임 가공, 변경 가능 | CharacterInfo (현재 레벨, 장비 합산 스탯) |

모든 시스템에 적용: Character, Equipment, Map, MiniGame, Obstacle 등

---

## 13. 에디터 툴 — ColliderExtractor

```csharp
// Assets/Scripts/Editor/ColliderExtractor.cs
// EditorWindow로 구현

기능:
1. 프리팹 선택
2. 애니메이션 클립 목록 표시
3. AnimationClip.SampleAnimation()으로 프레임별 순회
4. 각 프레임에서 활성화된 Collider 정보 추출
   - BoxCollider → AABB (center→offset, size→size)
   - SphereCollider → Circle (center→offset, radius→size.x)
   - CapsuleCollider → Capsule
5. 자식 오브젝트의 로컬 좌표 → 루트 기준 offset 변환
6. 오브젝트 스케일 보정
7. Collider 이름에서 boxType 파싱 ("AttackBox_*" → Attack, "DamageBox_*" → Damage)
8. 결과 → ScriptableObject 또는 JSON으로 저장 (AnimationColliderData)
```

---

## 14. 씬 구조

```
Scenes/
├── Intro.unity
├── Lobby.unity
└── Games/
    ├── ThumpThumpSlope.unity
    ├── FlutterFlutter.unity
    ├── WaddleSprint.unity
    ├── HoppyForest.unity
    ├── SlideRun.unity
    ├── NarrowPath.unity
    └── DodgeVillain.unity
```

---

## 15. 전체 관계도

```
GameManager (MonoSingleton, FRAME_TIME 기반)
│ - timer, gameTimer
│ - spawnQueue, despawnQueue
│ - DoUpdate(dt): pre → main → post → ViewSync → UIProc
│
├── CurrentModule: ModuleBase
│   │ - objects: List<ObjectBase>
│   │ - pre/main/postProc 체인
│   │
│   ├── IntroModule
│   ├── LobbyModule
│   └── MiniGameModule
│       ├── currentGameType, currentGame: MiniGameBase
│       ├── mapManager: MapManagerBase
│       └── collisionManager: CollisionManager
│
├── ViewObjectManager (MonoSingleton)
│   │ - activeViews, isEnabled, SyncAll()
│   └── ViewPool (GameObject 풀)
│
├── InputManager (MonoSingleton)
│   ├── IGameInput (Tap, Swipe, RapidTap, Joystick)
│   ├── inputQueue
│   └── InputSync (PvP)
│
├── ObjectPool (ObjectType enum 키)
│   └── ObjectFactory
│
├── CollisionManager
│   ├── hitResults[256] (사전 할당)
│   ├── CheckCollisions() — mainProc
│   └── ProcessResults() — postProc
│
├── UIManager (MonoSingleton, 단일 Canvas 스택)
│   └── UIBase → 화면/팝업
│
├── PlayerInfo (현재 플레이어)
├── CurrencyManager
├── FirebaseService (MonoSingleton)
└── DataRepository

ObjectBase (Logic, DWTimer, LogicAnimator)
├── DrawOnly (이펙트)
└── CollisionObject (HP, LogicCollider, CollisionLayer)
    │ - colliderDataRef → AnimationColliderData (불변, 사전 로드)
    │ - activeColliderIndex/Count (int 2개, GC 제로 교체)
    │ - facingDirection (미러링)
    │
    ├── ObstacleBase (장애물 효과)
    ├── PassiveObject (자체이동, 조작X)
    └── ActiveObject (상태머신, Appendage, Equipment)
        ├── CharacterBase (CharacterInfo)
        └── MonsterBase (AI)
```

---

## 16. 구현 단계별 작업 계획

### Phase 1: Core 뼈대
**목표**: 프레임 루프가 돌아가는 최소 구조
```
파일: MonoSingleton, GameManager, ModuleBase, DWTimer, CommonFunc, CommonConsts
검증: GameManager.Update → DoUpdate 호출 → 로그 출력 확인
```

### Phase 2: Object 계층
**목표**: ObjectBase → CollisionObject → ActiveObject 계층 완성
```
파일: ObjectBase, LogicAnimator, DrawOnly, CollisionObject, PassiveObject, 
      ActiveObject, Appendage, CharacterBase, MonsterBase, ObstacleBase
검증: ObjectBase 생성 → ModuleBase에 Register → preProc/mainProc/postProc 호출 확인
```

### Phase 3: 충돌 시스템
**목표**: Logic 레벨 충돌 판정 + GC 제로 보장
```
파일: LogicColliderData, AnimationColliderData, CollisionLayer,
      CollisionManager, CollisionResult, CollisionHelper
검증: 두 CollisionObject 겹침 → hitResults에 기록 → postProc에서 처리 확인
      GC Profiler로 할당 제로 확인
```

### Phase 4: Factory + Pool
**목표**: ObjectType enum 기반 풀링, PreWarm
```
파일: ObjectType enum, ObjectFactory, ObjectPool
검증: PreWarm → Get → Return → Get 반복, GC Profiler 확인
```

### Phase 5: View 시스템
**목표**: Logic-View 분리 완성, on/off 토글
```
파일: ViewObjectManager, ObjectView
검증: View 켜고 끄고 → 로직 동일하게 동작 확인
```

### Phase 6: 입력 시스템
**목표**: 입력 큐 + PvP 대비 구조
```
파일: InputManager, IGameInput, TapInput, SwipeInput, RapidTapInput, JoystickInput
검증: 터치 → Enqueue → preProc에서 ProcessQueue → 캐릭터 상태 변경
```

### Phase 7: 미니게임 프레임워크
**목표**: 미니게임 공통 흐름 (Ready→Play→End)
```
파일: MiniGameBase, MiniGameFactory, MiniGameModule, MapManagerBase, GameType enum
검증: MiniGameModule.OnEnter → Factory → OnReady → OnPlay → OnEnd
```

### Phase 8: UI 프레임워크
**목표**: 단일 Canvas 스택 + NonLogicProc
```
파일: UIBase, UIManager
검증: Push → Pop → Replace 스택 동작 + GameManager에서 Proc 호출
```

### Phase 9: Player + Data/Info
**목표**: 플레이어 정보 + Data/Info 분리 체계
```
파일: PlayerSimpleInfo, PlayerInfo, CharacterData, CharacterInfo,
      EquipmentData, EquipmentInfo, MiniGameData, MiniGameInfo 등
검증: Data 로드 → Info 생성 → 런타임 수정 가능 확인
```

### Phase 10: 에디터 툴
**목표**: 프리팹 콜라이더 추출 → AnimationColliderData 자동 생성
```
파일: Editor/ColliderExtractor.cs
검증: 테스트 프리팹에 BoxCollider 배치 → 추출 → 데이터 파일 생성 확인
```

### Phase 11: Module 구현 (Intro, Lobby)
**목표**: 인트로/로비 기본 흐름
```
파일: IntroModule, LobbyModule
검증: Intro → Lobby 씬 전환 흐름
```

### Phase 12: 첫 미니게임 (퍼덕퍼덕)
**목표**: 전체 아키텍처 실제 검증
```
파일: FlutterFlutter/, UIFlutterFlutter
검증: 로비 → 게임 선택 → 플레이 → 게임오버 → 결과화면 전체 흐름
```

### Phase 13: 백엔드 + 네트워크 (후순위)
```
파일: FirebaseService, DataRepository, NetworkManager, InputSync, PhotonManager
```

### Phase 14: 경제 시스템 (후순위)
```
파일: CurrencyManager, GachaSystem, RewardManager
```

---

## 17. 검증 방법 (전체)

1. GameManager FRAME_TIME 기반 DoUpdate 체인 동작
2. ModuleBase → 오브젝트 pre/main/post 체인 호출
3. CollisionManager — 충돌 감지(main) → 결과 처리(post) 분리
4. GC Profiler — 충돌/풀링/입력에서 할당 제로
5. ViewObjectManager.SetEnabled(false) → 로직 정상 + 뷰 없음
6. MiniGameFactory → 게임 생성 → Ready/Play/End 흐름
7. InputManager 큐 → preProc 처리 → 상태 반영
8. UIManager Push/Pop 스택 + Proc 호출
9. ColliderExtractor 에디터 툴 → 데이터 추출 정상
