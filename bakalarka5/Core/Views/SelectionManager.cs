using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public class SelectionManager : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private SelectionRange? _current;
    public SelectionRange? Current
    {
        get => _current;
        private set
        {
            if (_current == value) return;
            _current = value;
            OnPropertyChanged();
        }
    }

    private InlineNode? _anchorNode;
    public InlineNode? AnchorNode
    {
        get => _anchorNode;
        private set
        {
            if (_anchorNode == value) return;
            _anchorNode = value;
            OnPropertyChanged();
        }
    }

    public void Clear()
    {
        Current = null;
        AnchorNode = null;
    }

    public void SelectSingle(InlineNode node)
    {
        var parent = node.Parent
                     ?? throw new InvalidOperationException("Node has no parent.");

        int index = parent.Children.IndexOf(node);
        Current = new SelectionRange(parent, index, index);
        AnchorNode = node;
    }

    public void ExtendSelectionTo(InlineNode node)
    {
        if (AnchorNode is null)
        {
            SelectSingle(node);
            return;
        }

        var commonParent = FindLowestCommonContainer(AnchorNode, node);

        var startNode = GetDirectChildUnderAncestor(AnchorNode, commonParent);
        var endNode = GetDirectChildUnderAncestor(node, commonParent);

        int startIndex = commonParent.Children.IndexOf(startNode);
        int endIndex = commonParent.Children.IndexOf(endNode);

        Current = new SelectionRange(commonParent, startIndex, endIndex);
    }

    private static InlineNode GetDirectChildUnderAncestor(InlineNode node, InlineContainerNode ancestor)
    {
        var current = node;

        while (current.Parent != ancestor)
        {
            if (current.Parent is null)
                throw new InvalidOperationException("Node is not under the given ancestor.");

            current = current.Parent;
        }

        return current;
    }

    private static InlineContainerNode FindLowestCommonContainer(InlineNode a, InlineNode b)
    {
        var ancestors = new HashSet<InlineContainerNode>();

        var currentA = a.Parent;
        while (currentA is not null)
        {
            ancestors.Add(currentA);
            currentA = currentA.Parent;
        }

        var currentB = b.Parent;
        while (currentB is not null)
        {
            if (ancestors.Contains(currentB))
                return currentB;

            currentB = currentB.Parent;
        }

        throw new InvalidOperationException("Nodes do not share a common container.");
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}