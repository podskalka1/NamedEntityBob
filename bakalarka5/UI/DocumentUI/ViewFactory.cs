using System;
using bakalarka5.Core.Annotation;
using bakalarka5.Core.DocumentModel;
using bakalarka5.Core.Selection;

namespace bakalarka5.UI.DocumentUI;

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