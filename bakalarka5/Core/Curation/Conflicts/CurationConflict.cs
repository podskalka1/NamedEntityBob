using System.Collections.Generic;

namespace bakalarka5.Core.Curation;

public class CurationConflict
{
    public ConflictKind Kind { get; init; }

    public CurationLine? LineA { get; init; }
    public CurationLine? LineB { get; init; }

    public CurationToken? TokenA { get; init; }
    public CurationToken? TokenB { get; init; }

    public List<CurationToken> TokensA { get; init; } = new();
    public List<CurationToken> TokensB { get; init; } = new();

    public CurationAnnotationSpan? SpanA { get; init; }
    public CurationAnnotationSpan? SpanB { get; init; }
    
    public NormalizedAnnotationSpan? NormalizedSpanA { get; init; }
    public NormalizedAnnotationSpan? NormalizedSpanB { get; init; }
    
    public CurationResolutionKind Resolution { get; private set; } = CurationResolutionKind.Unresolved;

    public bool IsResolved => Resolution != CurationResolutionKind.Unresolved;

    public void Resolve(CurationResolutionKind resolution)
    {
        Resolution = resolution;
    }
}