using System.Collections.Generic;

namespace bakalarka5.Core.DocumentModel;

public class LineItem:InlineContainerNode
{
    public List<TokenItem> Tokens { get; set; } = [];
}
