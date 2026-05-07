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
        IReadOnlyList<CurationConflict> conflicts,
        CurationConflict? currentConflict)
    {
        Model = model;
        Tokens = model.Tokens
            .Select(token => new CurationTokenView(
                token,
                side,
                FindConflict(token, side, conflicts),
                currentConflict))
            .ToList();
    }

    private static CurationConflict? FindConflict(
        CurationToken token,
        AnnotatorSide side,
        IReadOnlyList<CurationConflict> conflicts)
    {
        foreach (var conflict in conflicts)
        {
            if (ContainsToken(conflict, token, side))
                return conflict;
        }

        return null;
    }

    private static bool ContainsToken(CurationConflict conflict, CurationToken token, AnnotatorSide side)
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

    private static bool IsInLine(CurationLine? line, CurationToken token)
    {
        return line?.OriginalIndex == token.OriginalLineIndex;
    }

    private static bool IsInNormalizedSpan(NormalizedAnnotationSpan? span, CurationToken token)
    {
        if (span is null || span.LineIndex != token.OriginalLineIndex)
            return false;

        return token.OriginalTokenIndex >= span.StartPosition &&
               token.OriginalTokenIndex < span.EndPosition;
    }
}
