using System.Collections.Generic;
using System.Linq;

namespace bakalarka5.Core.Curation;

public class CurationLine
{
    public int OriginalIndex { get; init; }
    public List<CurationToken> Tokens { get; } = new();

    public string PlainText => string.Concat(Tokens.Select(t => t.Text));
}