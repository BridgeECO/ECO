# CLAUDE.md

Claude Code가 세션을 시작할 때 자동으로 읽는 파일입니다.

이 프로젝트의 개발 규칙 원본은 `AGENTS.md`입니다. 아래 한 줄이 그 내용을 여기로 그대로 가져오므로,
규칙을 고칠 일이 있으면 **`AGENTS.md`만 고치면 됩니다.** 두 벌로 복사해 두면 한쪽만 고쳐져 어긋납니다.

@AGENTS.md

## 저장소 구조

이 저장소의 루트는 유니티 프로젝트 루트가 아닙니다. 경로를 헷갈리면 엉뚱한 위치에 파일을 만들게 됩니다.

* `Unity/ECO/` — 유니티 프로젝트 루트 (`Assets/`, `Packages/`, `ProjectSettings/`, `ECO.sln`)
* `AGENTS.md`, `Convention.md` — 개발 규칙 원본 (저장소 루트)
* `docs/` — GitHub 이슈·PR 템플릿 (`ISSUE_TEMPLATE.yml`, `PULL_REQUEST_TEMPLATE.md`)
* `Excel/` — 기획 데이터 원본

## Convention.md는 자동으로 읽지 않습니다

`Convention.md`는 1,000줄이 넘어 매 세션 전부 끌어오면 낭비입니다.
`AGENTS.md`의 [Team Conventions & Standards] 항목이 정한 시점에 해당 섹션만 직접 열어 읽습니다.

* C# 스크립트·필드·메서드를 작성하기 **전** → [1. Coding Convention]
* 커밋 메시지나 PR 초안을 쓰기 **전** → [2. Github]
* 폴더나 에셋(씬, 프리팹, 스크립트)을 만들기 **전** → [3. Unity Editor]

PR 본문을 작성할 때는 `docs/PULL_REQUEST_TEMPLATE.md`의 형식도 함께 따릅니다.
