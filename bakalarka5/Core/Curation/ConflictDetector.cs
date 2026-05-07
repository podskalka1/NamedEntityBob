using System.Collections.Generic;
using System.Linq;

namespace bakalarka5.Core.Curation;

public class ConflictDetector
{
    private readonly TokenAligner _tokenAligner = new();

    public List<CurationConflict> Detect(List<LineAlignment> lines)
    {
        var conflicts = new List<CurationConflict>();

        foreach (var line in lines)
        {
            if (line.Kind == AlignmentKind.OnlyInA)
            {
                conflicts.Add(new CurationConflict
                {
                    Kind = ConflictKind.LineOnlyInA,
                    LineA = line.A
                });
                continue;
            }

            if (line.Kind == AlignmentKind.OnlyInB)
            {
                conflicts.Add(new CurationConflict
                {
                    Kind = ConflictKind.LineOnlyInB,
                    LineB = line.B
                });
                continue;
            }

            if (line.Kind == AlignmentKind.Different)
            {
                conflicts.Add(new CurationConflict
                {
                    Kind = ConflictKind.TextMismatch,
                    LineA = line.A,
                    LineB = line.B
                });
                continue;
            }
            
            var tokenAlignments = _tokenAligner.AlignTokens(line.A!, line.B!);
            var groups = TokenAlignmentGrouper.Group(tokenAlignments);

            foreach (var group in groups)
            {
                if (group.Kind == AlignmentKind.Different)
                {
                    conflicts.Add(new CurationConflict
                    {
                        Kind = ConflictKind.TextMismatch,
                        TokensA = group.TokensA,
                        TokensB = group.TokensB
                    });

                    continue;
                }

                var tokenA = group.TokensA.Single();
                var tokenB = group.TokensB.Single();

                if (!AnnotationTypesEqual(tokenA, tokenB))
                {
                    conflicts.Add(new CurationConflict
                    {
                        Kind = ConflictKind.AnnotationMismatch,
                        TokenA = tokenA,
                        TokenB = tokenB
                    });
                }
            }
        }

        return conflicts;
    }
    
    private static bool AnnotationTypesEqual(CurationToken a, CurationToken b)
    {
        var pathA = a.AnnotationPath.Select(x => x.Type);
        var pathB = b.AnnotationPath.Select(x => x.Type);

        return pathA.SequenceEqual(pathB);
    }
}