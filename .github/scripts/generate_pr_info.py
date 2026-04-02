import os
import google.generativeai as genai

genai.configure(api_key=os.environ["GEMINI_API_KEY"])
model = genai.GenerativeModel('gemini-2.5-pro', generation_config={"response_mime_type": "application/json"})

with open('commits.txt', 'r', encoding='utf-8') as f:
    commits = f.read()

with open('diff.txt', 'r', encoding='utf-8') as f:
    diff_text = f.read()

with open('.github/PULL_REQUEST_TEMPLATE.md', 'r', encoding='utf-8') as f:
    template = f.read()

prompt = f"""
다음 컨벤션과 PR 템플릿을 바탕으로 PR 정보를 JSON 형태로 생성해.

컨벤션:
- PR 제목: Tag | Subject (예: Feat | 플레이어 이동 구현)
- 사용 가능한 태그: Add, Feat, Fix, Style, Refactor, Docs, Chore, Remove
- PR 내용은 제공된 커밋 메시지와 Diff를 요약해서 작성.
- 예상 리뷰 시간은 Diff의 크기와 복잡도를 보고 분 단위로 추정.
- 리뷰 요구사항은 Diff를 분석하여 복잡하거나 특별히 검토가 필요한 로직에 대해 작성.

Template:
{template}

Commits:
{commits}

Diff:
{diff_text}

출력 JSON 스키마:
{{
    "title": "PR 제목 (컨벤션 준수)",
    "labels": ["커밋에서 도출된 영문 태그 (예: enhancement, bug, documentation 등 Github 기본 라벨 형식으로 매핑)"],
    "body": "템플릿 양식에 맞춰 작성된 마크다운 전체 텍스트"
}}
"""

response = model.generate_content(prompt)
print(response.text)
