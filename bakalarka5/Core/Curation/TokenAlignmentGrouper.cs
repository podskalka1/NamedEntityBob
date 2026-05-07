using System.Collections.Generic;

namespace bakalarka5.Core.Curation;

public static class TokenAlignmentGrouper
{
    public static List<TokenAlignmentGroup> Group(List<TokenAlignment> alignments)
    {
        var result = new List<TokenAlignmentGroup>();

        int i = 0;

        while (i < alignments.Count)
        {
            var current = alignments[i];

            if (current.Kind == AlignmentKind.Same)
            {
                result.Add(new TokenAlignmentGroup
                {
                    Kind = AlignmentKind.Same
                });

                result[^1].TokensA.Add(current.A!);
                result[^1].TokensB.Add(current.B!);

                i++;
                continue;
            }

            var group = new TokenAlignmentGroup
            {
                Kind = AlignmentKind.Different
            };

            while (i < alignments.Count && alignments[i].Kind != AlignmentKind.Same)
            {
                if (alignments[i].A is not null)
                    group.TokensA.Add(alignments[i].A!);

                if (alignments[i].B is not null)
                    group.TokensB.Add(alignments[i].B!);

                i++;
            }

            result.Add(group);
        }

        return result;
    }
}