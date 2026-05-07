using System.Collections.Generic;
using System.Linq;
using bakalarka5.Core.Curation;

namespace bakalarka5.UI.CurationUI;

public class CurationDocumentView
{
    public List<CurationLineView> Lines { get; }

    public CurationDocumentView(
        CurationDocument document,
        AnnotatorSide side,
        IReadOnlyList<CurationConflict> conflicts,
        CurationConflict? currentConflict)
    {
        Lines = document.Lines
            .Select(line => new CurationLineView(line, side, conflicts, currentConflict))
            .ToList();
    }
}
