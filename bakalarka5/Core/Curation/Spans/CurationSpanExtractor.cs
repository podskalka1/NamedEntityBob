using System.Collections.Generic;
using System.Linq;

namespace bakalarka5.Core.Curation;

public static class CurationSpanExtractor
{
    public static List<CurationAnnotationSpan> ExtractSpans(CurationDocument document)
    {
        var result = new List<CurationAnnotationSpan>();

        foreach (var line in document.Lines)
        {
            ExtractSpansFromLine(line, result);
        }

        return result;
    }

    private static void ExtractSpansFromLine(
        CurationLine line,
        List<CurationAnnotationSpan> result)
    {
        var open = new Dictionary<(int Depth, int InstanceId), SpanBuilder>();

        for (int tokenIndex = 0; tokenIndex < line.Tokens.Count; tokenIndex++)
        {
            var token = line.Tokens[tokenIndex];

            var currentKeys = token.AnnotationPath
                .Select((layer, depth) => (Depth: depth, layer.InstanceId))
                .ToHashSet();

            // close spans no longer active
            foreach (var key in open.Keys.ToList())
            {
                if (!currentKeys.Contains(key))
                {
                    var builder = open[key];
                    result.Add(builder.Build(endTokenIndex: tokenIndex));
                    open.Remove(key);
                }
            }

            // open new spans
            for (int depth = 0; depth < token.AnnotationPath.Count; depth++)
            {
                var layer = token.AnnotationPath[depth];
                var key = (Depth: depth, layer.InstanceId);

                if (!open.ContainsKey(key))
                {
                    open[key] = new SpanBuilder
                    {
                        Type = layer.Type,
                        LineIndex = line.OriginalIndex,
                        StartTokenIndex = tokenIndex,
                        Depth = depth
                    };
                }
            }
        }

        // close remaining spans at line end
        foreach (var builder in open.Values)
        {
            result.Add(builder.Build(line.Tokens.Count));
        }
    }

    private class SpanBuilder
    {
        public string Type { get; init; } = "";
        public int LineIndex { get; init; }
        public int StartTokenIndex { get; init; }
        public int Depth { get; init; }

        public CurationAnnotationSpan Build(int endTokenIndex)
        {
            return new CurationAnnotationSpan
            {
                Type = Type,
                LineIndex = LineIndex,
                StartTokenIndex = StartTokenIndex,
                EndTokenIndex = endTokenIndex,
                Depth = Depth
            };
        }
    }
}