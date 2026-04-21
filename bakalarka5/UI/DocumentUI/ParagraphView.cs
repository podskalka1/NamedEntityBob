using System.Collections.Generic;
using System.Linq;
using bakalarka5.Core.DocumentModel;
using bakalarka5.Core.Selection;

namespace bakalarka5.UI.DocumentUI;

public class ParagraphView
{
    public ParagraphItem Model { get; }
    public List<LineView> Lines { get; }

    public ParagraphView(ParagraphItem model, SelectionManager selectionManager)
    {
        Model = model;
        Lines = model.Lines
            .Select(l => new LineView(l, selectionManager))
            .ToList();
    }
}