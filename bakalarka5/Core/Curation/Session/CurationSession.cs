using System;
using System.Collections.Generic;
using System.Linq;
using bakalarka5.Core.DocumentModel;

namespace bakalarka5.Core.Curation;

public class CurationSession
{
    public CurationDocument DocumentA { get; }
    public CurationDocument DocumentB { get; }

    public List<LineAlignment> LineAlignments { get; private set; } = new();
    public List<CurationConflict> Conflicts { get; private set; } = new();

    public int CurrentConflictIndex { get; private set; }

    public CurationConflict? CurrentConflict =>
        Conflicts.Count == 0 ? null : Conflicts[CurrentConflictIndex];

    public CurationSession(Document documentA, Document documentB)
    {
        DocumentA = CurationDocumentFactory.FromDocument(documentA, AnnotatorSide.A);
        DocumentB = CurationDocumentFactory.FromDocument(documentB, AnnotatorSide.B);

        RebuildConflicts();
    }

    public void Next()
    {
        if (Conflicts.Count == 0)
            return;

        CurrentConflictIndex = Math.Min(CurrentConflictIndex + 1, Conflicts.Count - 1);
    }

    public void Previous()
    {
        if (Conflicts.Count == 0)
            return;

        CurrentConflictIndex = Math.Max(CurrentConflictIndex - 1, 0);
    }

    public void ResolveCurrent(CurationResolutionKind resolution)
    {
        var conflict = CurrentConflict;
        if (conflict is null)
            return;

        var previousIndex = CurrentConflictIndex;
        ApplyResolution(conflict, resolution);
        RebuildConflicts();

        if (Conflicts.Count == 0)
        {
            CurrentConflictIndex = 0;
            return;
        }

        CurrentConflictIndex = Math.Min(previousIndex, Conflicts.Count - 1);
    }

    public void ResolveAll(CurationResolutionKind resolution)
    {
        while (Conflicts.Count > 0)
            ResolveCurrent(resolution);
    }

    public string SerializeCuratedDocument(string title = "Curated")
    {
        return CurationDocumentSerializer.Serialize(DocumentA, title);
    }

    public void GoTo(int index)
    {
        if (index < 0 || index >= Conflicts.Count)
            return;

        CurrentConflictIndex = index;
    }

    private void RebuildConflicts()
    {
        LineAlignments = new DocumentAligner().AlignLines(DocumentA, DocumentB);

        var textConflicts = new ConflictDetector()
            .Detect(LineAlignments);

        var spanConflicts = new AnnotationSpanConflictDetector()
            .Detect(DocumentA, DocumentB, LineAlignments);

        Conflicts = textConflicts
            .Concat(spanConflicts)
            .ToList();

        if (Conflicts.Count == 0)
            CurrentConflictIndex = 0;
        else
            CurrentConflictIndex = Math.Min(CurrentConflictIndex, Conflicts.Count - 1);
    }

    private void ApplyResolution(CurationConflict conflict, CurationResolutionKind resolution)
    {
        switch (conflict.Kind)
        {
            case ConflictKind.TextMismatch:
                ApplyTextResolution(conflict, resolution);
                break;

            case ConflictKind.AnnotationMismatch:
                ApplyAnnotationMismatchResolution(conflict, resolution);
                break;

            case ConflictKind.AnnotationSpanOnlyInA:
                ApplySpanResolution(conflict.NormalizedSpanA, AnnotatorSide.A, resolution);
                break;

            case ConflictKind.AnnotationSpanOnlyInB:
                ApplySpanResolution(conflict.NormalizedSpanB, AnnotatorSide.B, resolution);
                break;

            case ConflictKind.LineOnlyInA:
            case ConflictKind.LineOnlyInB:
            case ConflictKind.TokenOnlyInA:
            case ConflictKind.TokenOnlyInB:
                break;
        }
    }

    private void ApplyTextResolution(CurationConflict conflict, CurationResolutionKind resolution)
    {
        if (resolution == CurationResolutionKind.UseA)
            ReplaceTokens(DocumentB, conflict.TokensB, conflict.TokensA, GetLineIndex(conflict, AnnotatorSide.B));
        else if (resolution == CurationResolutionKind.UseB)
            ReplaceTokens(DocumentA, conflict.TokensA, conflict.TokensB, GetLineIndex(conflict, AnnotatorSide.A));
    }

    private void ApplyAnnotationMismatchResolution(CurationConflict conflict, CurationResolutionKind resolution)
    {
        if (conflict.TokenA is null || conflict.TokenB is null)
            return;

        if (resolution == CurationResolutionKind.UseA)
            ApplyAnnotationPathTypes(conflict.TokenB, conflict.TokenA.AnnotationPath);
        else if (resolution == CurationResolutionKind.UseB)
            ApplyAnnotationPathTypes(conflict.TokenA, conflict.TokenB.AnnotationPath);
    }

    private void ApplySpanResolution(
        NormalizedAnnotationSpan? span,
        AnnotatorSide sourceSide,
        CurationResolutionKind resolution)
    {
        if (span is null)
            return;

        var sourceDocument = sourceSide == AnnotatorSide.A ? DocumentA : DocumentB;
        var targetDocument = sourceSide == AnnotatorSide.A ? DocumentB : DocumentA;

        if ((sourceSide == AnnotatorSide.A && resolution == CurationResolutionKind.UseA) ||
            (sourceSide == AnnotatorSide.B && resolution == CurationResolutionKind.UseB))
        {
            AddSpan(targetDocument, span);
            return;
        }

        RemoveSpan(sourceDocument, span);
    }

