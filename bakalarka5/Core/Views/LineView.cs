using System.Collections.Generic;
using System.Linq;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public class LineView
{
    public LineItem Model { get; }

    public List<TokenView> Tokens { get; }

    public LineView(LineItem model)
    {
        Model = model;
        Tokens = model.Tokens.Select(t => new TokenView(t)).ToList();
    }
}