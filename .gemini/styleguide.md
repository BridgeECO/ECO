# ECO 프로젝트 프로그래밍 컨벤션(2026)

본 문서는 프로젝트 ECO의 프로그래밍 컨벤션을 정의합니다. 경직되지 않은 유연한 컨벤션을 지향하며, 토의를 통해 언제든 변경될 수 있습니다.

## 1. Coding Convention

### 1-a. Naming Convention 및 Indentation
* 본 프로젝트는 C# at Google Style Guide를 준수합니다. 별도로 명시되지 않은 세부 사항은 해당 가이드를 따릅니다.
* **메서드 작명 규칙**:
  * 초기화: `Init` + [초기화 대상] (예: `InitPlayerData()`)
  * 변경(데이터 변화/대입): `Set` + [변경 대상] (예: `SetQuest()`)
  * UI 데이터 반영: `Refresh` + [대상] (대상은 필요에 따라 생략 가능, 예: `RefreshQuest()`)
* `if`문 내부 로직이 1줄이더라도 항상 중괄호 `{}`를 사용합니다.
* 리스트, 해시 테이블 등 컨테이너 필드명은 끝에 복수형 접미어 `s`를 붙이며, 컨테이너의 이름(List, Dictionary 등)을 필드명에 포함하지 않습니다.
* 인터페이스명에는 접두사 `I`를 명시합니다.
* 열거형(enum)명에는 접두사 `E`를 명시합니다.
* `bool` 변수의 접두사로는 `Is`, `Has`, `Can`만 사용하며, `Need`, `Should` 등은 지양합니다.
* `const` 변수는 대문자 스네이크 케이스(UPPER_SNAKE_CASE)로 작성합니다.
* UI 로직 클래스는 접두사 `UI_`를 명시하고, 인게임 로직과 명확히 책임을 분리합니다.

### 1-b. 필드 및 프로퍼티
* 클래스와 구조체 내 모든 필드는 `private`으로 선언합니다.
  * 예외: Serializable, Scriptable Object, DTO 클래스는 `public` 필드 선언이 가능합니다.
* 대리자(delegate, Action, Func 등) 필드는 항상 `public`으로 선언합니다.
* `[SerializeField]` 속성은 선언부와 같은 줄에 쓰지 않고 줄바꿈을 하여 작성합니다.
* `[SerializeField]` 필드는 코드로 기본값을 할당하지 않고 Inspector에서 할당합니다.
* 프로퍼티는 외부 접근이 필요한 경우에만 `protected` 또는 `public`으로 선언하며, 선언이 필수는 아닙니다.
* 프로퍼티 선언 방식(백업 필드 또는 자동 구현)이나 접근자 선언 방식에는 네이밍 컨벤션 외의 별도 제한이 없습니다.
* 필드는 클래스 최상단에, 프로퍼티는 모든 필드가 선언된 하단에 배치하며, 백업 필드와 프로퍼티를 섞어서 선언하지 않습니다.

### 1-c. 이벤트 메서드 (생명 주기 메서드) 선언
* 이벤트 메서드에는 반드시 접근 제한자를 선언합니다.
* Unity 이벤트 메서드는 일반 메서드보다 상단에 위치해야 합니다.
* 이벤트 메서드는 호출 순서를 고려하여 작성합니다:
  1. 초기화부: `Awake()`, `OnEnable()`, `Start()`
  2. 게임 로직부: `Update()`, `FixedUpdate()`, 충돌 계열
  3. 해체부: `OnApplicationQuit()`, `OnDisable()`, `OnDestroy()`

### 1-d. null 체크
* 순수 C# 객체(POCO)는 `is` 또는 `ReferenceEquals()`로 null 체크를 진행합니다.
* Unity 객체는 null 체크 주기 및 fake null을 고려해 상황에 맞는 문법을 사용합니다.
* 프레임 단위의 빈번한 null 체크는 지양하되, 불가피할 경우 반드시 `ReferenceEquals()`를 사용합니다.

### 1-e. 싱글톤
* 객체 타입에 따라 `POCOSingleton<T>` 또는 `MonoBehaviourSingleton<T>`를 상속하여 구현합니다.
* `MonoBehaviourSingleton<T>` 상속 시 `Awake()`를 오버라이딩해야 한다면 `base.Awake()`를 반드시 호출합니다.
* 해체부 이벤트 메서드(`OnDisable`, `OnDestroy`, `OnApplicationQuit`)에서는 싱글톤 인스턴스에 직접 접근하는 것을 지양하고, 별도의 메서드를 선언하여 접근합니다.

