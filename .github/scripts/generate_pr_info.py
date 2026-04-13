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
(중략...)
Commits: {commits}
Diff: {diff_text}
Template: {template}

출력 JSON 스키마:
{{
    "title": "PR 제목",
    "labels": ["라벨"],
    "body": "마크다운 전체 텍스트"
}}
"""

models = ['gemini-3-flash-preview', 'gemini-2.5-flash']
success = False

for model_name in models:
    try:
        print(f"Trying model: {model_name}...", file=sys.stderr)
        
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
    print("All models failed. Check your API quota or model availability.", file=sys.stderr)
    sys.exit(1)
