import os
import sys
from google import genai
from google.genai import types

client = genai.Client(api_key=os.environ["GEMINI_API_KEY"])

with open('commits.txt', 'r', encoding='utf-8', errors='ignore') as f:
    commits = f.read()

with open('diff.txt', 'r', encoding='utf-8', errors='ignore') as f:
    diff_text = f.read()

commits = commits[:5000]
diff_text = diff_text[:80000]

with open('docs/PULL_REQUEST_TEMPLATE.md', 'r', encoding='utf-8', errors='ignore') as f:
    template = f.read()

prompt = f"""
다음 컨벤션과 PR 템플릿을 바탕으로 PR 정보를 JSON 형태로 생성해.

컨벤션 및 작성 규칙:
- PR 제목: Tag | Subject (예: Feature | 플레이어 이동 구현)
- 📝작업 내용 작성 규칙 (매우 중요):
  1. 반드시 숫자(1., 2., 3. ...)를 사용하여 주요 작업 단위나 컴포넌트를 큰 목차로 분류해.
  2. 각 숫자 목차 아래에는 하이픈(-)을 사용하여 들여쓰기 된 세부 작업 내역을 글머리 기호로 작성해.
  예시:
  1. SceneGridOverlay 에디터 스크립트를 구현했습니다.
     - 프로젝트 상단의 ECO/Grid/Toggle Scene Grid를 누름으로써 Scene View 상의 Grid를 키고 끌 수 있습니다.
- 예상 리뷰 시간은 Diff의 크기와 복잡도를 보고 분 단위로 추정해.
- 리뷰 요구사항은 특별히 작성하지는 말고, 템플릿 그대로 냅둬. 브랜치 작업자에게 맡기도록 해.

사용 가능한 라벨 목록 (이 중에서만 선택해):
"Add", "Chore", "Documentation", "Feature", "Fix", "Init", "Refactor", "Remove", "Test"

Template:
{template}

Commits:
{commits}

Diff:
{diff_text}

출력 JSON 스키마:
{{
    "title": "PR 제목 (컨벤션 준수)",
    "labels": ["사용 가능한 라벨 목록에서 커밋 내용에 가장 알맞은 라벨 이름"],
    "body": "템플릿 양식과 작성 규칙에 맞춰 작성된 마크다운 전체 텍스트"
}}
"""

models = ['gemini-3.0-flash', 'gemini-2.5-flash', 'gemini-2.0-flash']
success = False

for model_name in models:
    try:
        response = client.models.generate_content(
            model=model_name,
            contents=prompt,
            config=types.GenerateContentConfig(
                response_mime_type="application/json",
            )
        )
        print(response.text)
        success = True
        break
    except Exception:
        continue

if not success:
    sys.exit(1)
