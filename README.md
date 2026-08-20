<div align="center">

<h2>ECO</h2>

몰락한 소리 문명의 도시 ‘율’을 다시 깨우는, 몽환적인 2D 액션 플랫포머 게임 ECO입니다!<br>
플레이어는 이름조차 없는 실험체 ‘이코(EC-0)’가 되어, 잠들어버린 고대 도시의 구조물에 에너지를 흘려보내며 나아갑니다.

#### ↓↓↓↓↓ ECO의 타이틀 화면입니다. ↓↓↓↓↓
![타이틀 화면](https://github.com/user-attachments/assets/218fe211-4547-44b8-872f-ffaad9f4d402)<br>

</div><br>

## 목차
  - [개요](#개요)
  - [게임 설명](#게임-설명)
  - [사용 기술](#사용-기술)
  - [게임 플레이](#게임-플레이)
<br>

## 개요
| **프로젝트 명** | ECO |
|:---:|:---:|
| **프로젝트 기간** | (작성 예정) |
| **팀원** | (작성 예정) |
| **기술 스택** | <img src="https://img.shields.io/badge/Unity-6000.0.71f1-000000?style=for-the-badge&logo=unity" height="25"> <img src="https://img.shields.io/badge/C%23-512BD4?style=for-the-badge&logo=csharp&logoColor=white" height="25"> <img src="https://img.shields.io/badge/Universal RP-17.0.4-000000?style=for-the-badge&logo=unity" height="25"> <br> <img src="https://img.shields.io/badge/UniTask-2C8EBB?style=for-the-badge" height="25"> <img src="https://img.shields.io/badge/R3-7B3FBF?style=for-the-badge" height="25"> <img src="https://img.shields.io/badge/Cinemachine-3.1.5-000000?style=for-the-badge&logo=unity" height="25"> <br> <img src="https://img.shields.io/badge/Github-181717?style=for-the-badge&logo=github" height="25"> <img src="https://img.shields.io/badge/Github Actions-363636?style=for-the-badge&logo=githubactions" height="25"> <img src="https://img.shields.io/badge/CodeRabbit-FF570A?style=for-the-badge&logo=coderabbit&logoColor=white" height="25"> <img src="https://img.shields.io/badge/Gemini Code Assist-000000?style=for-the-badge&logo=googlegemini" height="25"> |
| **플랫폼 및 장르** | <img src="https://img.shields.io/badge/Platform-Windows-lightgrey?style=for-the-badge" height="25"> <img src="https://img.shields.io/badge/Genre-2D Platformer %2F Metroidvania-2E8B57?style=for-the-badge" height="25"> |
<br>

## 게임 설명
|![타이틀 화면](https://github.com/user-attachments/assets/95f86386-47ad-415c-abac-95a30b5c155b)|![인게임 화면](https://github.com/user-attachments/assets/7cf6b57f-7492-466f-bc58-652b0c67a998)|
|:---:|:---:|
|타이틀 화면|인게임 화면|


#### **잠든 도시에 다시 소리를 불어넣으세요!** <br>

ECO는 플레이어가 고대 도시의 발판과 장치에 에너지를 공급해 길을 만들고, 에너지가 끊기기 전에 방을 돌파해야 하는 1인용 2D 액션 플랫포머 게임입니다. <br><br>

#### 1. 에너지가 끊기기 전에 돌파하는 스피드런 
- 방 안의 플랫폼들은 기본적으로 **비활성 상태**이며, 공급선에 연결된 채 잠들어 있습니다.
- 플레이어가 스위치를 작동시키면 에너지구가 공급선을 따라 흐르고, 에너지가 도달한 플랫폼부터 차례로 깨어납니다.
- 하지만 스위치에서 손을 떼면 **3초 뒤 같은 순서로 에너지가 차단**되며 플랫폼이 하나씩 사라집니다. 길이 완전히 닫히기 전에 방을 빠져나가야 합니다. <br>

#### 2. 물리치는 것이 아니라 도망치는 보스전 
- 지역의 방들을 모두 통과하면 보스전에 진입하지만, 보스를 **쓰러뜨리는 것이 아니라 시간 내에 도망쳐야** 합니다.
- 도망치는 코스는 지금까지 플레이어가 지나온 방들을 이어 붙인 구간입니다. 즉, 각 방을 진행하는 것 자체가 마지막 보스전의 **구간별 예행연습**이 됩니다.
- 일반 구간이 '플레이어가 길을 여는' 감각이라면, 보스전은 플랫폼이 보스에 의해 강제로 작동하며 '세계가 플레이어를 몰아붙이는' 감각으로 설계되었습니다. <br>

#### 3. 정교하고 속도감 있는 플랫포머 액션 
- 코요테 타임, 점프 선입력, 머리 모서리 보정 등 **조작 불쾌감을 없애기 위한 보정 로직**이 촘촘히 들어가 있습니다.
- 마우스 방향으로 도약하는 대쉬는 발동 전 **체류(Hover)** 상태를 거치므로, 공중에서 잠시 멈춰 타이밍을 재고 각도를 고를 수 있습니다.
- 벽에 붙어 미끄러지는 벽타기와, 반대 방향 45도로 튀어 오르는 벽점프를 조합해 수직 구간을 오릅니다. <br>

#### 4. 지역과 방으로 이루어진 무대 
- 무대는 하나의 테마를 공유하는 **지역(Region)**과, 그 지역을 나눈 **방(Room)** 단위로 구성됩니다.
- 세이브 포인트를 지날 때마다 자동 저장되며 목숨이 3개로 회복되고, 사망 시 그 방의 진행 상황이 통째로 초기화됩니다.
- 지형은 이미지 교체, 충돌체 온/오프, 왕복 이동, 숨겨진 공간 노출 등 여러 기믹을 동시에 가질 수 있습니다. <br>

<br>

## 사용 기술
### 1. Energy Line Simulation
- 이 게임의 핵심 차별점인 **에너지 공급선**을 스플라인 경로 위의 구간(Segment) 단위 시뮬레이션으로 구현했습니다.
- `EnergyPathCalculator`가 노드 사이를 **Hermite 스플라인**으로 보간해 부드러운 경로를 만들고, `EnergySegmentController`가 그 위를 진행/후퇴하는 에너지 구간의 머리 위치를 관리합니다. 접선은 인접 노드 방향의 평균으로 계산하고 tension 값으로 곡률을 조절합니다.
- 지형은 에너지구가 **중심을 통과한 시점**에 활성화되고, 에너지가 **완전히 빠져나간 시점**에 비활성화되어 시각적 인과가 어긋나지 않도록 했습니다.
- 플레이어 시야 밖에서 장치가 작동해 조작 결과를 알 수 없는 문제를 막기 위해, 최초 작동 시 카메라가 에너지구를 따라가는 `EnergyLineTracker`를 두었습니다.
<br>

### 2. FSM-based Player Controller
- 플레이어 상태를 `Grounded`, `Airborne`, `Dash`, `Hover`, `WallSlide`로 분리한 **유한 상태 기계**로 구성해, 상태별 입력 해석과 전이 조건을 한 곳에서 관리합니다.
- 플랫포머 조작감을 위한 보정 로직을 각각 별도 클래스로 분리했습니다. `PlayerCornerCorrector`는 머리를 3분할해 모서리 충돌 시 캐릭터를 옆으로 밀어내 점프를 유지시키고, `PlayerSlip`은 지형 끝자락에서 미끄러지도록 처리합니다.
- 몸통·발·상호작용·미끄러짐 4종 충돌체를 분리해, 발 충돌체만 One Way Platform과 아래 방향으로 판정하도록 했습니다.
<br>

### 3. Data-driven Terrain Gimmick
- 지형 기믹을 `TerrainGimmickBase`(POCO 로직)와 `TerrainGimmickBaseSO`(ScriptableObject 데이터)로 나눠, **기획자가 코드 수정 없이 인스펙터에서 기믹을 조합**할 수 있게 했습니다.
- 하나의 지형이 이미지 토글·이미지 교체·충돌체 토글·단일 이동·왕복 이동·컨베이어·페이크 지형·즉사 판정 등 여러 기믹을 동시에 가질 수 있으며, 각 기믹은 '상시' 또는 '에너지 연동'으로 설정됩니다.
- 기믹이 MonoBehaviour가 아니라 POCO이므로 소유 오브젝트의 파괴를 스스로 감지할 수 없습니다. 이를 위해 소유자의 `CancellationToken`을 주입해 씬 전환 시 비동기 작업이 누수되지 않도록 했습니다.
<br>

### 4. Designer-friendly Editor Tools
- 레벨 디자인 반복 비용을 줄이기 위해 에디터 도구를 직접 제작했습니다.
- Tag / Layer / Scene / SFX 열거형을 프로젝트 설정으로부터 **자동 생성**해, 문자열 하드코딩과 오타로 인한 런타임 오류를 제거했습니다.
- 기획 단위 기준인 '블록'(FHD 기준 64×64px) 그리드를 씬 뷰에 그리는 `GridGuide`, 기믹 이동 경로와 보스 추격 경로를 기즈모로 시각화하는 도구를 두어 배치 결과를 즉시 확인할 수 있게 했습니다.
- `SubclassSelector` 어트리뷰트와 커스텀 프로퍼티 드로어로, 인스펙터에서 직렬화된 파생 타입을 드롭다운으로 선택하도록 했습니다.
<br>

### 5. UniTask & Event-driven Architecture
- 코루틴을 전면 배제하고 **UniTask**로 비동기 흐름을 통일했으며, 모든 비동기 작업에 `CancellationToken`을 전달해 파괴된 객체가 남긴 작업이 계속 도는 것을 방지했습니다.
- 매니저 간 직접 참조 대신 `EventManager`의 이벤트 채널로 통지해 단방향 의존성을 유지했습니다. 페이로드가 있는 채널은 타입 불일치를 실행 시점에 검증합니다.
- 상호작용 입력처럼 스트림 성격이 강한 일부 로직에는 **R3**를 선택적으로 사용하고, 단순한 지연 처리는 가독성을 위해 UniTask로 유지했습니다.
<br>

### 6. Screen-relative Spatial Audio
- 효과음의 공간감 기준점을 플레이어가 아닌 **화면 중심(카메라)**으로 잡아, 카메라가 비추는 범위 안에서는 위치와 무관하게 동일한 볼륨이 유지되도록 했습니다.
- `ViewportAttenuator`가 화면 밖으로 벗어난 오브젝트의 볼륨만 빠르게 감쇠시키며, 오브젝트별로 감쇠 예외를 지정할 수 있습니다.
- 플레이어 효과음은 예외적으로 항상 정중앙에서 재생되고, 밟은 지형의 태그에 따라 발소리가 달라집니다.
<br>

### 7. Build & Code Review Automation
- **GitHub Actions**로 매일 정해진 시각에 Windows 빌드를 수행하고, Library 캐시를 활용해 빌드 시간을 단축했습니다.
- 브랜치 푸시 시 PR 초안을 자동 생성해, 커밋 이력으로부터 변경 요약을 채우도록 했습니다.
- **CodeRabbit**과 **Gemini Code Assist**를 연동해 PR마다 한국어 코드 리뷰를 자동으로 받고, 컨벤션 위반과 리팩터링 지점을 지속적으로 관리했습니다.
<br>


## 게임 플레이
### 조작법
| 구분 | 동작 | 입력 키 (Input) |
| :---: | :---: | :---: |
| **이동** | 좌우 이동 | <kbd>A</kbd> <kbd>D</kbd> |
| **상하 입력** | 아래 방향 입력 (밑점프 등) | <kbd>W</kbd> <kbd>S</kbd> |
| **점프** | 일반 점프 / 벽 점프 | <kbd>Space</kbd> |
| **대쉬** | 체류 후 마우스 방향으로 도약 | <kbd>마우스 좌클릭</kbd> |
| **상호작용** | 스위치 / NPC / 오브젝트 | <kbd>F</kbd> |
| **시스템** | 일시정지 / 뒤로가기 | <kbd>ESC</kbd> |

> 벽타기는 별도 키 없이, 공중에서 **벽 방향으로 이동키를 누른 채 벽에 붙으면** 진입합니다.
<br>

### 주요 화면
#### 1. 타이틀 및 시스템 화면
|타이틀 메뉴|저장 데이터 선택|환경설정|일시정지|
|:---:|:---:|:---:|:---:|
|![타이틀 메뉴](https://github.com/user-attachments/assets/95f86386-47ad-415c-abac-95a30b5c155b)|![저장 데이터 선택](https://github.com/user-attachments/assets/6862d552-d36d-4849-ba2c-73177e239526)|![환경설정](https://github.com/user-attachments/assets/4819a798-5ca2-4c87-b0c3-17aaf72eca87)|![일시정지](https://github.com/user-attachments/assets/91f91b95-845f-43cc-9546-50407ff4df7e)|
|새 게임과 이어하기, 환경설정으로 진입합니다.|3개의 슬롯에 마지막으로 저장된 지역이 표시됩니다.|그래픽·사운드·조작·게임플레이 탭으로 나뉩니다.|게임 진행 중 설정과 타이틀 복귀를 지원합니다.|
<br>

#### 2. 탐험 단계
|인게임 화면|에너지 공급선|
|:---:|:---:|
|![인게임 화면](https://github.com/user-attachments/assets/7cf6b57f-7492-466f-bc58-652b0c67a998)|![에너지 공급선](https://github.com/user-attachments/assets/dfa921a3-2241-4a64-a537-587da57f4642)|
|우측 상단에 남은 목숨이 표시되며, 지형을 이용해 출구로 향합니다.|스위치를 작동시키면 에너지가 공급선을 따라 흐르며 플랫폼을 깨웁니다.|
<br>

#### 3. 보스전 단계
![보스 추격전](https://github.com/user-attachments/assets/4d7c1113-633c-4515-84ea-62a7ab606928)
<br>

지역의 마지막에는 보스가 기다립니다. 보스를 공격할 수단은 없으며, 지금까지 익힌 기믹을 총동원해 추격을 따돌리고 탈출해야 합니다.
