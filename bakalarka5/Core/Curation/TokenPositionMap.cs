using System.Collections.Generic;

namespace bakalarka5.Core.Curation;

public class TokenPositionMap
{
    private readonly Dictionary<CurationToken, int> _positions = new();

    public void Add(CurationToken token, int position)
    {
        _positions[token] = position;
    }

    public int Get(CurationToken token)
    {
        return _positions[token];
    }
}