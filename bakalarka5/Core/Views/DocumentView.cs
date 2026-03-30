using System.Collections.Generic;
using System.Linq;

namespace bakalarka5.Core.Views;

public class DocumentView
{
    public Document Model { get; }

    public List<ParagraphView> Paragraphs { get; }

    public DocumentView(Document model)
    {
        Model = model;
        Paragraphs = model.Paragraphs.Select(p => new ParagraphView(p)).ToList();
    }
}