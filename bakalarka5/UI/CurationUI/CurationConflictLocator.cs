using System.Collections.Generic;
using System.Linq;
using bakalarka5.Core.Curation;

namespace bakalarka5.UI.CurationUI;

public static class CurationConflictLocator
{
    public static bool ContainsToken(CurationConflict conflict, CurationToken token, AnnotatorSide side)
    {
        return conflict.Kind switch
        {
            ConflictKind.LineOnlyInA when side == AnnotatorSide.A =>
                conflict.LineA?.OriginalIndex == token.OriginalLineIndex,

            ConflictKind.LineOnlyInB when side == AnnotatorSide.B =>
                conflict.LineB?.OriginalIndex == token.OriginalLineIndex,

            ConflictKind.TextMismatch when side == AnnotatorSide.A =>
                IsInLine(conflict.LineA, token) || conflict.TokensA.Contains(token),

            ConflictKind.TextMismatch when side == AnnotatorSide.B =>
                IsInLine(conflict.LineB, token) || conflict.TokensB.Contains(token),

            ConflictKind.AnnotationMismatch when side == AnnotatorSide.A =>
                ReferenceEquals(conflict.TokenA, token),

            ConflictKind.AnnotationMismatch when side == AnnotatorSide.B =>
                ReferenceEquals(conflict.TokenB, token),

            ConflictKind.AnnotationSpanOnlyInA when side == AnnotatorSide.A =>
                IsInNormalizedSpan(conflict.NormalizedSpanA, token),

            ConflictKind.AnnotationSpanOnlyInB when side == AnnotatorSide.B =>
                IsInNormalizedSpan(conflict.NormalizedSpanB, token),

            _ => false
        };
    }

    public static bool BelongsToLine(CurationConflict conflict, LineAlignment alignment)
    {
        var lineAIndex = alignment.A?.OriginalIndex;
        var lineBIndex = alignment.B?.OriginalIndex;

        return MatchesLine(conflict.LineA, lineAIndex) ||
               MatchesLine(conflict.LineB, lineBIndex) ||
               MatchesToken(conflict.TokenA, lineAIndex) ||
               MatchesToken(conflict.TokenB, lineBIndex) ||
               conflict.TokensA.Any(token => MatchesToken(token, lineAIndex)) ||
               conflict.TokensB.Any(token => MatchesToken(token, lineBIndex)) ||
               MatchesSpan(conflict.NormalizedSpanA, lineAIndex) ||
               MatchesSpan(conflict.NormalizedSpanB, lineBIndex);
    }

    public static CurationConflict? FindConflict(
        CurationToken token,
        AnnotatorSide side,
        IReadOnlyList<CurationConflict> conflicts,
        CurationConflict? currentConflict = null)
    {
        if (currentConflict is not null && ContainsToken(currentConflict, token, side))
            return currentConflict;

        foreach (var conflict in conflicts)
        {
            if (ContainsToken(conflict, token, side))
                return conflict;
        }

        return null;
    }

    private static bool IsInLine(CurationLine? line, CurationToken token)
    {
        return line?.OriginalIndex == token.OriginalLineIndex;
    }

    private static bool IsInNormalizedSpan(NormalizedAnnotationSpan? span, CurationToken token)
    {
        if (span is null || span.LineIndex != token.OriginalLineIndex)
            return false;

        return token.OriginalTokenIndex >= span.OriginalStartTokenIndex &&
               token.OriginalTokenIndex < span.OriginalEndTokenIndex;
    }

    private static bool MatchesLine(CurationLine? line, int? originalLineIndex)
    {
        return line is not null && originalLineIndex == line.OriginalIndex;
    }

    private static bool MatchesToken(CurationToken? token, int? originalLineIndex)
    {
        return token is not null && originalLineIndex == token.OriginalLineIndex;
    }

    private static bool MatchesSpan(NormalizedAnnotationSpan? span, int? originalLineIndex)
    {
        return span is not null && originalLineIndex == span.LineIndex;
    }
}
