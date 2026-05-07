using System;
using System.Collections.Generic;

namespace bakalarka5.Core.Curation;


public class TokenAligner
{
    public List<TokenAlignment> AlignTokens(CurationLine a, CurationLine b)
    {
        var tokensA = a.Tokens;
        var tokensB = b.Tokens;

        int n = tokensA.Count;
        int m = tokensB.Count;

        var lcs = new int[n + 1, m + 1];

        for (int i = n - 1; i >= 0; i--)
        {
            for (int j = m - 1; j >= 0; j--)
            {
                if (tokensA[i].Text == tokensB[j].Text)
                    lcs[i, j] = lcs[i + 1, j + 1] + 1;
                else
                    lcs[i, j] = Math.Max(lcs[i + 1, j], lcs[i, j + 1]);
            }
        }

        var result = new List<TokenAlignment>();

        int x = 0;
        int y = 0;

        while (x < n && y < m)
        {
            if (tokensA[x].Text == tokensB[y].Text)
            {
                result.Add(new TokenAlignment
                {
                    A = tokensA[x],
                    B = tokensB[y],
                    Kind = AlignmentKind.Same
                });

                x++;
                y++;
            }
            else if (lcs[x + 1, y] >= lcs[x, y + 1])
            {
                result.Add(new TokenAlignment
                {
                    A = tokensA[x],
                    B = null,
                    Kind = AlignmentKind.OnlyInA
                });

                x++;
            }
            else
            {
                result.Add(new TokenAlignment
                {
                    A = null,
                    B = tokensB[y],
                    Kind = AlignmentKind.OnlyInB
                });

                y++;
            }
        }

        while (x < n)
        {
            result.Add(new TokenAlignment
            {
                A = tokensA[x],
                B = null,
                Kind = AlignmentKind.OnlyInA
            });

            x++;
        }

        while (y < m)
        {
            result.Add(new TokenAlignment
            {
                A = null,
                B = tokensB[y],
                Kind = AlignmentKind.OnlyInB
            });

            y++;
        }

        return result;
    }
}