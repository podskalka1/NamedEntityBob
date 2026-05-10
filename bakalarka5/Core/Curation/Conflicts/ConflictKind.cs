namespace bakalarka5.Core.Curation;

public enum ConflictKind
{
    LineOnlyInA,
    LineOnlyInB,
    TokenOnlyInA,
    TokenOnlyInB,
    TextMismatch,

    AnnotationMismatch,
    AnnotationSpanOnlyInA,
    AnnotationSpanOnlyInB
}