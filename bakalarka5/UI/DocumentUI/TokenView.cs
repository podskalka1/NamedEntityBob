using bakalarka5.Core.DocumentModel;
using bakalarka5.Core.Selection;

namespace bakalarka5.UI.DocumentUI;

public class TokenView : InlineNodeView
{
    public TokenItem TokenModel => (TokenItem)Model;

    public string Text => TokenModel.Text;

    public TokenView(TokenItem model, SelectionManager selectionManager)
        : base(model, selectionManager)
    {
    }
}