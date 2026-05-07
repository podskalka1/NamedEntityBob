using System.Collections.Generic;

namespace bakalarka5.Core.Curation;

public class CurationToken
{
    public string Text { get; init; } = "";
    public int OriginalLineIndex { get; init; }
    public int OriginalTokenIndex { get; init; }

    public List<CurationAnnotationLayer> AnnotationPath { get; init; } = new();
}