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
│   ├── PoolManager.cs                     Object + Appendage + View 통합 풀
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
│   ├── InputManager.cs                    입력 큐, preProc에서 처리 (스와이프/연타 판정 포함)
│   ├── IGameInput.cs                      인터페이스 (미니게임별 입력 타입 선언)
│   ├── TapInput.cs                        탭 입력
│   ├── SwipeInput.cs                      스와이프 입력
│   ├── RapidTapInput.cs                   연타 입력
│   └── JoystickInput.cs                   조이스틱 입력
│
├── Factory/                           — 생성
│   ├── ObjectFactory.cs                   오브젝트 생성
│   └── AppendageFactory.cs               Appendage 생성
│
├── Bundle/                            — 번들 관리
│   └── BundleManager.cs                   번들 버전 비교 + 다운로드 + 캐시 + 로드
│
├── Network/                           — PvP (추후 고도화 단계에서 구현)
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
│   ├── UILoading.cs                       로딩 UI (프로그레스바, GameSceneLoader 전용)
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

## 1-1. GameSceneLoader — 씬 전환 + 로딩

```csharp
public class GameSceneLoader : MonoSingleton<GameSceneLoader>
{
    UILoading loadingUI;         // 전체 가리는 로딩 UI (프로그레스바)
    
    // 씬 전환 요청 (어디서든 호출)
    public void LoadScene(string sceneName, ModuleBase nextModule)
    {
        StartCoroutine(LoadSequence(sceneName, nextModule));
    }
    
    IEnumerator LoadSequence(string sceneName, ModuleBase nextModule)
    {
        // 1) 로딩 UI 올리기 — 현재 화면 가리기
        loadingUI.Show();
        loadingUI.SetProgress(0f);
        
        // 2) 현재 모듈 정리
        GameManager.Instance.CurrentModule?.OnExit();
        
        // 3) 비동기 씬 로딩
        var asyncOp = SceneManager.LoadSceneAsync(sceneName);
        asyncOp.allowSceneActivation = false;
        
        while (asyncOp.progress < 0.9f)
        {
            loadingUI.SetProgress(asyncOp.progress * 0.5f); // 0~50%
            yield return null;
        }
        
        asyncOp.allowSceneActivation = true;
        yield return asyncOp;
        
        // 4) 씬 리소스 로드 (모듈별 필요 에셋)
        float moduleProgress = 0f;
        yield return nextModule.LoadResources((p) =>
        {
            moduleProgress = p;
            loadingUI.SetProgress(0.5f + p * 0.5f); // 50~100%
        });
        
        // 5) 모듈 시작
        GameManager.Instance.SetModule(nextModule);
        nextModule.OnEnter();
        
        // 6) 로딩 UI 치우기 — 다음 씬 노출
        loadingUI.Hide();
    }
}
```

**ModuleBase에 추가:**
```csharp
public abstract class ModuleBase
{
    // 기존 코드...
    
    // 씬 전환 시 호출 — 필요한 리소스 로드 (오브젝트 PreWarm, 데이터 로드 등)
    public virtual IEnumerator LoadResources(Action<float> onProgress)
    {
        onProgress?.Invoke(1f);
        yield break;
    }
}
```

