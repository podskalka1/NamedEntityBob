using System;
using System.Collections.ObjectModel;

namespace bakalarka5.Core.Models;

public abstract class InlineContainerNode: InlineNode
{
    public ObservableCollection<InlineNode> Children { get; } = new();

    public void AddChild(InlineNode child)
    {
        if (child.Parent is not null)
            throw new InvalidOperationException("Node already has a parent.");

        Children.Add(child);
        child.Parent = this;
    }

    public void InsertChild(int index, InlineNode child)
    {
        if (child.Parent is not null)
            throw new InvalidOperationException("Node already has a parent.");

        Children.Insert(index, child);
        child.Parent = this;
    }

    public void RemoveChild(InlineNode child)
    {
        if (!Children.Remove(child))
            throw new InvalidOperationException("Child was not found in this container.");

        child.Parent = null;
    }
}