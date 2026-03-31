\# 유니티 클라이언트 개발 - AGENTS.md



\## 🎯 프로젝트 맥락

\* \*\*역할:\*\* 유니티 클라이언트 개발자

\* \*\*엔진:\*\* Unity

\* \*\*언어:\*\* C#

\* \*\*주요 목표:\*\* 엄격한 성능 기준 유지, 가비지 컬렉션(GC) 오버헤드 방지, 예측 가능한 비동기 제어 흐름 보장.



\## 🚀 비동기 프로그래밍

\* \*\*절대 금지:\*\* `System.Collections.IEnumerator` 및 `UnityEngine.Coroutine`의 사용을 절대 금지한다.

\* \*\*필수 사항:\*\* 모든 비동기 작업에는 반드시 `Cysharp.Threading.Tasks.UniTask`를 사용한다.

\* \*\*제한적 허용:\*\* `System.Threading.Tasks.Task`는 이를 강제하는 외부 .NET 라이브러리와 연동할 때만 예외적으로 사용한다.

\* \*\*UI 예외:\*\* `async void`는 UI 이벤트 핸들러에서만 허용하며, 그 외의 로직에서는 절대 금지한다.



\## 📦 오브젝트 풀링 및 생명주기

\* \*\*절대 금지:\*\* `UnityEngine.Object.Instantiate` 및 `UnityEngine.Object.Destroy`의 직접 호출을 절대 금지한다.

\* \*\*필수 사항:\*\* 모든 동적 오브젝트의 생성과 해제는 `PoolManager`와 `EPoolable` 열거형(enum)을 통해 관리한다.

&#x20; \* 생성: `PoolManager.Instance.Pop(EPoolable.\[Type])`

&#x20; \* 해제: `PoolManager.Instance.Push(EPoolable.\[Type], gameObject)`

\* \*\*주의 사항:\*\* 풀링된 오브젝트의 초기화는 `Awake`나 `Start` 대신 반드시 `OnEnable`에서 수행한다.



\## 🖼️ 컴포넌트 및 UI 참조 바인딩

\* \*\*절대 금지:\*\* 스크립트 내부에서 UI 요소나 외부 객체를 찾기 위해 `GetComponent`, `transform.Find`, `GameObject.Find` 등의 탐색 함수를 사용하는 것을 절대 금지한다.

\* \*\*필수 사항:\*\* 모든 컴포넌트 참조는 반드시 `\[SerializeField] private`으로 선언하고, 유니티 에디터의 인스펙터(Inspector)를 통해 직렬화하여 할당한다.



\## ⚠️ 일반 성능 및 안티패턴

\* \*\*절대 금지:\*\* `Update`, `FixedUpdate`, `LateUpdate` 내부에서 가비지 컬렉션(GC.Alloc)을 발생시키는 새로운 객체 생성을 절대 금지한다.

\* \*\*절대 금지:\*\* `System.Linq` 사용을 절대 금지한다. 컬렉션 순회가 필요한 경우 `for` 또는 `foreach` 루프를 사용한다.

## 📚 Team Conventions & Standards (External)
* **MANDATORY (Coding):** 새로운 C# 스크립트를 생성하거나 필드, 프로퍼티, 메서드를 작성할 때는 **코드 작성 전 반드시** `Convention.md`의 **[1. Coding Convention]**을 읽고 적용한다.
  * *주요 확인 사항:* 네이밍 접두사(Init/Set/Refresh), UI_ 클래스 명명, VInspector Foldout 속성, 대리자(Action) 최상단 선언, 이벤트 메서드 상단 배치.
* **MANDATORY (Git & PR):** 커밋 메시지를 자동 생성하거나 PR 초안을 작성할 때는 **반드시** `Convention.md`의 **[2. Github]** 섹션을 읽고 규정된 태그(Feat, Fix, Refactor 등)와 포맷을 엄격히 따른다.
* **MANDATORY (Unity Editor):** 새로운 폴더를 만들거나 에셋(씬, 프리팹, 스크립트 등)을 생성할 때는 **반드시** `Convention.md`의 **[3. Unity Editor]** 섹션을 읽는다.
  * *주요 확인 사항:* 기능이 아닌 '컨텐츠 중심' 폴더 구조, 에셋 종류별 네이밍 규칙(PascalCase vs Snake_case 변형), 직관적인 Hierarchy 명명.
