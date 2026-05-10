using System.Collections.Generic;
using System.Linq;

namespace bakalarka5.Core.Curation;

public class AnnotationSpanConflictDetector
{
    public List<CurationConflict> Detect(
        CurationDocument documentA,
        CurationDocument documentB,
        List<LineAlignment> lineAlignments)
    {
        var tokenAligner = new TokenAligner();

        var (mapA, mapB) = TokenPositionMapBuilder.Build(
            lineAlignments,
            tokenAligner);

        var spansA = NormalizedSpanExtractor.ExtractSpans(documentA, mapA);
        var spansB = NormalizedSpanExtractor.ExtractSpans(documentB, mapB);

        var keysA = spansA.ToDictionary(s => s.Key);
        var keysB = spansB.ToDictionary(s => s.Key);
        var rangeKeysA = spansA.Select(s => s.RangeKey).ToHashSet();
        var rangeKeysB = spansB.Select(s => s.RangeKey).ToHashSet();

        var conflicts = new List<CurationConflict>();

        foreach (var spanA in spansA)
        {
            if (!keysB.ContainsKey(spanA.Key))
            {
                if (rangeKeysB.Contains(spanA.RangeKey))
                    continue;

                conflicts.Add(new CurationConflict
                {
                    Kind = ConflictKind.AnnotationSpanOnlyInA,
                    NormalizedSpanA = spanA
                });
            }
        }

        foreach (var spanB in spansB)
        {
            if (!keysA.ContainsKey(spanB.Key))
            {
                if (rangeKeysA.Contains(spanB.RangeKey))
                    continue;

                conflicts.Add(new CurationConflict
                {
                    Kind = ConflictKind.AnnotationSpanOnlyInB,
                    NormalizedSpanB = spanB
                });
            }
        }

        return conflicts;
    }
}
