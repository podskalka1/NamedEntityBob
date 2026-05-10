namespace bakalarka5.Core.Curation;

public class NormalizedAnnotationSpan
{
    public string Type { get; init; } = "";

    public int LineIndex { get; init; }

    public int StartPosition { get; init; }
    public int EndPosition { get; init; } // exclusive

    public int OriginalStartTokenIndex { get; init; }
    public int OriginalEndTokenIndex { get; init; } // exclusive

    public int Depth { get; init; }

    public string Key =>
        $"{Type}:{LineIndex}:{StartPosition}:{EndPosition}:{Depth}";

    public string RangeKey =>
        $"{LineIndex}:{StartPosition}:{EndPosition}:{Depth}";
}
