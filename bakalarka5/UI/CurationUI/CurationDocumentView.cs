using System.Collections.Generic;
using System.Linq;
using bakalarka5.Core.Curation;

namespace bakalarka5.UI.CurationUI;

public class CurationDocumentView
{
    public List<CurationLineRowView> Rows { get; }

    public CurationDocumentView(
        IReadOnlyList<LineAlignment> alignments,
        IReadOnlyList<CurationConflict> conflicts,
        CurationConflict? currentConflict)
    {
        Rows = alignments
            .Select(alignment => new CurationLineRowView(alignment, conflicts, currentConflict))
            .Where(row => row.HasConflict || row.Variants.Any(variant => variant.Line.Tokens.Count > 0))
            .ToList();
    }
}
