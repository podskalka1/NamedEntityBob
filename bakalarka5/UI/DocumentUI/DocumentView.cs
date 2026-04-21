using System.Collections.Generic;
using System.Linq;
using bakalarka5.Core.DocumentModel;
using bakalarka5.Core.Selection;

namespace bakalarka5.UI.DocumentUI;

public class DocumentView
{
    public Document Model { get; }
    public List<ParagraphView> Paragraphs { get; }

    public DocumentView(Document model, SelectionManager selectionManager)
    {
        Model = model;
        Paragraphs = model.Paragraphs
            .Select(p => new ParagraphView(p, selectionManager))
            .ToList();
    }
}