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

        var conflicts = new List<CurationConflict>();

        foreach (var spanA in spansA)
        {
            if (!keysB.ContainsKey(spanA.Key))
            {
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