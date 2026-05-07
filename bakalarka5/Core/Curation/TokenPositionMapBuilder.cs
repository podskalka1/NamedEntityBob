using System.Collections.Generic;

namespace bakalarka5.Core.Curation;

public static class TokenPositionMapBuilder
{
    public static (TokenPositionMap A, TokenPositionMap B) Build(
        List<LineAlignment> lineAlignments,
        TokenAligner tokenAligner)
    {
        var mapA = new TokenPositionMap();
        var mapB = new TokenPositionMap();

        foreach (var line in lineAlignments)
        {
            if (line.A is null || line.B is null)
                continue;

            var tokenAlignments = tokenAligner.AlignTokens(line.A, line.B);

            int position = 0;

            foreach (var alignment in tokenAlignments)
            {
                if (alignment.A is not null)
                    mapA.Add(alignment.A, position);

                if (alignment.B is not null)
                    mapB.Add(alignment.B, position);

                position++;
            }
        }

        return (mapA, mapB);
    }
}