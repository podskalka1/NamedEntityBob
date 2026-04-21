using System;
using bakalarka5.Core.DocumentModel;

namespace bakalarka5.Core.Selection;

public class SelectionRange
{
    public InlineContainerNode Parent { get; }
    public int StartIndex { get; }
    public int EndIndex { get; }

    public SelectionRange(InlineContainerNode parent, int startIndex, int endIndex)
    {
        Parent = parent;
        StartIndex = Math.Min(startIndex, endIndex);
        EndIndex = Math.Max(startIndex, endIndex);
    }

    public bool Contains(InlineNode node)
    {
        if (node.Parent != Parent)
            return false;

        int index = Parent.Children.IndexOf(node);
        return index >= StartIndex && index <= EndIndex;
    }
}