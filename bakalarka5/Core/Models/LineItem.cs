using System.Collections.Generic;

namespace bakalarka5.Core.Models;

public class LineItem:InlineContainerNode
{
    public List<TokenItem> Tokens { get; set; } = [];
}