### 1-f. Inspector 가독성
* Inspector에서 할당하는 필드는 할당 위치에 따라 VInspector의 `[Foldout]` 어트리뷰트를 사용합니다.
  * Hierarchy 할당: `[Foldout("Hierarchy")]`
  * Project 폴더 할당: `[Foldout("Project")]`
* 이후 필드의 분류는 `[Header]`를 통해 진행합니다.

### 1-g. 대리자 (Delegate)
* 프로젝트 내 대리자 필드는 항상 `public` 접근 제한자를 가진 PascalCase로 작성합니다.
* 대리자 필드명은 접두사 `On`을 사용합니다.
* 일반 필드 및 프로퍼티보다 우선하여 클래스 최상단에 작성합니다.

### 1-h. 스크립트 파일 분리
* `interface`, `enum`, `class`는 각각 별도의 파일로 분리합니다 (1파일 1클래스/인터페이스/열거형).
* 파일명과 내부 타입(클래스/인터페이스 등)의 이름은 동일해야 합니다.

---

## 2. Github

### 2-a. Commit Message Convention
* 커밋 메시지는 `Tag | Subject` 형식의 제목과 `Body`(본문)로 구성됩니다.
* **태그 종류**:
  * `Add`: 에셋, 패키지, 이미지 등 파일 추가
  * `Feat`: 새로운 기능 구현
  * `Fix`: 버그 수정
  * `Style`: 오브젝트/컴포넌트 속성값 변경 (코드 수정 없음)
  * `Refactor`: 코드 리팩토링 및 기능 개선 (기능 변화 없음)
  * `Docs`: 폴더 정리, 파일/폴더명 수정 및 이동
  * `Chore`: 프로젝트/빌드 설정 변경
  * `Remove`: 불필요한 파일 삭제
* **제목(Subject)**: 50자 이내, 마침표/특수기호 금지, 현재시제 및 간결한 표현 사용.
* **본문(Body)**: 의무는 아니나, 코드 변경 이유를 최대한 상세히 작성하는 것을 권장합니다.

### 2-b. PR(Pull Request) Convention
* 머지를 위해 PR 게시는 의무이며, Task 단위 작성을 지향합니다.
* 연관 없는 여러 Task는 분리하여 PR을 작성합니다.
* 리뷰는 게시자 포함 최소 2인 이상이 진행합니다.
* 제목 형식은 커밋 메시지와 동일하게 작성합니다.
* **PR 작성 전 체크리스트**:
  1. 브랜치 최신화 (behind 커밋 없음)
  2. 기능 및 전체 로직 정상 동작 확인 (기능 테스트 완료)
  3. 작업 Scene 변경사항을 메인 Scene에 반영
* PR 게시 시 `Assignees`에 게시자(본인)를 의무적으로 추가하며, `Labels`는 태그나 추가 작업에 맞춰 선택적으로 적용합니다.

### 2-c. Branch Convention
* 브랜치는 작업 단위로 생성합니다.
* 브랜치명 규칙: `태그-작업명` (띄어쓰기 생략, 예: `Feat-캐릭터컨트롤러`).
* Main 브랜치에 병합된 로컬 브랜치는 반드시 삭제하고, 새로운 작업은 새 브랜치에서 수행해야 합니다.

---

## 3. Unity Editor

### 3-a. 폴더 정리
* **파일 네이밍 규칙**:
  * PascalCase: Scene, 스크립트 (예: `GameScene`, `GameManager.cs`)
  * Snake_Case 변형: 프리팹, 이미지, 사운드, 애니메이션 (예: `Sprite_Adventurer_1`, `Prefab_UI_ResultInfo`)
* 외부 Import 에셋은 반드시 `20. External Assets` 하위 폴더로 이동시킵니다.
* 기능 중심이 아닌 **컨텐츠 중심**의 폴더 정리를 지향합니다 (예: `[Player]` 폴더 내에 `PlayerController`, `UI_Player` 등을 함께 배치).
* Hierarchy나 Project 창에서 아무 의미 없는 게임 오브젝트명을 작성하지 않으며, 단번에 역할을 알 수 있도록 명명합니다.
