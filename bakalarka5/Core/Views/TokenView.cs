using System.ComponentModel;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public class TokenView : InlineNodeView
{
    public TokenItem TokenModel => (TokenItem)Model;

    public string Text => TokenModel.Text;

    public TokenView(TokenItem model) : base(model)
    {
    }
}