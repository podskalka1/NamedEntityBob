using System.Collections.Generic;
using System.Linq;

namespace bakalarka5.Core.Curation;

public class TokenAlignmentGroup
{
    public List<CurationToken> TokensA { get; } = new();
    public List<CurationToken> TokensB { get; } = new();

    public AlignmentKind Kind { get; init; }

    public string TextA => string.Concat(TokensA.Select(t => t.Text));
    public string TextB => string.Concat(TokensB.Select(t => t.Text));
}