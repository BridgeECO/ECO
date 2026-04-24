import os
import sys
from google import genai
from google.genai import types

client = genai.Client(
    api_key=os.environ["GEMINI_API_KEY"],
    http_options={'api_version': 'v1beta'}
)

with open('commits.txt', 'r', encoding='utf-8', errors='ignore') as f:
    commits = f.read()

with open('diff.txt', 'r', encoding='utf-8', errors='ignore') as f:
    diff_text = f.read()

if os.path.exists('labels.txt'):
    with open('labels.txt', 'r', encoding='utf-8') as f:
        repo_labels = [line.strip() for line in f.readlines() if line.strip()]
else:
    repo_labels = ["Add", "Chore", "Documentation", "Feature", "Fix", "Init", "Refactor", "Remove", "Test"]

commits = commits[:5000]
diff_text = diff_text[:80000]

with open('docs/PULL_REQUEST_TEMPLATE.md', 'r', encoding='utf-8', errors='ignore') as f:
    template = f.read()

prompt = f"""
다음 컨벤션과 PR 템플릿을 바탕으로 PR 정보를 JSON 형태로 생성해.

컨벤션 및 작성 규칙:
- PR 제목: Tag | Subject (예: Feat | 플레이어 이동 구현)
- 📝작업 내용 작성 규칙:
  1. 반드시 마크다운(markdown) 문법을 준수할 것
  2. 반드시 숫자(1., 2., 3. ...)를 사용하여 주요 작업 단위 분류
  3. 작업 단위 작성시 기능 A 구현, XX 시스템 개선과 같이 명사형 종결을 사용
  4. 하이픈(-)을 사용하여 세부 내역 작성
  5. 세부 내역 작성시 ~했습니다. 와 같이 서술형 종결을 사용
  6. 작업 단위 사이에는 1줄 띄워주기(마크다운의 </br>을 사용)
- 예상 리뷰 시간 추정(최소 1분에서 최대 20분)
- 리뷰 요구사항은 템플릿 그대로 유지

사용 가능한 라벨 목록 (반드시 이 목록에 존재하는 이름으로만 정확히 선택해줘):
{", ".join(repo_labels)}

Template:
{template}

Commits:
{commits}

Diff:
{diff_text}

출력 JSON 스키마:
{{
    "title": "PR 제목",
    "labels": ["위 목록에서 커밋 내용에 가장 알맞은 라벨 이름"],
    "body": "마크다운 전체 텍스트"
}}
"""

models = [
    'gemini-3-flash-preview',
    'gemini-2.5-flash'
]
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
        if response and response.text:
            print(response.text)
            success = True
            break
    except Exception as e:
        print(f"Failed {model_name}: {e}", file=sys.stderr)
        continue

if not success:
    sys.exit(1)
