# 유니티 클라이언트 개발 - AGENTS.md

## 🎯 프로젝트 맥락

* **역할:** 유니티 클라이언트 개발자

* **엔진:** Unity

* **언어:** C#

* **주요 목표:** 엄격한 성능 기준 유지, 가비지 컬렉션(GC) 오버헤드 방지, 예측 가능한 비동기 제어 흐름 보장.

* **규칙 준수 경고 (Strict Compliance Warning):** 이 문서(`AGENTS.md`)와 `Convention.md`에 기재된 모든 규칙은 타협 불가능한 **절대 원칙**이다. 에이전트는 코드를 작성하거나 수정할 때 항상 이 규칙들을 대조하고 엄격하게 준수해야 하며, 이를 위반하여 규칙에 어긋나는 코드를 단 한 줄이라도 생산해서는 안 된다.


## 주석
* **절대 금지:** 단순히 코드를 한 줄씩 기계적으로 설명하는 주석 작성을 절대 금지한다.
* **필수 사항:** 복잡한 알고리즘, 수학 공식, 또는 특정 비즈니스 맥락 및 기술적 결정 이유(최적화 근거 등)를 명시하기 위한 필수적인 주석은 적극적으로 작성하여 유지보수성을 높인다.


## 🚀 비동기 프로그래밍

* **절대 금지:** `System.Collections.IEnumerator` 및 `UnityEngine.Coroutine`의 사용을 절대 금지한다.

* **필수 사항:** 모든 비동기 작업에는 반드시 `Cysharp.Threading.Tasks.UniTask`를 사용한다.

* **필수 사항:** `UniTask` 사용 시 비동기 작업 도중 객체가 파괴되어 발생할 수 있는 메모리 누수 및 오작동(Dangling Task)을 방지하기 위해, 반드시 `CancellationToken`을 인자로 전달하여 작업을 취소할 수 있도록 구현한다. (예: `gameObject.GetCancellationTokenOnDestroy()`)

* **제한적 허용:** `System.Threading.Tasks.Task`는 이를 강제하는 외부 .NET 라이브러리와 연동할 때만 예외적으로 사용한다.

* **UI 예외:** `async void`는 UI 이벤트 핸들러에서만 허용하며, 그 외의 로직에서는 절대 금지한다.



## 📦 오브젝트 풀링 및 생명주기

* **절대 금지:** `UnityEngine.Object.Instantiate` 및 `UnityEngine.Object.Destroy`의 직접 호출을 절대 금지한다.

* **필수 사항:** 모든 동적 오브젝트의 생성과 해제는 `PoolManager`와 `EPoolable` 열거형(enum)을 통해 관리한다.

  * 생성: `PoolManager.Instance.Pop(EPoolable.[Type])`

  * 해제: `PoolManager.Instance.Push(EPoolable.[Type], gameObject)`

* **주의 사항:** 풀링된 오브젝트의 초기화는 `Awake`나 `Start` 대신 반드시 `OnEnable`에서 수행한다.

* **필수 사항:** 풀링된 오브젝트가 반환(`Push`)되거나 활성화(`OnEnable`)될 때, 이전 상태의 오염을 방지하기 위해 클래스 내부의 모든 가변 상태(리스트, 딕셔너리, 변수 값 등)를 반드시 초기화(Reset/Clear)해야 한다.



## 🖼️ 컴포넌트 및 UI 참조 바인딩

* **절대 금지:** 스크립트 내부에서 UI 요소나 외부 객체를 찾기 위해 `transform.Find`, `GameObject.Find` 등의 탐색 함수를 사용하는 것을 절대 금지한다.

* **필수 사항:** 모든 컴포넌트 참조는 반드시 `[SerializeField] private`으로 선언하고, 유니티 에디터의 인스펙터(Inspector)를 통해 직렬화하여 할당한다.

* **예외 사항:** 동적으로 생성되는 오브젝트나 런타임에 주입되는 참조의 경우, `GetComponent`를 사용하거나 초기화(Init/Set) 메서드를 통해 외부에서 직접 참조를 주입받아 바인딩하도록 하며, 어떠한 경우에도 `Find` 계열 탐색 함수는 사용하지 않는다.



## ⚠️ 일반 성능 및 안티패턴

