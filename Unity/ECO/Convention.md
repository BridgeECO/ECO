\# ECO 프로그래밍 컨벤션(2026)



\## 개 요

프로젝트 ECO의 프로그래밍 컨벤션을 작성해놓은 페이지입니다.

경직되지 않은 유연한 컨벤션을 지향합니다. 언제든 토의 및 토론을 통해 변경될 수 있습니다.



\## 목 차

1\. \*\*Coding Convention\*\*

&#x20;  - 1-a. Naming Convention 및 Indentation

&#x20;  - 1-b. 필드 및 프로퍼티

&#x20;  - 1-c. 이벤트 메서드(생명 주기 메서드) 선언

&#x20;  - 1-d. null 체크

&#x20;  - 1-e. 싱글톤

&#x20;  - 1-f. Inspector 가독성

&#x20;  - 1-g. 대리자

&#x20;  - 1-h. 스크립트 파일 분리

2\. \*\*Github\*\*

&#x20;  - 2-a. Commit Message Convention

&#x20;  - 2-b. PR Convention

&#x20;  - 2-c. Branch Convention

3\. \*\*Unity Editor\*\*

&#x20;  - 3-a. 폴더 정리



\---



\## 1. Coding Convention



\### 1-a. Naming Convention 및 Indentation



\*\*1-a-(1).\*\* 본 프로젝트의 네이밍 컨벤션은 \[C# at Google Style Guide](https://google.github.io/styleguide/csharp-style.html)를 준수한다.

\- 해당 문서에 별도로 명시되지 않은 세부 컨벤션은 위 가이드를 준수하면 된다.



\*\*1-a-(2).\*\* 아래와 같은 기능을 담당하는 메서드의 경우, 아래와 같은 형식의 메서드 작명 규칙을 준수해야 한다. 적절한 메서드명이 떠오르지 않을 경우, PR에 해당 내용을 추가한다.



\* \*\*기능 : 초기화\*\*

&#x20; \* \*\*메서드명 : Init + \[초기화 대상]\*\*

&#x20; \* 예시: `InitPlayerData()`, `InitMapInfo()`

\* \*\*기능 : 변경(이미 초기화된 대상에 대해 내부 데이터가 변화하거나 다른 대상을 대입할 때)\*\*

&#x20; \* \*\*메서드명 : Set + \[변경 대상]\*\*

&#x20; \* 예시: `SetQuest()`, `SetAdventurerData()`

\* \*\*기능 : UI에 데이터 반영\*\*

&#x20; \* \*\*메서드명 : Refresh + \[대상](필요에 따라 생략 가능)\*\*

&#x20; \* 예시: `RefreshAdventurerCard()`, `RefreshQuest()`



\*\*1-a-(3).\*\* If문 내부의 로직이 단순 return과 같은 1줄의 형태더라도, 항상 중괄호를 연다.

\* ❌ \*\*지양할 예시\*\*

&#x20; ```csharp

&#x20; if (a == null) return;

&#x20; ```

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; if (a == null)

&#x20; {

&#x20;     return;

&#x20; }

&#x20; ```



\*\*1-a-(4).\*\* 리스트, 해시 테이블 등의 컨테이너를 사용할 땐, 항상 필드명의 끝에 복수를 의미하는 \*\*“s”\*\*를 접미어로 붙여준다. \*\*필드명에 컨테이너의 이름을 포함하는 것은 지양한다.\*\*

\* ❌ \*\*지양할 예시\*\*

&#x20; ```csharp

&#x20; private Dictionary<string, int> \_scoreDictionary = new Dictionary<string, int>();

&#x20; private List<string> \_userNicknameList = new List<string>();

&#x20; ```

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; private Dictionary<string, int> \_scores = new Dictionary<string, int>();

&#x20; private List<string> \_userNicknames = new List<string>();

&#x20; ```



\*\*1-a-(5).\*\* \*\*인터페이스\*\* 추가 시, 인터페이스명에 접두사 \*\*I\*\*를 명시한다.

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; public interface IPoolable {}

&#x20; ```



\*\*1-a-(6).\*\* \*\*열거형(enum)\*\* 추가 시, 열거형명에 \*\*접두사 E\*\*를 명시한다.

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; public enum EPoolableType {}

&#x20; ```



\*\*1-a-(7).\*\* \*\*bool\*\* 변수의 접두사로는 항상 `Is`, `Has`, `Can`만 사용한다.

\- `Need`, `Should` 등의 접두사는 사용하지 않는다.

\* ❌ \*\*지양할 예시\*\*

&#x20; ```csharp

&#x20; public bool NeedMoney { get; private set; }

&#x20; private bool \_shouldRecovery;

&#x20; ```

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; public bool IsWorking { get; set; }

&#x20; private bool \_hasEnergy;

&#x20; private bool \_canDo;

&#x20; ```



\*\*1-a-(8).\*\* \*\*const\*\* 변수는 \*\*대문자 스네이크 케이스\*\*로 작성한다.

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; public const float FACTORY\_PRODUCTION\_INTERVAL;

&#x20; public const float PLAYER\_SKELETON\_XOFFSET;

&#x20; public const float PLAYER\_SKELETON\_YOFFSET;

&#x20; ```



\*\*1-a-(9).\*\* \*\*UI 로직을 수행하는 클래스\*\* 추가 시, 클래스명에 \*\*접두사 UI\_\*\*를 명시한다.

\- UI 로직은 \*\*인게임 로직과 동일한 클래스에 존재하지 않도록 명확한 책임 분리를 수행\*\*한다.

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; public class UI\_Joystick : MonoBehaviour {}

&#x20; ```



\### 1-b. 필드 및 프로퍼티



\*\*1-b-(1).\*\* 클래스와 구조체 내 모든 필드는 \*\*private 필드\*\*로 선언한다.



\*\*1-b-(2).\*\* 단, \*\*Serializeable 클래스와 Scirptable Object(SO) 클래스, DTO(Data Transfer Object) 클래스\*\*에 한해 public 필드를 선언할 수 있다.



\*\*1-b-(3).\*\* \*\*대리자(delegate, Action, Func 등) 필드\*\*는, \*\*항상 public 필드\*\*로 선언한다. (1-g에서 자세히 설명한다.)



\*\*1-b-(4).\*\* `\[SerializeField]` 속성을 가진 필드를 선언할 때, 선언부를 한줄에 작성하지 않고 `\[SerializeField]` 속성 작성 후 줄바꿈을 진행한다.

\* ❌ \*\*지양할 예시\*\*

&#x20; ```csharp

&#x20; \[SerializeField] private int \_movementSpeed;

&#x20; ```

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; \[SerializeField] 

&#x20; private int \_movementSpeed;

&#x20; ```



\*\*1-b-(5).\*\* `\[SerializeField]` 속성을 가진 필드는 코드로 기본값을 할당하지 않고, 항상 Inspector 상에서 값을 할당한다.

\* ❌ \*\*지양할 예시\*\*

&#x20; ```csharp

&#x20; \[SerializeField] 

&#x20; private int \_movementSpeed = 5f;

&#x20; ```



\*\*1-b-(6).\*\* \*\*필드를 클래스 외부에서 접근해야 하는 경우\*\*에만 프로퍼티를 선언하고, protected 또는 public으로 선언한다.

\- 즉, 프로퍼티의 선언은 필수가 아니다.



\*\*1-b-(7).\*\* 프로퍼티 선언 방식은 네이밍 컨벤션을 제외하고 별도의 제한을 두지 않는다.

\- \*\*백업 필드 + 프로퍼티\*\* 또는 \*\*자동구현 프로퍼티\*\* 둘 중 어느 것을 사용해도 무방하다.

\- 프로퍼티 접근자의 선언 방식 역시 별도의 제한을 두지 않는다.

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; private int \_count;

&#x20; public int Count {get => \_count; set => \_count = value; }

&#x20; 

&#x20; private int \_count;

&#x20; public int Count 

&#x20; {

&#x20;     get { return \_count; }

&#x20;     set 

&#x20;     {

&#x20;         \_count = value;

&#x20;         OnCountChanged?.Invoke();

&#x20;     }

&#x20; }

&#x20; 

&#x20; public int Count { get; private set; }

&#x20; ```



\*\*1-b-(8).\*\* 필드는 클래스 최상단에 선언하고, 모든 필드가 선언된 후 필드 하단에 프로퍼티를 선언한다. 백업 필드와 프로퍼티를 함께 선언하지 않는다.

\* ❌ \*\*지양할 예시\*\*

&#x20; ```csharp

&#x20; \[SerializeField]

&#x20; private int \_count;

&#x20; public int Count {get => \_count; private set => \_count = value; }

&#x20; 

&#x20; \[SerializeField]

&#x20; private int \_speed;

&#x20; public int Speed {get => \_speed; private set => \_speed = value; }

&#x20; ```

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; \[SerializeField]

&#x20; private int \_count;

&#x20; \[SerializeField]

&#x20; private int \_speed;

&#x20; 

&#x20; public int Count {get => \_count; private set => \_count = value; }

&#x20; public int Speed {get => \_speed; private set => \_speed = value; }

&#x20; ```



\### 1-c. 이벤트 메서드(생명 주기 메서드) 선언



\*\*1-c-(1).\*\* 이벤트 메서드에는 \*\*반드시 접근 제한자를 선언\*\*한다.

\* ❌ \*\*지양할 예시\*\*

&#x20; ```csharp

&#x20; void Awake()

&#x20; {

&#x20; }

&#x20; ```

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; protected override void Awake()

&#x20; {

&#x20; }

&#x20; 

&#x20; private void Start()

&#x20; {

&#x20; }

&#x20; ```



\*\*1-c-(2).\*\* MonoBehaviour 혹은 유니티 이벤트 인터페이스를 상속하는 스크립트의 경우, Unity 이벤트 메서드(생명 주기 메서드)를 일반 메서드보다 위쪽에 작성한다. 즉, \*\*일반 메서드 밑에 유니티 이벤트 메서드가 선언되어서는 안된다.\*\*

\* ❌ \*\*지양할 예시\*\*

&#x20; ```csharp

&#x20; private void MethodA() {}

&#x20; private void MethodB() {}

&#x20; 

&#x20; private void Start() {}

&#x20; private void Update() {}

&#x20; ```

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; private void Start() {}

&#x20; private void Update() {}

&#x20; 

&#x20; private void MethodA() {}

&#x20; private void MethodB() {}

&#x20; ```



\*\*1-c-(3).\*\* 이벤트 메서드의 경우, 스크립트 상단에서부터 호출 순서를 고려하여 작성한다.

\- \*\*초기화부 (Awake(), OnEnable(), Start() 등)\*\*

\- \*\*게임 로직부 (Update(), FixedUpdate(), 충돌계열 이벤트 메서드 등)\*\*

\- \*\*해체부 (OnApplicationQuit(), OnDisable(), OnDestroy())\*\*

(예를 들어, `OnDestroy()` 메서드가 `Start()` 메서드의 상단에 위치해서는 안된다.)



\* ❌ \*\*지양할 예시\*\*

&#x20; ```csharp

&#x20; private void Update() {}

&#x20; private void Awake() {}

&#x20; private void OnDestroy() {}

&#x20; private void Start() {}

&#x20; ```

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; private void Awake() {}

&#x20; private void Start() {}

&#x20; private void Update() {}

&#x20; private void LateUpdate() {}

&#x20; private void OnDestroy() {}

&#x20; ```



\### 1-d. null 체크



\*\*1-d-(1).\*\* 스크립트 내에서 객체의 null 체크를 진행할 땐, 아래와 같은 방식으로 진행한다.

\- \*\*순수 C# 객체(POCO)\*\*의 경우, \*\*is 또는 ReferenceEquals()\*\*를 통해 null 체크를 진행한다.

\- \*\*Unity 객체\*\*의 경우, \*\*fake null을 감지할 수 있는 == 및 != 연산자\*\*를 통해 null 체크를 진행한다.


\*\*1-d-(2).\*\* 객체의 프레임 단위의 빈번한 null 체크는 기본적으로 지양한다. 불가피하게 프레임 단위의 null 체크가 필요하다면 반드시 \*\*ReferenceEquals()\*\*를 사용한다.

\* ❌ \*\*지양할 예시\*\*

&#x20; ```csharp

&#x20; \[SerializeField]

&#x20; private GameObject \_unityGO;

&#x20; 

&#x20; private void Update()

&#x20; {

&#x20;     if (\_unityGo == null)

&#x20;     {

&#x20;         return;

&#x20;     }

&#x20; }

&#x20; ```

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; \[SerializeField]

&#x20; private GameObject \_unityGO;

&#x20; 

&#x20; private void Update()

&#x20; {

&#x20;     if (ReferenceEquals(\_unityGO, null))

&#x20;     {

&#x20;         return;

&#x20;     }

&#x20; }

&#x20; ```



\### 1-e. 싱글톤



\*\*1-e-(1).\*\* 싱글톤 클래스는 \*\*객체의 타입\*\*(\*\*POCO 객체\*\* 또는 \*\*MonoBehaviour 상속 Unity 객체\*\*)에 따라 \*\*`POCOSingleton<T>`\*\* 또는 \*\*`MonoBehaviourSingleton<T>`\*\* 제네릭 클래스를 상속하여 사용한다.



\*\*1-e-(2).\*\* \*\*`MonoBehaviourSingleton<T>`를 상속하는\*\* 싱글톤 클래스에서 `Awake()` 메서드를 사용해야 하는 경우, `Awake` 메서드의 오버라이딩 버전을 파생 클래스에 선언한 후, `base.Awake()`를 호출해준다.

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; public class UIManager : MonoBehaviourSingleton<UIManager>

&#x20; {

&#x20;     protected override void Awake()

&#x20;     {

&#x20;         base.Awake();

&#x20;     }

&#x20; }

&#x20; ```



\*\*1-e-(3).\*\* 클래스의 \*\*해체부 이벤트 메서드\*\*(`OnDisable`, `OnDestroy`, `OnApplicationQuit`)에서는 \*\*싱글톤 클래스 인스턴스에 접근하는 것을 지양\*\*한다.

\- 대신, \*\*별도의 메서드\*\*를 선언한 후 \*\*해당 메서드 내에서 접근하는 것을 권장\*\*한다.

\* ❌ \*\*지양할 예시\*\*

&#x20; ```csharp

&#x20; public void OnDestroy()

&#x20; {

&#x20;     CurrencyManager.Instance.Get(0);

&#x20;     GameManager.Instance.PlayData.Score += 100;

&#x20; }

&#x20; ```

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; public void OnEnemyDead()

&#x20; {

&#x20;     CurrencyManager.Instance.Get(0);

&#x20;     GameManager.Instance.PlayData.Score += 100;

&#x20;     Destroy(gameObject);

&#x20; }

&#x20; ```



\### 1-f. Inspector 가독성



\*\*1-f-(1).\*\* 에디터의 Inspector에서 직접 할당해줘야 하는 `\[SerializeField]` 혹은 public 필드의 경우, 할당 위치에 따라 VInspector의 FoldOut 어트리뷰트를 추가해준다. Hierarchy에서 할당해야하는 필드는 \*\*어트리뷰트명을 Hierarchy\*\*로 하고, Project 폴더에서 할당해야 하는 필드는 \*\*어트리뷰트명을 Project\*\*로 한다. 이후 필드의 분류는 Header를 통해 진행한다.



\* ❌ \*\*지양할 예시\*\*

&#x20; ```csharp

&#x20; \[SerializeField]

&#x20; private GameObject \_shootPosition;

&#x20; 

&#x20; \[SerializeField]

&#x20; private GameObject \_muzzleEffect;

&#x20; 

&#x20; \[SerializeField]

&#x20; private GameObject \_bladeEffect;

&#x20; 

&#x20; \[SerializeField]

&#x20; private GameObject \_bombEffect;

&#x20; ```

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; \[Foldout("Hierarchy")]

&#x20; \[SerializeField]

&#x20; private GameObject \_shootPosition;

&#x20; 

&#x20; \[Foldout("Project")]

&#x20; \[Header("VFX")]

&#x20; \[SerializeField]

&#x20; private GameObject \_muzzleEffect;

&#x20; 

&#x20; \[SerializeField]

&#x20; private GameObject \_bladeEffect;

&#x20; 

&#x20; \[SerializeField]

&#x20; private GameObject \_bombEffect;

&#x20; ```



\### 1-g. 대리자



\*\*1-g-(1).\*\* 프로젝트 내에서 사용하는 대리자 필드의 경우, 항상 \*\*public 접근 제한자를 가진 PascalCase로 작성\*\*한다.

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; public Action OnWorkComplete;

&#x20; ```



\*\*1-g-(2).\*\* 대리자 필드명은 항상 \*\*접두사 “On”\*\*을 사용한다.



\*\*1-g-(3).\*\* \*\*대리자 필드는 클래스 최상단에 작성한다. 해당 규칙은 필드, 프로퍼티보다 우선순위가 높다.\*\* (즉, 일반 필드 및 프로퍼티보다 상단에 대리자 필드가 선언되어야 한다.)

\* ❌ \*\*지양할 예시\*\*

&#x20; ```csharp

&#x20; public class NPCController : MonoBehaviour

&#x20; {

&#x20;     \[SerializeField]

&#x20;     private Transform \_targetTransform;

&#x20; 

&#x20;     private bool \_isWorking = false;

&#x20; 

&#x20;     public bool IsWorking { get => \_isWorking; private set => \_isWorking = value; }

&#x20;   

&#x20;     public Action OnWorkComplete;

&#x20; }

&#x20; ```

\* ✅ \*\*권장할 예시\*\*

&#x20; ```csharp

&#x20; public class NPCController : MonoBehaviour

&#x20; {

&#x20;     public Action OnWorkComplete;

&#x20; 

&#x20;     \[SerializeField]

&#x20;     private Transform \_targetTransform;

&#x20; 

&#x20;     private bool \_isWorking = false;

&#x20; 

&#x20;     public bool IsWorking { get => \_isWorking; private set => \_isWorking = value; }

&#x20; }

&#x20; ```



\### 1-h. 스크립트 파일 분리



\*\*1-h-(1).\*\* \*\*interface, enum, class는 모두 별도의 파일(스크립트)로 분리\*\*한다. 즉, 하나의 스크립트 파일에 2개 이상의 클래스나 인터페이스, enum 등이 존재해선 안된다.



\*\*1-h-(2).\*\* 파일(스크립트)명과 해당 클래스/인터페이스/열거형/구조체의 이름은 동일해야 한다.



\---



\## 2. Github



\### 2-a. Commit Message Convention



\*\*2-a-(1). 기본 구조\*\*

\- 커밋 메시지는 태그와 제목, 본문으로 구성한다. 여기서 \*\*태그와 제목은 의무적으로 작성\*\*한다.

&#x20; - `Tag(태그) | Subject(제목)`

&#x20; - `----------------------`

&#x20; - `Body(본문)`



\*\*2-a-(2). 태그\*\*

\- 태그는 해당 커밋 변경사항의 카테고리를 파악하는 매우 중요한 요소이다.

\- \*\*태그의 종류는 다음과 같다.\*\*

&#x20; - \*\*Add |\*\* 에셋이나 패키지, 이미지 등 단순히 파일을 추가하는 작업

&#x20; - \*\*Feat |\*\* 새로운 기능을 구현한 작업

&#x20; - \*\*Fix |\*\* 버그를 수정한 작업

&#x20; - \*\*Style |\*\* 오브젝트나 컴포넌트의 속성값 등을 바꾸는 작업(\*\*코드 수정이 없는 경우\*\*)

&#x20; - \*\*Refactor |\*\* 코드 리팩토링 및 기능 개선 작업(\*\*기능에 변화가 없는 경우\*\*)

&#x20; - \*\*Docs |\*\* 폴더 정리 및, 파일, 폴더명을 수정하거나 옮기는 작업

&#x20; - \*\*Chore |\*\* 프로젝트, 빌드 설정의 변경 및 빌드 관련 작업

&#x20; - \*\*Remove |\*\* 불필요한 파일을 삭제한 작업



\* ❌ \*\*지양할 예시\*\*

&#x20; \* 버그 수정

&#x20; \* implement contents

&#x20; \* test commit

&#x20; \* 전반적인 코드 리팩토링

\* ✅ \*\*권장할 예시\*\*

&#x20; \* Feat | 플레이어 이동 구현

&#x20; \* Add | UI 아이콘 이미지 추가

&#x20; \* Refactor | Ability 캐싱 Dictionary를 이용한 방식으로 변경



\*\*2-a-(3). Subject(제목)\*\*

\- 커밋 메시지의 제목은 50글자 이내로 작성한다.

\- 마침표 및 특수기호는 사용하지 않는다.

\- 현재시제와, 간결한 표현을 사용한다.



\*\*2-a-(4). Body(본문)\*\*

\- 커밋 메시지의 본문은 의무가 아니므로, 자유롭게 작성한다.

\- 작성한다면, 최대한 상세히(코드 변경의 이유가 명확할수록 좋음) 작성한다. PR 작성 시 해당 커밋의 Body가 큰 도움이 된다.



\### 2-b. PR(Pull Request) Convention



\- 작업한 브랜치를 머지하기 위해서는 \*\*Pull Request(풀 리퀘스트, 약칭 PR)을 의무적으로 게시\*\*해야 한다.

\- PR은 Task 단위의 작성을 지향한다.

\- 연관이 없는 여러 Task는 분리하여 PR을 작성한다.

\- PR 리뷰는 PR 게시자를 포함한 최소 2인 이상으로 진행한다.

\- PR 제목은 해당 PR의 커밋 내역을 아우를 수 있는 형식으로 작성하고, 형식은 커밋 메시지의 형식과 동일하다.

\- PR 템플릿의 \*\*PR 작성 전 체크리스트를 반드시 확인\*\*한다. 해당 체크리스트의 조건을 만족하지 않을 경우, PR을 게시해선 안된다.

\- PR 게시 시, Assignees에 PR 게시자(본인)를 추가한다. Label의 경우 PR 제목의 태그와 해당 태그 이외의 작업을 했을 경우 선택적으로 추가한다. PR 게시자의 이름 태그는 의무적으로 추가한다.

\- PR 내용은 기본적으로 레포지토리 내 PR 템플릿을 준수하되, 필요에 따라 카테고리나 항목을 추가적으로 작성한다.

\- \*\*리뷰어는 성심성의껏 리뷰를 진행\*\*한다. \*\*성의있는 리뷰는 프로젝트의 코드 퀄리티를 유지할 수 있을 뿐만 아니라 PR 게시자와 리뷰어의 실력을 동시에 향상시킬 수 있는 지름길\*\*이다.



\### 2-c. Branch Convention



\- 브랜치는 작업 단위로 생성한다.

\- 브랜치명은 \*\*태그-작업명\*\*으로 하고, 띄어쓰기는 생략한다.



\* ❌ \*\*지양할 예시\*\*

&#x20; \* `dev-mingyu`

&#x20; \* `test`

\* ✅ \*\*권장할 예시\*\*

&#x20; \* `Feat-캐릭터컨트롤러`

&#x20; \* `Style-2단계씬내플랫폼배치`

&#x20; \* `Refactor-절차적맵생성알고리즘개선`



\- 원격 저장소의 Main 브랜치에 병합이 완료된 브랜치를 다른 작업을 진행하는 데 계속 사용해선 안된다. PR 게시 후 Main 브랜치에 작업 내용이 병합되었다면, \*\*해당 작업을 진행한 로컬 브랜치를 반드시 삭제\*\*한 후, \*\*새로운 작업은 새로운 브랜치에서 수행\*\*한다.



\---



\## 3. Unity Editor



\### 3-a. 폴더 정리



\*\*3-a-(1).\*\* Unity Editor의 프로젝트 폴더 내 파일들은, 파일 종류에 따라 다음과 같은 네이밍 케이스를 준수한다.

\- \*\*PascalCase\*\* : Scene, 스크립트 (예: `GameScene`, `StartScene`, `GameManager.cs`, `CalculateManager.cs` 등)

\- \*\*Snake\_Case의 변형된 형태\*\* : 프리팹, 이미지(스프라이트), 사운드(오디오클립), 애니메이션 관련 파일 (예: `Sprite\_Adventurer\_1`, `Prefab\_UI\_ResultInfo`, `AudioClip\_Item\_Drop` 등)



\*\*3-a-(2).\*\* 외부에서 Import한 에셋 폴더는 반드시 `20. External Assets`의 하위 폴더로 옮겨둔다.



\*\*3-a-(3).\*\* 스크립트 폴더의 경우, 기능 중심이 아닌 \*\*컨텐츠 중심 폴더 정리를 지향\*\*한다.

\* ❌ \*\*지양할 예시 (기능 중심)\*\*

&#x20; \* \[UI]

&#x20;   \* UI\_PlayerJoystick

&#x20;   \* UI\_Player

&#x20;   \* UI\_Enemy

&#x20; \* \[Controller]

&#x20;   \* PlayerController

&#x20;   \* EnemyController

&#x20;   \* NPCController

\* ✅ \*\*권장할 예시 (컨텐츠 중심)\*\*

&#x20; \* \[Player]

&#x20;   \* PlayerController

&#x20;   \* UI\_Player

&#x20;   \* UI\_PlayerJoystick

&#x20; \* \[Enemy]

&#x20;   \* EnemyController

&#x20;   \* UI\_Enemy

&#x20; \* \[NPC]

&#x20;   \* NPCController



\*\*3-a-(4).\*\* Hierarchy에 존재하는 게임 오브젝트건, Project 폴더 내 Prefab이건, \*\*아무 의미없는 게임 오브젝트명은 절대로 작성하지 않는다.\*\* 이름을 통해 해당 게임 오브젝트가 게임에서 어떤 역할을 하는지 단번에 알 수 있도록 명확히 작성한다.

