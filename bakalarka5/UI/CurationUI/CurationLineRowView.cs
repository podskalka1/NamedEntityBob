using System.Collections.Generic;
using System.Linq;
using bakalarka5.Core.Curation;

namespace bakalarka5.UI.CurationUI;

public class CurationLineRowView
{
    public LineAlignment Alignment { get; }
    public List<CurationConflict> Conflicts { get; }
    public List<CurationLineVariantView> Variants { get; } = new();

    public bool HasConflict => Conflicts.Count > 0;
    public bool HasUnresolvedConflict => Conflicts.Any(conflict => !conflict.IsResolved);

    public CurationLineRowView(
        LineAlignment alignment,
        IReadOnlyList<CurationConflict> allConflicts,
        CurationConflict? currentConflict)
    {
        Alignment = alignment;
        Conflicts = allConflicts
            .Where(conflict => CurationConflictLocator.BelongsToLine(conflict, alignment))
            .ToList();

        BuildVariants(currentConflict);
    }

    private void BuildVariants(CurationConflict? currentConflict)
    {
        var lineA = Alignment.A;
        var lineB = Alignment.B;

        if (!HasUnresolvedConflict)
        {
            AddSingleResolvedVariant(lineA, lineB, currentConflict);
            return;
        }

        if (lineA is not null)
            Variants.Add(new CurationLineVariantView(
                lineA,
                AnnotatorSide.A,
                lineB,
                GetResolvedSide(),
                Conflicts,
                currentConflict));

        if (lineB is not null)
            Variants.Add(new CurationLineVariantView(
                lineB,
                AnnotatorSide.B,
                lineA,
                GetResolvedSide(),
                Conflicts,
                currentConflict));
    }

    private void AddSingleResolvedVariant(
        CurationLine? lineA,
        CurationLine? lineB,
        CurationConflict? currentConflict)
    {
        var side = GetResolvedSide() ?? (lineA is not null ? AnnotatorSide.A : AnnotatorSide.B);
        var line = side == AnnotatorSide.B ? lineB ?? lineA : lineA ?? lineB;
        var otherLine = side == AnnotatorSide.B ? lineA : lineB;

        if (line is null)
            return;

        Variants.Add(new CurationLineVariantView(
            line,
            side,
            otherLine,
            side,
            Conflicts,
            currentConflict));
    }

    private AnnotatorSide? GetResolvedSide()
    {
        var resolved = Conflicts
            .Where(conflict => conflict.IsResolved)
            .Select(conflict => conflict.Resolution)
            .ToList();

        if (resolved.Count == 0)
            return null;

        if (resolved.LastOrDefault(resolution => resolution == CurationResolutionKind.UseB) == CurationResolutionKind.UseB)
            return AnnotatorSide.B;

        if (resolved.LastOrDefault(resolution => resolution == CurationResolutionKind.UseA) == CurationResolutionKind.UseA)
            return AnnotatorSide.A;

        return null;
    }
}