    private static void ReplaceTokens(
        CurationDocument document,
        IReadOnlyList<CurationToken> oldTokens,
        IReadOnlyList<CurationToken> newTokens,
        int lineIndex)
    {
        var line = GetLine(document, lineIndex);
        if (line is null)
            return;

        var start = GetStartIndex(oldTokens, newTokens);
        if (start is null)
            return;

        var count = oldTokens.Count == 0
            ? 0
            : oldTokens.Max(token => token.OriginalTokenIndex) - start.Value + 1;

        line.Tokens.RemoveRange(start.Value, count);
        line.Tokens.InsertRange(start.Value, CloneTokens(newTokens, line.OriginalIndex, start.Value));
        ReindexTokens(line);
    }

    private int GetLineIndex(CurationConflict conflict, AnnotatorSide side)
    {
        var tokens = side == AnnotatorSide.A ? conflict.TokensA : conflict.TokensB;
        if (tokens.Count > 0)
            return tokens[0].OriginalLineIndex;

        var otherTokens = side == AnnotatorSide.A ? conflict.TokensB : conflict.TokensA;
        if (otherTokens.Count > 0)
            return otherTokens[0].OriginalLineIndex;

        return 0;
    }

    private static int? GetStartIndex(
        IReadOnlyList<CurationToken> oldTokens,
        IReadOnlyList<CurationToken> newTokens)
    {
        if (oldTokens.Count > 0)
            return oldTokens.Min(token => token.OriginalTokenIndex);

        if (newTokens.Count > 0)
            return newTokens.Min(token => token.OriginalTokenIndex);

        return null;
    }

    private static List<CurationToken> CloneTokens(
        IReadOnlyList<CurationToken> tokens,
        int lineIndex,
        int startIndex)
    {
        return tokens
            .Select((token, index) => CloneToken(token, lineIndex, startIndex + index))
            .ToList();
    }

    private static CurationToken CloneToken(CurationToken token, int lineIndex, int tokenIndex)
    {
        return new CurationToken
        {
            Text = token.Text,
            OriginalLineIndex = lineIndex,
            OriginalTokenIndex = tokenIndex,
            AnnotationPath = CloneAnnotationPath(token.AnnotationPath)
        };
    }

    private static List<CurationAnnotationLayer> CloneAnnotationPath(
        IReadOnlyList<CurationAnnotationLayer> path)
    {
        return path
            .Select(layer => new CurationAnnotationLayer
            {
                Type = layer.Type,
                InstanceId = layer.InstanceId
            })
            .ToList();
    }

    private static void ApplyAnnotationPathTypes(
        CurationToken token,
        IReadOnlyList<CurationAnnotationLayer> sourcePath)
    {
        var commonLength = Math.Min(token.AnnotationPath.Count, sourcePath.Count);

        for (var index = 0; index < commonLength; index++)
        {
            token.AnnotationPath[index] = new CurationAnnotationLayer
            {
                Type = sourcePath[index].Type,
                InstanceId = token.AnnotationPath[index].InstanceId
            };
        }

        while (token.AnnotationPath.Count > sourcePath.Count)
            token.AnnotationPath.RemoveAt(token.AnnotationPath.Count - 1);

        for (var index = commonLength; index < sourcePath.Count; index++)
        {
            token.AnnotationPath.Add(new CurationAnnotationLayer
            {
                Type = sourcePath[index].Type,
                InstanceId = sourcePath[index].InstanceId
            });
        }
    }

    private static void AddSpan(CurationDocument document, NormalizedAnnotationSpan span)
    {
        var line = GetLine(document, span.LineIndex);
        if (line is null)
            return;

        foreach (var token in TokensInSpan(line, span))
        {
            while (token.AnnotationPath.Count < span.Depth)
            {
                token.AnnotationPath.Add(new CurationAnnotationLayer
                {
                    Type = "",
                    InstanceId = 0
                });
            }

            var layer = new CurationAnnotationLayer
            {
                Type = span.Type,
                InstanceId = span.GetHashCode()
            };

            if (token.AnnotationPath.Count == span.Depth)
                token.AnnotationPath.Add(layer);
            else
                token.AnnotationPath[span.Depth] = layer;
        }
    }

    private static void RemoveSpan(CurationDocument document, NormalizedAnnotationSpan span)
    {
        var line = GetLine(document, span.LineIndex);
        if (line is null)
            return;

        foreach (var token in TokensInSpan(line, span))
        {
            if (token.AnnotationPath.Count <= span.Depth)
                continue;

            if (token.AnnotationPath[span.Depth].Type == span.Type)
                token.AnnotationPath.RemoveAt(span.Depth);
        }
    }

    private static IEnumerable<CurationToken> TokensInSpan(CurationLine line, NormalizedAnnotationSpan span)
    {
        return line.Tokens.Where(token =>
            token.OriginalTokenIndex >= span.OriginalStartTokenIndex &&
            token.OriginalTokenIndex < span.OriginalEndTokenIndex);
    }

    private static CurationLine? GetLine(CurationDocument document, int lineIndex)
    {
        return document.Lines.FirstOrDefault(line => line.OriginalIndex == lineIndex);
    }

    private static void ReindexTokens(CurationLine line)
    {
        for (var index = 0; index < line.Tokens.Count; index++)
        {
            var token = line.Tokens[index];
            line.Tokens[index] = CloneToken(token, line.OriginalIndex, index);
        }
    }
}
