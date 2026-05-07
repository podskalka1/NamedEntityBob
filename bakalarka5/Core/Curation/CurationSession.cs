using System;
using System.Collections.Generic;
using System.Linq;
using bakalarka5.Core.DocumentModel;

namespace bakalarka5.Core.Curation;

public class CurationSession
{
    public CurationDocument DocumentA { get; }
    public CurationDocument DocumentB { get; }

    public List<CurationConflict> Conflicts { get; }

    public int CurrentConflictIndex { get; private set; }

    public CurationConflict? CurrentConflict =>
        Conflicts.Count == 0 ? null : Conflicts[CurrentConflictIndex];

    public CurationSession(Document documentA, Document documentB)
    {
        DocumentA = CurationDocumentFactory.FromDocument(documentA, AnnotatorSide.A);
        DocumentB = CurationDocumentFactory.FromDocument(documentB, AnnotatorSide.B);

        var lineAlignments = new DocumentAligner().AlignLines(DocumentA, DocumentB);

        var textConflicts = new ConflictDetector()
            .Detect(lineAlignments);

        var spanConflicts = new AnnotationSpanConflictDetector()
            .Detect(DocumentA, DocumentB, lineAlignments);

        Conflicts = textConflicts
            .Concat(spanConflicts)
            .ToList();
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
        CurrentConflict?.Resolve(resolution);
    }
    
    public void GoTo(int index)
    {
        if (index < 0 || index >= Conflicts.Count)
            return;

        CurrentConflictIndex = index;
    }
}