**UILoading:**
- 전체 화면을 덮는 UI (UIManager 스택이 아닌 별도 관리)
- 프로그레스바 + 간단한 팁 텍스트
- GameSceneLoader가 직접 참조

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
// float3 사용 (Unity.Mathematics) — Burst 호환 필수
public struct LogicColliderData
{
    public ColliderBoxType boxType;   // Attack, Damage, Guard
    public ColliderShape shape;       // AABB, Circle, Capsule
    public float3 offset;             // 오브젝트 기준 상대 좌표
    public float3 size;               // 반크기 (AABB) 또는 x=반지름 (Circle)
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

충돌 감지(BroadPhase + NarrowPhase)는 Burst Job으로 처리. 결과는 같은 프레임 내 postProc에서 소비.

```csharp
// Job에 넘기는 경량 스냅샷 (class 참조 없음, Burst 호환)
public struct ColliderSnapshot
{
    public int objectIndex;          // objects[] 인덱스 (결과 처리용)
    public float3 position;
    public int facingDirection;
    public int collisionLayer;
    public int collisionMask;
    public int colliderStartIndex;   // AnimationColliderData.allColliders 인덱스
    public int colliderCount;
}

// 충돌 결과 (struct, Job 출력)
public struct CollisionResult
{
    public int indexA;               // ColliderSnapshot.objectIndex
    public int indexB;
    public ColliderBoxType boxTypeA;
    public ColliderBoxType boxTypeB;
    public int valueA;
    public int valueB;
}

[BurstCompile]
struct CollisionJob : IJob
{
    [ReadOnly] public NativeArray<ColliderSnapshot> snapshots;
    [ReadOnly] public NativeArray<LogicColliderData> allColliders; // AnimationColliderData.allColliders
    public NativeList<CollisionResult> results;

    public void Execute()
    {
        for (int i = 0; i < snapshots.Length; i++)
        {
            for (int j = i + 1; j < snapshots.Length; j++)
            {
                var a = snapshots[i];
                var b = snapshots[j];

                if ((a.collisionMask & b.collisionLayer) == 0 &&
                    (b.collisionMask & a.collisionLayer) == 0)
                    continue;

                // a의 활성 충돌체 vs b의 활성 충돌체
                for (int ai = a.colliderStartIndex; ai < a.colliderStartIndex + a.colliderCount; ai++)
                for (int bi = b.colliderStartIndex; bi < b.colliderStartIndex + b.colliderCount; bi++)
                {
                    var ca = allColliders[ai];
                    var cb = allColliders[bi];

                    float3 worldOffsetA = new float3(ca.offset.x * a.facingDirection, ca.offset.y, ca.offset.z);
                    float3 worldOffsetB = new float3(cb.offset.x * b.facingDirection, cb.offset.y, cb.offset.z);

                    if (CollisionHelper.Overlaps(a.position + worldOffsetA, ca,
                                                  b.position + worldOffsetB, cb))
                    {
                        results.Add(new CollisionResult
                        {
                            indexA = a.objectIndex, indexB = b.objectIndex,
                            boxTypeA = ca.boxType,  boxTypeB = cb.boxType,
                            valueA = ca.value,      valueB = cb.value,
                        });
                    }
                }
            }
        }
    }
}

public class CollisionManager
{
    NativeArray<ColliderSnapshot> snapshots;    // 프레임마다 채움
    NativeArray<LogicColliderData> allColliders; // 로딩 시 1회 할당
    NativeList<CollisionResult> results;

    public void Init(LogicColliderData[] colliderTable, int maxObjects)
    {
        allColliders = new NativeArray<LogicColliderData>(colliderTable, Allocator.Persistent);
        snapshots    = new NativeArray<ColliderSnapshot>(maxObjects, Allocator.Persistent);
        results      = new NativeList<CollisionResult>(256, Allocator.Persistent);
    }

    public void Dispose()
    {
        allColliders.Dispose();
        snapshots.Dispose();
        results.Dispose();
    }

    // mainProc에서 호출 — 스냅샷 빌드 후 Job 실행
    public void CheckCollisions(List<ObjectBase> objects)
    {
        int count = 0;
        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] is not CollisionObject co) continue;
            snapshots[count++] = co.BuildSnapshot(i);
        }

        results.Clear();
        var job = new CollisionJob
        {
            snapshots    = snapshots.GetSubArray(0, count),
            allColliders = allColliders,
            results      = results,
        };
        job.Schedule().Complete();
    }

    // postProc에서 호출 — 실제 결과 처리
    public void ProcessResults(List<ObjectBase> objects)
    {
        for (int i = 0; i < results.Length; i++)
        {
            var r = results[i];
            var a = objects[r.indexA] as CollisionObject;
            var b = objects[r.indexB] as CollisionObject;
            a?.OnCollision(b, r.boxTypeA, r.valueB);
            b?.OnCollision(a, r.boxTypeB, r.valueA);
        }
    }
}
```

### 5-4. CollisionHelper (순수 수학)

`[BurstCompile]` static 클래스. float3 (Unity.Mathematics) 사용. Burst Job 내부에서 직접 호출.

```csharp
using Unity.Mathematics;
using Unity.Burst;

[BurstCompile]
public static class CollisionHelper
{
    // 진입점 — shape 조합 분기
    public static bool Overlaps(float3 posA, in LogicColliderData a,
                                 float3 posB, in LogicColliderData b)
    {
        return (a.shape, b.shape) switch
        {
            (ColliderShape.AABB,   ColliderShape.AABB)   => OverlapAABB(posA, a.size, posB, b.size),
            (ColliderShape.Circle, ColliderShape.Circle) => OverlapCircle(posA, a.size.x, posB, b.size.x),
            (ColliderShape.AABB,   ColliderShape.Circle) => OverlapAABBvsCircle(posA, a.size, posB, b.size.x),
            (ColliderShape.Circle, ColliderShape.AABB)   => OverlapAABBvsCircle(posB, b.size, posA, a.size.x),
            _ => false,
        };
    }

    static bool OverlapAABB(float3 posA, float3 sizeA, float3 posB, float3 sizeB)
    {
        var d = math.abs(posA - posB);
        return d.x < sizeA.x + sizeB.x &&
               d.y < sizeA.y + sizeB.y;
    }

    static bool OverlapCircle(float3 posA, float rA, float3 posB, float rB)
    {
        float3 d = posA - posB;
        float r = rA + rB;
        return d.x * d.x + d.y * d.y < r * r; // sqrt 안 씀
    }

    static bool OverlapAABBvsCircle(float3 boxPos, float3 boxSize, float3 circPos, float r)
    {
        float3 closest = math.clamp(circPos, boxPos - boxSize, boxPos + boxSize);
        float3 d = circPos - closest;
        return d.x * d.x + d.y * d.y < r * r;
    }
    // Capsule 등 추가 가능
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
    
    public void SetEnabled(bool enabled) { isEnabled = enabled; }
    
    // Logic → View 동기화 (GameManager.DoUpdate 맨 끝에서 호출)
    public void SyncAll(List<ObjectBase> objects)
    {
        if (!isEnabled) return;
        
        for (int i = 0; i < objects.Count; i++)
        {
            var logic = objects[i];
            
            // View 없으면 생성
            if (!activeViews.TryGetValue(logic.objectId, out var view))
            {
                view = ViewPool.Get(logic.poolKey);
                activeViews[logic.objectId] = view;
            }
            
            // 값이 다른 것만 갱신
            if (view.position != logic.position)
                view.SetPosition(logic.position);
            
            if (view.rotation != logic.rotation)
                view.SetRotation(logic.rotation);
            
            if (view.currentAnimId != logic.logicAnimator.currentAnimId ||
                view.currentFrame != logic.logicAnimator.currentFrame)
                view.SetAnimation(logic.logicAnimator.currentAnimId, 
                                  logic.logicAnimator.currentFrame);
            
            if (view.facingDirection != logic.facingDirection)
                view.SetFacing(logic.facingDirection);
            
            if (view.isVisible != logic.isVisible)
                view.SetVisible(logic.isVisible);
        }
        
        // Logic에서 삭제된 오브젝트의 View 회수
        CleanupOrphanedViews(objects);
    }
    
    void CleanupOrphanedViews(List<ObjectBase> objects) { ... }
    
    public void DestroyView(int objectId)
    {
        if (activeViews.Remove(objectId, out var view))
            ViewPool.Return(view);
    }
}
```

```csharp
public class ObjectView : MonoBehaviour
{
    public Vector3 position;
    public Quaternion rotation;
    public int currentAnimId;
    public int currentFrame;
    public int facingDirection;
    public bool isVisible;
    
    public void SetPosition(Vector3 pos) { position = pos; transform.position = pos; }
    public void SetRotation(Quaternion rot) { rotation = rot; transform.rotation = rot; }
    public void SetAnimation(int animId, int frame) { ... }  // SpriteRenderer 또는 Animator 제어
    public void SetFacing(int dir) { facingDirection = dir; transform.localScale = new Vector3(dir, 1, 1); }
    public void SetVisible(bool visible) { isVisible = visible; gameObject.SetActive(visible); }
}
```

**ViewPool:** GameObject 전용 풀 (ObjectPool과 별도). ObjectPool은 Logic 객체, ViewPool은 GameObject.

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

### InputData (struct, GC 없음)

```csharp
public enum InputType
{
    TouchDown,      // 터치 시작
    TouchMove,      // 터치 이동 (드래그)
    TouchUp,        // 터치 종료
    JoystickMove,   // 조이스틱 입력
}

public struct InputData
{
    public InputType inputType;
    public int fingerId;            // 멀티터치 구분 (0, 1, 2...)
    public Vector2 position;        // 터치 좌표 (스크린 또는 월드)
    public Vector2 joystickValue;   // 조이스틱 방향+크기 (-1~1)
    public int frameNumber;         // PvP 동기화용 (gameTimer 기반 프레임 번호)
}
```

**스와이프 판정 — InputManager에서 처리:**
- TouchDown 시 `fingerId`별로 시작 position 저장
- TouchUp 시 시작 position과 종료 position 차이(delta)로 스와이프 판정
- 별도 `SwipeInput` 클래스 불필요 — InputManager가 `fingerId` 추적하여 판정
- 판정 기준: delta 거리 > threshold && delta 방향으로 방향 결정

**연타(RapidTap) 판정 — InputManager에서 처리:**
- TouchDown 빈도를 시간 윈도우 내에서 카운트
- 일정 횟수 이상이면 RapidTap으로 판정
- 별도 `RapidTapInput` 클래스 불필요

> **정리**: `IGameInput` 구현체(TapInput, SwipeInput, RapidTapInput, JoystickInput)는 "이 미니게임이 어떤 입력을 받을지" 선언하는 역할. 실제 스와이프/연타 판정 로직은 InputManager에 집중.

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

### MiniGameBase + 상태 전이

```csharp
public enum MiniGameState { None, Ready, Playing, Paused, End }

public abstract class MiniGameBase
{
    public abstract GameType GameType { get; }
    public MiniGameState State { get; private set; } = MiniGameState.None;
    
    public abstract void OnReady(), OnPlay(), OnPause(), OnResume(), OnEnd(), OnRestart();
    public virtual void mainProc(float dt) { }
    public abstract float GetScore();
    public abstract IGameInput CreateInput();
    public abstract MapManagerBase CreateMapManager();
    
    // MiniGameModule에서만 호출
    internal void SetState(MiniGameState state) { State = state; }
}
```

**상태 전이 규칙 — MiniGameModule이 관리:**
```
None → Ready → Playing → End
                 ↕
               Paused

전이 트리거:
- None → Ready   : MiniGameModule.OnEnter() 내부
- Ready → Playing : UI 카운트다운 완료 후 UI가 MiniGameModule.StartGame() 호출
- Playing → Paused : UI 일시정지 버튼 → MiniGameModule.PauseGame()
- Paused → Playing : UI 재개 버튼 → MiniGameModule.ResumeGame()
- Playing → End    : 게임 내부 조건 충족 → MiniGameModule.EndGame()
- End → Ready      : UI 재시작 버튼 → MiniGameModule.RestartGame()
```

```csharp
// MiniGameModule 전이 메서드
public class MiniGameModule : ModuleBase
{
    public void StartGame()   { CurrentGame.SetState(MiniGameState.Playing); CurrentGame.OnPlay(); }
    public void PauseGame()   { CurrentGame.SetState(MiniGameState.Paused);  CurrentGame.OnPause(); }
    public void ResumeGame()  { CurrentGame.SetState(MiniGameState.Playing); CurrentGame.OnResume(); }
    public void EndGame()     { CurrentGame.SetState(MiniGameState.End);     CurrentGame.OnEnd(); }
    public void RestartGame() { CurrentGame.SetState(MiniGameState.Ready);   CurrentGame.OnRestart(); }
    
    // mainProc에서 Playing 상태일 때만 게임 로직 실행
    public override void mainProc(float dt)
    {
        if (CurrentGame.State == MiniGameState.Playing)
        {
            base.mainProc(dt);
            CurrentGame.mainProc(dt);
            collisionManager.CheckCollisions(objects);
        }
    }
}
```

### MapManagerBase — 맵 생성/관리 추상

```csharp
public abstract class MapManagerBase
{
    // === 공통 정보 (모든 미니게임 공유) ===
    public float mapWidth;          // 맵 가로 크기
    public float mapHeight;         // 맵 세로 크기
    public float scrollSpeed;       // 맵 스크롤 속도
    public Rect spawnArea;          // 오브젝트 스폰 가능 영역
    public Rect despawnArea;        // 오브젝트 회수 영역 (화면 밖)
    
    public virtual void Init(MapData mapData)
    {
        // 공통 정보 세팅
        mapWidth = mapData.width;
        mapHeight = mapData.height;
        scrollSpeed = mapData.baseScrollSpeed;
        spawnArea = mapData.spawnArea;
        despawnArea = mapData.despawnArea;
    }
    
    public abstract void OnReady();     // 맵 초기 세팅
    public abstract void mainProc(float dt);  // 스크롤, 스폰 타이밍 등
    public abstract void OnEnd();       // 맵 정리
}
```

**각 미니게임의 MapManager 구현체:**
- `MapManagerBase`를 상속하여 `Init()` 오버라이드
- `MapData`에서 미니게임별 디테일 정보를 읽어 세팅

> **중요**: 맵별 고유 정보(장애물 배치 규칙, 난이도 곡선, 지형 패턴 등)는 각 맵의 구현체에서 반드시 세팅할 것. `MapManagerBase`는 공통 정보만 보유하며, 게임별 세부 로직은 구현체 책임.

```csharp
// 예시: 퍼덕퍼덕 맵 매니저
public class FlutterFlutterMapManager : MapManagerBase
{
    float pipeGap;              // 파이프 간격
    float pipeSpawnInterval;    // 파이프 생성 주기
    float difficultyTimer;      // 난이도 상승 타이머
    
    public override void Init(MapData mapData)
    {
        base.Init(mapData);
        // MapData에서 게임 고유 정보 세팅
        pipeGap = mapData.GetFloat("pipeGap");
        pipeSpawnInterval = mapData.GetFloat("pipeSpawnInterval");
    }
    
    public override void OnReady() { /* 초기 파이프 배치 */ }
    public override void mainProc(float dt) { /* 스크롤 + 스폰 + 난이도 조절 */ }
    public override void OnEnd() { /* 정리 */ }
}
```

### GameType enum
ThumpThumpSlope, FlutterFlutter, WaddleSprint, HoppyForest, SlideRun, NarrowPath, DodgeVillain

---

## 8-1. Network (PvP) — 현재 상태

> **네트워크(PvP)는 추후 고도화 단계에서 진행. 현재는 싱글 플레이 우선 구현.**

- `Network/` 폴더(NetworkManager, InputSync, PhotonManager)는 폴더 구조에만 예약
- 구현은 후순위 (Phase 13)
- 현재 InputData의 `frameNumber` 필드만 PvP 대비로 미리 포함
- 싱글 플레이에서는 `frameNumber`를 무시해도 무방

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

### PoolManager (GameManager가 소유, 모든 풀 통합 관리)

```csharp
public class PoolManager
{
    // === Object 풀 ===
    Dictionary<ObjectType, Queue<ObjectBase>> objectPools;
    
    public void PreWarmObject(ObjectType type, int count) { ... }
    
    public T GetObject<T>(ObjectType type) where T : ObjectBase, new()
    {
        if (objectPools[type].Count > 0)
        {
            var obj = (T)objectPools[type].Dequeue();
            obj.OnSpawn();
            return obj;
        }
        return ObjectFactory.Create<T>(type);
    }
    
    public void ReturnObject(ObjectBase obj)
    {
        obj.OnDespawn();
        objectPools[obj.poolKey].Enqueue(obj);
    }
    
    // === Appendage 풀 ===
    Dictionary<AppendageType, Queue<Appendage>> appendagePools;
    
    public void PreWarmAppendage(AppendageType type, int count) { ... }
    
    public Appendage GetAppendage(AppendageType type)
    {
        if (appendagePools[type].Count > 0)
        {
            var app = appendagePools[type].Dequeue();
            app.Reset();
            return app;
        }
        return AppendageFactory.Create(type);
    }
    
    public void ReturnAppendage(Appendage app)
    {
        app.OnDetach(app.owner);
        appendagePools[app.type].Enqueue(app);
    }
    
    // === View 풀 (GameObject) ===
    Dictionary<ObjectType, Queue<ObjectView>> viewPools;
    
    public ObjectView GetView(ObjectType type) { ... }
    public void ReturnView(ObjectView view) { ... }
    
    // === 전체 정리 ===
    public void ClearAll() { ... }  // 씬 전환 시
}
```

```csharp
// GameManager에서 통합 관리
public class GameManager : MonoSingleton<GameManager>
{
    public PoolManager Pool { get; private set; }
    // 호출: GameManager.Instance.Pool.GetObject<CharacterBase>(ObjectType.Character_Duck_Default)
    // 호출: GameManager.Instance.Pool.GetAppendage(AppendageType.SpeedUp)
}
```

**GC 최소화:**
- PreWarm으로 로딩 시 충분한 풀 생성 (Object, Appendage, View 전부)
- OnSpawn/OnDespawn/Reset에서 필드 초기화만 (new 없음)
- 컬렉션은 Clear() (capacity 유지)
- InputData, CollisionResult는 struct

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

## 12. Data / Info 분리 + 번들 파이프라인

| 구분 | 폴더 | 특성 | 예시 |
|------|------|------|------|
| Data | Data/ | 테이블 원본, 불변, 로딩 시 1회 생성 | CharacterData (기본 스탯, 이름) |
| Info | Info/ | 런타임 가공, 변경 가능 | CharacterInfo (현재 레벨, 장비 합산 스탯) |

모든 시스템에 적용: Character, Equipment, Map, MiniGame, Obstacle 등

### 12-1. 데이터 파이프라인

```
[에디터 타임]
Google Drive → Google Sheet (원본 데이터 테이블)
    ↓ (에디터 툴로 파싱)
ScriptableObject 에셋 생성 (CharacterData, MapData 등)
    ↓ (빌드)
AssetBundle로 패킹
    ↓ (배포)
서버(Firebase Storage 등)에 업로드
```

### 12-2. 번들 로딩 (IntroModule)

```
[런타임 — Intro 씬]
1) 서버에서 번들 버전 체크 (로컬 vs 서버)
2) 새 번들이 있으면 다운로드 → 디바이스 저장
3) 디바이스에 저장된 번들 전체 로드 → 메모리에 올림
4) DataRepository에 Data 객체 등록
5) 완료 → 로비 씬 전환
```

```csharp
public class DataRepository : MonoSingleton<DataRepository>
{
    // Data 타입별 Dictionary
    Dictionary<int, CharacterData> characterTable;
    Dictionary<int, EquipmentData> equipmentTable;
    Dictionary<int, MiniGameData> miniGameTable;
    Dictionary<int, MapData> mapTable;
    Dictionary<int, ObstacleData> obstacleTable;
    
    // 번들에서 로드한 ScriptableObject를 파싱하여 등록
    public void RegisterFromBundle(AssetBundle bundle)
    {
        var characters = bundle.LoadAllAssets<CharacterDataAsset>();
        foreach (var asset in characters)
            characterTable[asset.id] = asset.ToData();
        // 다른 테이블도 동일 패턴
    }
    
    // 조회 (런타임 어디서든)
    public CharacterData GetCharacter(int id) => characterTable[id];
    public MapData GetMap(int id) => mapTable[id];
    // ...
}
```

```csharp
// IntroModule.LoadResources 에서 번들 로딩
public class IntroModule : ModuleBase
{
    public override IEnumerator LoadResources(Action<float> onProgress)
    {
        // 1) 번들 버전 체크
        yield return BundleManager.CheckUpdate();
        onProgress?.Invoke(0.2f);
        
        // 2) 새 번들 다운로드 (있으면)
        yield return BundleManager.DownloadIfNeeded((p) => onProgress?.Invoke(0.2f + p * 0.4f));
        
        // 3) 로컬 번들 로드 → 메모리
        yield return BundleManager.LoadAll((p) => onProgress?.Invoke(0.6f + p * 0.3f));
        
        // 4) DataRepository에 등록
        DataRepository.Instance.RegisterAll(BundleManager.LoadedBundles);
        onProgress?.Invoke(1f);
    }
}
```

### 12-3. BundleManager — 번들 버전 관리 + 다운로드 + 캐시

```csharp
public class BundleManager
{
    // === 상수 ===
    const string VERSION_URL = "https://storage.googleapis.com/.../bundle_version.json";
    const string BUNDLE_BASE_URL = "https://storage.googleapis.com/.../bundles/";
    const string LOCAL_VERSION_KEY = "BundleVersionHash";  // PlayerPrefs 키
    const int MAX_RETRY = 3;
    
    // === 번들 버전 매니페스트 (서버 JSON) ===
    // bundle_version.json 예시:
    // {
    //   "version": "1.0.3",
    //   "bundles": [
    //     { "name": "character", "hash": "a1b2c3d4", "size": 1024000 },
    //     { "name": "map",       "hash": "e5f6g7h8", "size": 512000 },
    //     { "name": "obstacle",  "hash": "i9j0k1l2", "size": 256000 },
    //     { "name": "ui",        "hash": "m3n4o5p6", "size": 768000 }
    //   ]
    // }
    
    [System.Serializable]
    public class BundleManifest
    {
        public string version;
        public BundleEntry[] bundles;
    }
    
    [System.Serializable]
    public class BundleEntry
    {
        public string name;
        public string hash;     // 번들별 해시값 (변경 감지용)
        public long size;       // 바이트 (다운로드 프로그레스 계산용)
    }
    
    // === 로컬 캐시 매니페스트 ===
    // Application.persistentDataPath/bundles/local_manifest.json 에 저장
    // 다운로드 완료된 번들의 name→hash 매핑
    [System.Serializable]
    class LocalManifest
    {
        public Dictionary<string, string> bundleHashes = new();  // name → hash
    }
    
    static BundleManifest serverManifest;
    static LocalManifest localManifest;
    static Dictionary<string, AssetBundle> loadedBundles = new();
    
    static string BundlePath => Path.Combine(Application.persistentDataPath, "bundles");
    static string LocalManifestPath => Path.Combine(BundlePath, "local_manifest.json");
    
    // ──────────────────────────────────
    // 1) 버전 비교
    // ──────────────────────────────────
    public static IEnumerator CheckUpdate()
    {
        // 서버 매니페스트 다운로드
        using var request = UnityWebRequest.Get(VERSION_URL);
        yield return request.SendWebRequest();
        
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"[BundleManager] 버전 체크 실패: {request.error}");
            // 오프라인 모드 — 로컬 번들만 사용
            yield break;
        }
        
        serverManifest = JsonUtility.FromJson<BundleManifest>(request.downloadHandler.text);
        
        // 로컬 매니페스트 로드
        if (File.Exists(LocalManifestPath))
            localManifest = JsonUtility.FromJson<LocalManifest>(File.ReadAllText(LocalManifestPath));
        else
            localManifest = new LocalManifest();
    }
    
    // ──────────────────────────────────
    // 2) 변경된 번들만 다운로드 (해시 기반 캐시)
    // ──────────────────────────────────
    public static IEnumerator DownloadIfNeeded(Action<float> onProgress)
    {
        if (serverManifest == null) { onProgress?.Invoke(1f); yield break; }
        
        // 변경된 번들 목록 필터링
        var toDownload = new List<BundleEntry>();
        foreach (var entry in serverManifest.bundles)
        {
            if (!localManifest.bundleHashes.TryGetValue(entry.name, out var localHash)
                || localHash != entry.hash)
            {
                toDownload.Add(entry);
            }
        }
        
        if (toDownload.Count == 0) { onProgress?.Invoke(1f); yield break; }
        
        // 전체 다운로드 크기 계산 (프로그레스용)
        long totalSize = 0;
        foreach (var e in toDownload) totalSize += e.size;
        long downloaded = 0;
        
        Directory.CreateDirectory(BundlePath);
        
        foreach (var entry in toDownload)
        {
            bool success = false;
            
            for (int retry = 0; retry < MAX_RETRY; retry++)
            {
                string url = BUNDLE_BASE_URL + entry.name;
                using var request = UnityWebRequest.Get(url);
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    // 디바이스에 저장
                    string filePath = Path.Combine(BundlePath, entry.name);
                    File.WriteAllBytes(filePath, request.downloadHandler.data);
                    
                    // 로컬 매니페스트 갱신
                    localManifest.bundleHashes[entry.name] = entry.hash;
                    success = true;
                    break;
                }
                
                Debug.LogWarning($"[BundleManager] 다운로드 재시도 ({retry + 1}/{MAX_RETRY}): {entry.name}");
                yield return new WaitForSeconds(1f);  // 재시도 전 대기
            }
            
            if (!success)
            {
                Debug.LogError($"[BundleManager] 다운로드 실패: {entry.name} — 오프라인 모드 전환 또는 에러 팝업");
                // TODO: UIManager에서 에러 팝업 표시 → 재시도/오프라인 선택
                yield break;
            }
            
            downloaded += entry.size;
            onProgress?.Invoke((float)downloaded / totalSize);
        }
        
        // 로컬 매니페스트 저장
        File.WriteAllText(LocalManifestPath, JsonUtility.ToJson(localManifest));
    }
    
    // ──────────────────────────────────
    // 3) 로컬 번들 → 메모리 로드
    // ──────────────────────────────────
    public static IEnumerator LoadAll(Action<float> onProgress)
    {
        string[] bundleFiles = Directory.GetFiles(BundlePath, "*", SearchOption.TopDirectoryOnly);
        // local_manifest.json 제외
        var validFiles = new List<string>();
        foreach (var f in bundleFiles)
            if (!f.EndsWith(".json")) validFiles.Add(f);
        
        for (int i = 0; i < validFiles.Count; i++)
        {
            var bundleRequest = AssetBundle.LoadFromFileAsync(validFiles[i]);
            yield return bundleRequest;
            
            if (bundleRequest.assetBundle != null)
            {
                string bundleName = Path.GetFileName(validFiles[i]);
                loadedBundles[bundleName] = bundleRequest.assetBundle;
            }
            
            onProgress?.Invoke((float)(i + 1) / validFiles.Count);
        }
    }
    
    // === 외부 접근 ===
    public static Dictionary<string, AssetBundle> LoadedBundles => loadedBundles;
    
    public static void UnloadAll()
    {
        foreach (var bundle in loadedBundles.Values)
            bundle.Unload(true);
        loadedBundles.Clear();
    }
}
```

**요약:**
| 단계 | 방식 | 세부 |
|------|------|------|
| 버전 비교 | 서버 `bundle_version.json` vs 로컬 `local_manifest.json` | 번들별 해시값 비교 |
| 캐시 전략 | 해시 기반 — 해시가 같으면 스킵, 다르면 재다운로드 | `Application.persistentDataPath/bundles/`에 저장 |
| 다운로드 | `UnityWebRequest`로 번들 파일 다운로드 | 변경된 번들만 선별 다운로드 |
| 에러 처리 | 실패 시 최대 3회 재시도, 이후 오프라인 모드 또는 에러 팝업 | 재시도 간 1초 대기 |
| 로드 | `AssetBundle.LoadFromFileAsync`로 메모리에 올림 | 비동기, 프로그레스 콜백 제공 |

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
├── PoolManager (Object + Appendage + View 통합 풀)
│   ├── objectPools: Dictionary<ObjectType, Queue<ObjectBase>>
│   ├── appendagePools: Dictionary<AppendageType, Queue<Appendage>>
│   ├── viewPools: Dictionary<ObjectType, Queue<ObjectView>>
│   └── ObjectFactory, AppendageFactory
│
├── CurrentModule: ModuleBase
│   │ - objects: List<ObjectBase>
│   │ - pre/main/postProc 체인
│   │ - LoadResources(onProgress) — 씬 전환 시 리소스 로드
│   │
│   ├── IntroModule (번들 체크/다운로드/로드 → DataRepository 등록)
│   ├── LobbyModule
│   └── MiniGameModule
│       ├── currentGameType, currentGame: MiniGameBase (상태: Ready/Playing/Paused/End)
│       ├── mapManager: MapManagerBase
│       └── collisionManager: CollisionManager
│
├── GameSceneLoader (MonoSingleton)
│   └── UILoading (프로그레스바, 씬 전환 시 가리개)
│
├── ViewObjectManager (MonoSingleton)
│   └── SyncAll(): position/rotation/anim/facing/visible 비교 후 갱신
│
├── InputManager (MonoSingleton)
│   ├── IGameInput (Tap, Swipe, RapidTap, Joystick)
│   ├── inputQueue
│   └── InputSync (PvP)
│
├── CollisionManager ([BurstCompile] Job 기반)
│   ├── NativeArray<ColliderSnapshot> (프레임마다 빌드)
│   ├── NativeArray<LogicColliderData> (로딩 시 1회 할당)
│   ├── NativeList<CollisionResult> (Job 출력)
│   ├── CheckCollisions() — Schedule + Complete — mainProc
│   └── ProcessResults() — postProc
│
├── UIManager (MonoSingleton, 단일 Canvas 스택)
│   └── UIBase → 화면/팝업
│
├── PlayerInfo (현재 플레이어)
├── CurrencyManager
├── DataRepository (MonoSingleton, 번들에서 로드한 Data 테이블)
├── BundleManager (번들 버전 체크/다운로드/로드)
└── FirebaseService (MonoSingleton)

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
파일: MonoSingleton, GameManager, ModuleBase, PoolManager, DWTimer, CommonFunc, CommonConsts
검증: GameManager.Update → DoUpdate 호출 → 로그 출력 확인
      PoolManager 기본 동작 (PreWarm, Get, Return) 확인
```

### Phase 2: Object 계층
**목표**: ObjectBase → CollisionObject → ActiveObject 계층 완성
```
파일: ObjectBase, LogicAnimator, DrawOnly, CollisionObject, PassiveObject, 
      ActiveObject, Appendage, CharacterBase, MonsterBase, ObstacleBase
검증: ObjectBase 생성 → ModuleBase에 Register → preProc/mainProc/postProc 호출 확인
```

### Phase 3: 충돌 시스템
**목표**: Logic 레벨 충돌 판정 + GC 제로 보장 + Burst/NativeArray 구조
```
파일: LogicColliderData, AnimationColliderData, CollisionLayer,
      CollisionManager, CollisionResult, CollisionHelper
구조: ColliderSnapshot/LogicColliderData는 NativeArray, CollisionJob은 [BurstCompile]
검증: 두 CollisionObject 겹침 → hitResults에 기록 → postProc에서 처리 확인
      GC Profiler로 할당 제로 확인, Burst Inspector로 컴파일 확인
```

### Phase 4: Factory
**목표**: ObjectFactory + AppendageFactory 생성 로직
```
파일: ObjectType enum, ObjectFactory, AppendageFactory
검증: Factory로 오브젝트/Appendage 생성 → PoolManager 연동 확인
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
파일: UIBase, UIManager, UILoading
검증: Push → Pop → Replace 스택 동작 + GameManager에서 Proc 호출
      UILoading 프로그레스바 동작 확인
```

### Phase 9: Player + Data/Info + Bundle + 씬 전환
**목표**: 플레이어 정보 + Data/Info 분리 체계 + 번들 파이프라인 + 씬 전환
```
파일: PlayerSimpleInfo, PlayerInfo, CharacterData, CharacterInfo,
      EquipmentData, EquipmentInfo, MiniGameData, MiniGameInfo,
      BundleManager, DataRepository, GameSceneLoader
검증: BundleManager 버전 체크 → 다운로드 → 로드 → DataRepository 등록
      Data 로드 → Info 생성 → 런타임 수정 가능 확인
      GameSceneLoader 씬 전환 + 로딩 UI 프로그레스바 동작
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
검증: Intro (번들 로드) → Lobby 씬 전환 흐름
```

### Phase 12: 첫 미니게임 (퍼덕퍼덕)
**목표**: 전체 아키텍처 실제 검증
```
파일: FlutterFlutter/, UIFlutterFlutter
검증: 로비 → 게임 선택 → 플레이 → 게임오버 → 결과화면 전체 흐름
```

### Phase 13: 백엔드 + 네트워크 (후순위)
**목표**: Firebase 연동 + PvP 기반
```
파일: FirebaseService, NetworkManager, InputSync, PhotonManager
비고: 네트워크(PvP)는 싱글 플레이 안정화 이후 진행
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
