using System.Collections.Generic;
using System.Linq;

namespace bakalarka5.Core.Curation;

public static class NormalizedSpanExtractor
{
    public static List<NormalizedAnnotationSpan> ExtractSpans(
        CurationDocument document,
        TokenPositionMap positionMap)
    {
        var result = new List<NormalizedAnnotationSpan>();

        foreach (var line in document.Lines)
        {
            ExtractSpansFromLine(line, positionMap, result);
        }

        return result;
    }

    private static void ExtractSpansFromLine(
        CurationLine line,
        TokenPositionMap positionMap,
        List<NormalizedAnnotationSpan> result)
    {
        var open = new Dictionary<(int Depth, int InstanceId), SpanBuilder>();

        for (int tokenIndex = 0; tokenIndex < line.Tokens.Count; tokenIndex++)
        {
            var token = line.Tokens[tokenIndex];
            var position = positionMap.Get(token);

            var currentKeys = token.AnnotationPath
                .Select((layer, depth) => (Depth: depth, layer.InstanceId))
                .ToHashSet();

            foreach (var key in open.Keys.ToList())
            {
                if (!currentKeys.Contains(key))
                {
                    var builder = open[key];
                    result.Add(builder.Build(position));
                    open.Remove(key);
                }
            }

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
                        StartPosition = position,
                        Depth = depth
                    };
                }
            }
        }

        var endPosition = line.Tokens.Count == 0
            ? 0
            : positionMap.Get(line.Tokens[^1]) + 1;

        foreach (var builder in open.Values)
        {
            result.Add(builder.Build(endPosition));
        }
    }

    private class SpanBuilder
    {
        public string Type { get; init; } = "";
        public int LineIndex { get; init; }
        public int StartPosition { get; init; }
        public int Depth { get; init; }

        public NormalizedAnnotationSpan Build(int endPosition)
        {
            return new NormalizedAnnotationSpan
            {
                Type = Type,
                LineIndex = LineIndex,
                StartPosition = StartPosition,
                EndPosition = endPosition,
                Depth = Depth
            };
        }
    }
}