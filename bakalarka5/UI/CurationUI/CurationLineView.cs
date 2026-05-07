using System.Collections.Generic;
using System.Linq;
using bakalarka5.Core.Curation;

namespace bakalarka5.UI.CurationUI;

public class CurationLineView
{
    public CurationLine Model { get; }
    public List<CurationTokenView> Tokens { get; }

    public CurationLineView(
        CurationLine model,
        AnnotatorSide side,
        CurationLine? otherLine,
        AnnotatorSide? resolvedSide,
        IReadOnlyList<CurationConflict> lineConflicts,
        CurationConflict? currentConflict)
    {
        Model = model;
        Tokens = BuildDisplayTokens(model, side, lineConflicts)
            .Select(token => new CurationTokenView(
                token,
                side,
                CurationConflictLocator.FindConflict(token, side, lineConflicts, currentConflict),
                currentConflict))
            .ToList();
    }

    private static List<CurationToken> BuildDisplayTokens(
        CurationLine line,
        AnnotatorSide side,
        IReadOnlyList<CurationConflict> lineConflicts)
    {
        var replacements = lineConflicts
            .Where(conflict => conflict is
            {
                Kind: ConflictKind.TextMismatch,
                IsResolved: true
            })
            .Where(conflict => conflict.Resolution is CurationResolutionKind.UseA or CurationResolutionKind.UseB)
            .Select(conflict => BuildReplacement(conflict, side))
            .Where(replacement => replacement is not null)
            .Select(replacement => replacement!)
            .OrderBy(replacement => replacement.Start)
            .ToList();

        if (replacements.Count == 0)
            return line.Tokens.ToList();

        var result = new List<CurationToken>();
        var tokenIndex = 0;

        foreach (var replacement in replacements)
        {
            while (tokenIndex < replacement.Start && tokenIndex < line.Tokens.Count)
            {
                result.Add(line.Tokens[tokenIndex]);
                tokenIndex++;
            }

            result.AddRange(replacement.Tokens);
            tokenIndex = replacement.End;
        }

        while (tokenIndex < line.Tokens.Count)
        {
            result.Add(line.Tokens[tokenIndex]);
            tokenIndex++;
        }

        return result;
    }

    private static TokenReplacement? BuildReplacement(CurationConflict conflict, AnnotatorSide side)
    {
        var affectedTokens = side == AnnotatorSide.A
            ? conflict.TokensA
            : conflict.TokensB;

        var chosenTokens = conflict.Resolution == CurationResolutionKind.UseA
            ? conflict.TokensA
            : conflict.TokensB;

        var start = GetStartIndex(affectedTokens, chosenTokens);
        if (start is null)
            return null;

        var end = affectedTokens.Count == 0
            ? start.Value
            : affectedTokens.Max(token => token.OriginalTokenIndex) + 1;

        return new TokenReplacement(start.Value, end, chosenTokens);
    }

    private static int? GetStartIndex(
        IReadOnlyList<CurationToken> affectedTokens,
        IReadOnlyList<CurationToken> chosenTokens)
    {
        if (affectedTokens.Count > 0)
            return affectedTokens.Min(token => token.OriginalTokenIndex);

        if (chosenTokens.Count > 0)
            return chosenTokens.Min(token => token.OriginalTokenIndex);

        return null;
    }

    private sealed record TokenReplacement(
        int Start,
        int End,
        IReadOnlyList<CurationToken> Tokens);
}
