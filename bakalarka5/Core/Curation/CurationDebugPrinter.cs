using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using bakalarka5.Core.DocumentModel;

namespace bakalarka5.Core.Curation;

public static class CurationDebugPrinter
{
    public static void Print(Document documentA, Document documentB)
    {
        Console.OutputEncoding = Encoding.UTF8;
        
        var curationA = CurationDocumentFactory.FromDocument(documentA, AnnotatorSide.A);
        var curationB = CurationDocumentFactory.FromDocument(documentB, AnnotatorSide.B);

        Console.WriteLine("=== DOCUMENT A ===");
        PrintDocument(curationA);

        Console.WriteLine();
        Console.WriteLine("=== DOCUMENT B ===");
        PrintDocument(curationB);
        
        var aligner = new DocumentAligner();
        var lineAlignments = aligner.AlignLines(curationA, curationB);

        var tokenAligner = new TokenAligner();
        var (mapA, mapB) = TokenPositionMapBuilder.Build(lineAlignments, tokenAligner);

        Console.WriteLine();
        Console.WriteLine("=== RAW SPANS A ===");
        PrintSpans(curationA);

        Console.WriteLine("=== RAW SPANS B ===");
        PrintSpans(curationB);

        Console.WriteLine();
        Console.WriteLine("=== NORMALIZED SPANS A ===");
        PrintNormalizedSpans(curationA, mapA);

        Console.WriteLine("=== NORMALIZED SPANS B ===");
        PrintNormalizedSpans(curationB, mapB);
        
        Console.WriteLine();
        Console.WriteLine("=== LINE ALIGNMENTS ===");

        foreach (var alignment in lineAlignments)
        {
            Console.WriteLine($"[{alignment.Kind}]");
            Console.WriteLine($"A: {alignment.A?.PlainText ?? "<missing>"}");
            Console.WriteLine($"B: {alignment.B?.PlainText ?? "<missing>"}");
            Console.WriteLine();
        }

        Console.WriteLine("=== TOKEN ALIGNMENTS ===");

        foreach (var line in lineAlignments)
        {
            if (line.A is null || line.B is null)
                continue;

            Console.WriteLine($"Line {line.A.OriginalIndex}:");

            var tokenAlignments = tokenAligner.AlignTokens(line.A, line.B);

            foreach (var t in tokenAlignments)
            {
                var aText = t.A is null ? "<missing>" : FormatToken(t.A);
                var bText = t.B is null ? "<missing>" : FormatToken(t.B);

                Console.WriteLine($"[{t.Kind}] A: {aText} | B: {bText}");
            }

            Console.WriteLine();
        }

        var textConflicts = new ConflictDetector()
            .Detect(lineAlignments);

        var spanConflicts = new AnnotationSpanConflictDetector()
            .Detect(curationA, curationB, lineAlignments);

        var conflicts = textConflicts
            .Concat(spanConflicts)
            .ToList();
        
        Console.WriteLine();
        Console.WriteLine("=== CONFLICTS ===");

        foreach (var conflict in conflicts)
        {
            PrintConflict(conflict);
        }

        Console.WriteLine($"Total conflicts: {conflicts.Count}");
    }

    private static void PrintDocument(CurationDocument document)
    {
        foreach (var line in document.Lines)
        {
            Console.Write($"Line {line.OriginalIndex}: ");

            foreach (var token in line.Tokens)
            {
                Console.Write(FormatToken(token));
            }

            Console.WriteLine();
        }
    }

    private static void PrintConflict(CurationConflict conflict)
    {
        Console.WriteLine($"Conflict: {conflict.Kind}");

        if (conflict.LineA is not null)
            Console.WriteLine($"A line: {conflict.LineA.PlainText}");

        if (conflict.LineB is not null)
            Console.WriteLine($"B line: {conflict.LineB.PlainText}");

        if (conflict.TokenA is not null)
            Console.WriteLine($"A token: {FormatToken(conflict.TokenA)}");

        if (conflict.TokenB is not null)
            Console.WriteLine($"B token: {FormatToken(conflict.TokenB)}");
    
        if (conflict.TokensA.Count > 0)
            Console.WriteLine($"A tokens: {FormatTokens(conflict.TokensA)}");

        if (conflict.TokensB.Count > 0)
            Console.WriteLine($"B tokens: {FormatTokens(conflict.TokensB)}");

        if (conflict.SpanA is not null)
            Console.WriteLine($"A raw span: {FormatSpan(conflict.SpanA)}");

        if (conflict.SpanB is not null)
            Console.WriteLine($"B raw span: {FormatSpan(conflict.SpanB)}");

        if (conflict.NormalizedSpanA is not null)
            Console.WriteLine($"A normalized span: {FormatNormalizedSpan(conflict.NormalizedSpanA)}");

        if (conflict.NormalizedSpanB is not null)
            Console.WriteLine($"B normalized span: {FormatNormalizedSpan(conflict.NormalizedSpanB)}");

        Console.WriteLine();
    }

    private static string FormatToken(CurationToken token)
    {
        if (token.AnnotationPath.Count == 0)
            return token.Text;

        var path = string.Join(">", token.AnnotationPath.Select(x => $"{x.Type}#{x.InstanceId}"));
        return $"[{path}:{token.Text}]";
    }
    
    private static string FormatTokens(IEnumerable<CurationToken> tokens)
    {
        return string.Concat(tokens.Select(FormatToken));
    }
    
    private static string FormatSpan(CurationAnnotationSpan span)
    {
        return $"{span.Type} line={span.LineIndex} start={span.StartTokenIndex} end={span.EndTokenIndex} depth={span.Depth}";
    }

    private static string FormatNormalizedSpan(NormalizedAnnotationSpan span)
    {
        return $"{span.Type} line={span.LineIndex} start={span.StartPosition} end={span.EndPosition} depth={span.Depth}";
    }
    
    private static void PrintSpans(CurationDocument document)
    {
        var spans = CurationSpanExtractor.ExtractSpans(document);

        foreach (var span in spans)
        {
            Console.WriteLine(FormatSpan(span));
        }
    }

    private static void PrintNormalizedSpans(CurationDocument document, TokenPositionMap positionMap)
    {
        var spans = NormalizedSpanExtractor.ExtractSpans(document, positionMap);

        foreach (var span in spans)
        {
            Console.WriteLine(FormatNormalizedSpan(span));
        }
    }
}