* **절대 금지:** `Update`, `FixedUpdate`, `LateUpdate` 내부에서 가비지 컬렉션(GC.Alloc)을 발생시키는 새로운 객체 생성을 절대 금지한다.

* **절대 금지:** 매 프레임 호출되는 루프(Update 등) 및 잦은 이벤트 핸들러 내부에서의 `System.Linq` 사용을 절대 금지하며, 컬렉션 순회 시 `for` 또는 `foreach` 루프를 사용한다. 단, 성능 영향이 미미한 일회성 초기화 단계(초기 로딩 등)나 에디터 스크립트 도구에서는 생산성을 위해 예외적으로 사용을 허용한다.

## 📐 설계 원칙 및 클래스 구조 (SRP)

* **필수 사항 (SRP 준수):** 단일 책임 원칙(Single Responsibility Principle)을 철저히 준수한다. 하나의 클래스는 오직 하나의 명확한 책임만을 가져야 하며, 클래스 내부의 로직이 여러 성격의 기능을 포함하고 있다면 이를 별도의 스크립트/클래스로 반드시 분리해야 한다.

* **필수 사항 (메서드 분리):** 하나의 메서드가 너무 많은 역할을 수행하지 않도록 작은 단위의 서브 메서드로 최대한 분리한다. 메서드는 하나의 명확한 작업 단위만 처리해야 한다.

* **임계 한계 (클래스 길이 제한):** 클래스(또는 구조체)의 전체 코드 길이가 **200줄**을 초과할 경우, 해당 클래스는 단일 책임 원칙(SRP)을 위반한 것으로 간주한다. 이 임계치를 초과하기 전 혹은 초과한 시점에 즉시 리팩토링을 수행하여 로직을 여러 개의 별도 클래스로 분리해야 한다.

## 🔄 반응형 프로그래밍 (R3)

* **권장 사항 (선택적 사용):** 반응형 프로그래밍 라이브러리인 **R3**는 강제사항이 아니며, 데이터 바인딩, 복잡한 인풋 스트림 처리, 프레임 단위 모니터링 등 반응형 패턴이 적합한 시나리오에 한해 **선택적**으로 사용한다.

* **필수 사항 (구독 생명주기 관리):** `Subscribe` 호출로 생성된 모든 `IDisposable`은 클래스의 비활성화(`OnDisable`) 또는 파괴(`OnDestroy`) 시점에 반드시 해제(`Dispose`)하여 메모리 누수를 방지한다. R3의 `CompositeDisposable`, `.AddTo(this)`, 또는 `DisposableBag`을 적극 활용한다.

* **제한적 사용 (가독성 우선):** 단순히 시간 지연(Delay) 후 액션을 처리하는 등의 선형적인 비동기 처리는 Observable 스트림 대신 `UniTask`를 사용하여 가독성을 높이고 호출 스택을 단순하게 유지한다.

## 📚 Team Conventions & Standards (External)
* **MANDATORY (Coding):** 새로운 C# 스크립트를 생성하거나 필드, 프로퍼티, 메서드를 작성할 때는 **코드 작성 전 반드시** `Convention.md`의 **[1. Coding Convention]**을 읽고 적용한다.
  * *주요 확인 사항:* 네이밍 접두사(Init/Set/Refresh), UI_ 클래스 명명, VInspector Foldout 속성, 대리자(Action) 최상단 선언, 이벤트 메서드 상단 배치.
* **MANDATORY (Git & PR):** 커밋 메시지를 자동 생성하거나 PR 초안을 작성할 때는 **반드시** `Convention.md`의 **[2. Github]** 섹션을 읽고 규정된 태그(Feat, Fix, Refactor 등)와 포맷을 엄격히 따른다.
* **MANDATORY (Unity Editor):** 새로운 폴더를 만들거나 에셋(씬, 프리팹, 스크립트 등)을 생성할 때는 **반드시** `Convention.md`의 **[3. Unity Editor]** 섹션을 읽는다.
  * *주요 확인 사항:* 기능이 아닌 '컨텐츠 중심' 폴더 구조, 에셋 종류별 네이밍 규칙(PascalCase vs Snake_case 변형), 직관적인 Hierarchy 명명.
