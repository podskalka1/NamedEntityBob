using System;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public static class ViewFactory
{
    public static InlineNodeView Create(InlineNode model, SelectionManager selectionManager)
    {
        return model switch
        {
            TokenItem token => new TokenView(token, selectionManager),
            TypeItem type => new TypeView(type, selectionManager),
            _ => throw new ArgumentException($"Unsupported node type: {model.GetType().Name}")
        };
    }
}