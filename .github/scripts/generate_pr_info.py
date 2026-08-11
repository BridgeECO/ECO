import json
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
  3. 작업 단위의 경우, 마크다운의 볼드체 문법을 사용하여 작성(예시 : **1. 몬스터 기믹 구현**) 
  4. 작업 단위 작성시 기능 A 구현, XX 시스템 개선과 같이 명사형 종결을 사용
  5. 하이픈(-)을 사용하여 세부 내역 작성
  6. 세부 내역 작성시 ~했습니다. 와 같이 서술형 종결을 사용
  7. 작업 단위 사이에는 1줄 띄워주기(마크다운의 </br>을 사용하고, 해당 문법이 제대로 적용될 수 있도록 </br>을 입력한 줄 위아래로도 1줄씩 띄워주기)
- 예상 리뷰 시간 추정(최소 1분에서 최대 15분)
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

# enum을 비워 보내면 API가 400을 돌려주므로, 라벨 목록이 비었을 땐 제약 없이 보낸다.
LABEL_SCHEMA = {"type": "STRING"}
if repo_labels:
    LABEL_SCHEMA["enum"] = repo_labels

PR_SCHEMA = {
    "type": "OBJECT",
    "properties": {
        "title": {"type": "STRING"},
        "labels": {"type": "ARRAY", "items": LABEL_SCHEMA},
        "body": {"type": "STRING"},
    },
    "required": ["title", "labels", "body"],
}

models = [
    'gemini-3-flash-preview',
    'gemini-2.5-flash'
]


def build_config(model_name):
    # thinking 토큰이 출력 예산을 잠식하면 응답이 MAX_TOKENS로 잘려 JSON이 깨진 채 나온다.
    # Gemini 3 계열은 thinking_budget 대신 thinking_level만 받으므로 모델별로 분기한다.
    if model_name.startswith('gemini-3'):
        try:
            thinking = types.ThinkingConfig(thinking_level='low')
        except Exception:
            thinking = None
    else:
        thinking = types.ThinkingConfig(thinking_budget=0)

    return types.GenerateContentConfig(
        response_mime_type="application/json",
        response_schema=PR_SCHEMA,
        max_output_tokens=16384,
        thinking_config=thinking,
    )


def parse_response(model_name, response):
    if response is None:
        print(f"No response from {model_name}", file=sys.stderr)
        return None

    finish = response.candidates[0].finish_reason if response.candidates else None
    text = (response.text or "").strip()

    if not text:
        print(f"Empty response from {model_name} (finish={finish})", file=sys.stderr)
        return None

    try:
        data = json.loads(text)
    except json.JSONDecodeError as e:
        # 응답이 잘리면 여기서 걸러진다. 꼬리를 함께 남겨야 로그만 보고 잘림 여부를 판별할 수 있다.
        print(f"Invalid JSON from {model_name} (finish={finish}): {e}", file=sys.stderr)
        print(f"...{text[-300:]}", file=sys.stderr)
        return None

    if isinstance(data, list):
        data = data[0] if data else None

    if not isinstance(data, dict):
        print(f"Unexpected JSON shape from {model_name}", file=sys.stderr)
        return None

    if not isinstance(data.get("title"), str) or not isinstance(data.get("body"), str):
        print(f"Missing title/body from {model_name} (finish={finish})", file=sys.stderr)
        return None

    if not isinstance(data.get("labels"), list):
        data["labels"] = []

    return data


success = False

for model_name in models:
    try:
        response = client.models.generate_content(
            model=model_name,
            contents=prompt,
            config=build_config(model_name),
        )
    except Exception as e:
        print(f"Failed {model_name}: {e}", file=sys.stderr)
        continue

    data = parse_response(model_name, response)
    if data is not None:
        json.dump(data, sys.stdout, ensure_ascii=False)
        sys.stdout.write("\n")
        success = True
        break

if not success:
    sys.exit(1)
