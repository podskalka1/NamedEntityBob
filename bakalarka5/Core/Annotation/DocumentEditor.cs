using System;
using System.Collections.Generic;
using System.Linq;
using bakalarka5.Core.DocumentModel;
using bakalarka5.Core.Selection;

namespace bakalarka5.Core.Annotation;

public class DocumentEditor
{
    private readonly SelectionManager _selectionManager;

    public Document Document { get; }

    public Action? DocumentChanged { get; set; }

    public DocumentEditor(Document document, SelectionManager selectionManager)
    {
        Document = document;
        _selectionManager = selectionManager;
    }

    public void ApplyTypeToSelection(string? type)
    {
        var selection = _selectionManager.Current;
        if (selection is null)
            return;

        var selectedNodes = GetSelectedNodes(selection);
        if (selectedNodes.Count == 0)
            return;

        if (selectedNodes.Count == 1 && selectedNodes[0] is TypeItem selectedTypeItem)
        {
            ApplyTypeToTypeItem(selectedTypeItem, type);
        }
        else
        {
            ApplyTypeToNodeRange(selection.Parent, selectedNodes, type);
        }

        _selectionManager.Clear();
        DocumentChanged?.Invoke();
    }

    private void ApplyTypeToTypeItem(TypeItem typeItem, string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            UnwrapTypeItem(typeItem);
        }
        else
        {
            typeItem.Type = type;
        }
    }

    private void ApplyTypeToNodeRange(InlineContainerNode parent, List<InlineNode> nodes, string? type)
    {
        if (string.IsNullOrWhiteSpace(type))
            return;

        int startIndex = parent.Children.IndexOf(nodes[0]);
        if (startIndex < 0)
            return;

        foreach (var node in nodes)
        {
            parent.RemoveChild(node);
        }

        var typeItem = new TypeItem(type);
        parent.InsertChild(startIndex, typeItem);

        foreach (var node in nodes)
        {
            typeItem.AddChild(node);
        }
    }

    private void UnwrapTypeItem(TypeItem typeItem)
    {
        var parent = typeItem.Parent;
        if (parent is null)
            return;

        int index = parent.Children.IndexOf(typeItem);
        if (index < 0)
            return;

        var children = typeItem.Children.ToList();

        parent.RemoveChild(typeItem);

        for (int i = 0; i < children.Count; i++)
        {
            typeItem.RemoveChild(children[i]);
            parent.InsertChild(index + i, children[i]);
        }
    }

    private static List<InlineNode> GetSelectedNodes(SelectionRange selection)
    {
        var result = new List<InlineNode>();

        for (int i = selection.StartIndex; i <= selection.EndIndex; i++)
        {
            result.Add(selection.Parent.Children[i]);
        }

        return result;
    }
    
    public bool CanDeleteSelection()
    {
        var selection = _selectionManager.Current;
        return selection is not null
               && selection.StartIndex >= 0
               && selection.EndIndex < selection.Parent.Children.Count
               && selection.StartIndex <= selection.EndIndex;
    }

    public void DeleteSelection()
    {
        var selection = _selectionManager.Current;
        if (selection is null)
            return;

        for (int i = selection.EndIndex; i >= selection.StartIndex; i--)
        {
            selection.Parent.RemoveChild(selection.Parent.Children[i]);
        }

        _selectionManager.Clear();
        DocumentChanged?.Invoke();
    }

    public bool CanDeleteCurrentLine()
    {
        var selection = _selectionManager.Current;
        if (selection is null)
            return false;

        var line = FindParentLine(selection.Parent);

        return line is not null && line.Children.Count > 0;
    }

    public void DeleteCurrentLine()
    {
        var selection = _selectionManager.Current;
        if (selection is null)
            return;

        var line = FindParentLine(selection.Parent);
        if (line is null || line.Children.Count == 0)
            return;

        _selectionManager.SelectSingle(line.Children[0]);
        _selectionManager.ExtendSelectionTo(line.Children[^1]);

        DeleteSelection();

        if (line.Children.Count != 0 || line.Paragraph is null) return;
        var paragraph = line.Paragraph;

        paragraph.Lines.Remove(line);
        line.Paragraph = null;

        if (paragraph.Lines.Count == 0 && paragraph.Document is not null)
        {
            var document = paragraph.Document;
            document.Paragraphs.Remove(paragraph);
            paragraph.Document = null;
        }

        DocumentChanged?.Invoke();
    }

    private static LineItem? FindParentLine(InlineNode? node)
    {
        while (node is not null)
        {
            if (node is LineItem line)
                return line;

            node = node.Parent;
        }

        return null;
    }
}