using System.Collections.Generic;
using bakalarka5.Core.Curation;

namespace bakalarka5.UI.CurationUI;

public class CurationLineVariantView
{
    public AnnotatorSide Side { get; }
    public string Header => Side == AnnotatorSide.A ? "A" : "B";
    public CurationLineView Line { get; }

    public CurationLineVariantView(
        CurationLine line,
        AnnotatorSide side,
        CurationLine? otherLine,
        AnnotatorSide? resolvedSide,
        IReadOnlyList<CurationConflict> conflicts,
        CurationConflict? currentConflict)
    {
        Side = side;
        Line = new CurationLineView(line, side, otherLine, resolvedSide, conflicts, currentConflict);
    }
}
