using System.Collections.Generic;
using System.Linq;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public class ParagraphView
{
    public ParagraphItem Model { get; }

    public List<LineView> Lines { get; }

    public ParagraphView(ParagraphItem model)
    {
        Model = model;
        Lines = model.Lines.Select(l => new LineView(l)).ToList();
    }
}