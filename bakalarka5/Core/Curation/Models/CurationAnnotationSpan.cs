namespace bakalarka5.Core.Curation;

public class CurationAnnotationSpan
{
    public string Type { get; init; } = "";

    public int LineIndex { get; init; }

    public int StartTokenIndex { get; init; }
    public int EndTokenIndex { get; init; } // exclusive

    public int Depth { get; init; }

    public CurationAnnotationSpan? Parent { get; init; }

    public string Key =>
        $"{Type}:{LineIndex}:{StartTokenIndex}:{EndTokenIndex}:{Depth}";
}