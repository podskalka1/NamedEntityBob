namespace bakalarka5.Core.DocumentModel;

public abstract class InlineNode
{
    public InlineContainerNode? Parent { get; internal set; }
}