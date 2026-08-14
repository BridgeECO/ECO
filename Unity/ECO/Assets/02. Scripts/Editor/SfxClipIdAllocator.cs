using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// ESfxClip 항목의 이름→정수값 매핑을 유지한다.
///
/// enum 값은 프리팹·씬에 정수로 직렬화되므로, 항목이 알파벳순으로 재정렬되면
/// 저장된 참조가 조용히 다른 클립을 가리킨다. 한 번 부여한 값은 절대 바꾸지 않고,
/// 클립이 삭제되어도 그 값을 다른 클립에 재사용하지 않는다.
/// </summary>
public class SfxClipIdAllocator
{
    public const string NEXT_ID_MARKER = "SfxEnumGenerator:nextId=";

    // "Name = 12," 와 "Name," 양쪽을 받는다. 후자는 명시값 도입 이전 파일의 마이그레이션용이다.
    private static readonly Regex MEMBER_PATTERN =
        new Regex(@"^\s*(?<name>[A-Za-z_]\w*)\s*(?:=\s*(?<id>\d+)\s*)?,\s*$", RegexOptions.Compiled);

    private static readonly Regex NEXT_ID_PATTERN =
        new Regex(NEXT_ID_MARKER + @"(?<id>\d+)", RegexOptions.Compiled);

    private readonly Dictionary<string, int> _ids = new Dictionary<string, int>();
    private int _nextId;

    public int NextId => _nextId;

    /// <summary>이미 값이 부여된 항목명. 클립이 사라져도 enum에서 지우지 않기 위해 참조한다.</summary>
    public IReadOnlyCollection<string> KnownNames => _ids.Keys;

    /// <summary>기존 ESfxClip.cs 내용에서 이미 부여된 값을 읽는다. 파일이 없으면 빈 문자열을 넘긴다.</summary>
    public void LoadFrom(string enumSource)
    {
        _ids.Clear();
        _nextId = 0;

        if (string.IsNullOrEmpty(enumSource))
        {
            return;
        }

        ParseMembers(enumSource);
        ParseNextId(enumSource);
    }

    public int GetOrAssign(string clipName)
    {
        if (_ids.TryGetValue(clipName, out int id))
        {
            return id;
        }

        id = _nextId;
        _ids[clipName] = id;
        _nextId++;
        return id;
    }

    private void ParseMembers(string enumSource)
    {
        string[] lines = enumSource.Split('\n');
        int ordinal = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            Match match = MEMBER_PATTERN.Match(lines[i].TrimEnd('\r'));
            if (!match.Success)
            {
                continue;
            }

            Group idGroup = match.Groups["id"];
            int id = idGroup.Success ? int.Parse(idGroup.Value) : ordinal;

            _ids[match.Groups["name"].Value] = id;
            ordinal++;

            if (_nextId <= id)
            {
                _nextId = id + 1;
            }
        }
    }

    // 마커는 마지막으로 부여한 값 다음을 가리킨다. 최대값이 달린 클립이 삭제되어도
    // 이 값을 읽어야 그 자리가 다른 클립에게 다시 나가지 않는다.
    private void ParseNextId(string enumSource)
    {
        Match match = NEXT_ID_PATTERN.Match(enumSource);
        if (!match.Success)
        {
            return;
        }

        int markedNextId = int.Parse(match.Groups["id"].Value);
        if (_nextId < markedNextId)
        {
            _nextId = markedNextId;
        }
    }
}
