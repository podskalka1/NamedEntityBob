using System.Collections.Generic;

namespace bakalarka5.Core.Models;

public class ParagraphItem
{
    public List<TokenItem> Tokens { get; set; } = new();
}