using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using bakalarka5.Core.Curation;
using bakalarka5.Core.DocumentModel;
using bakalarka5.UI.CurationUI;

namespace bakalarka5.UI.Windows;

public partial class CurationWindow : Window
{
    private readonly CurationSession _session;
    private readonly ObservableCollection<CurationLineRowView> _rows = new();

    private CurationConflict? _lastSelectedConflict;

    public CurationWindow(Document documentA, Document documentB)
    {
        InitializeComponent();

        _session = new CurationSession(documentA, documentB);

        foreach (var row in BuildDocumentRows())
            _rows.Add(row);

        DocumentRowsItemsControl.ItemsSource = _rows;

        RefreshConflictList();
        ShowCurrentConflict();
    }

    private void RefreshConflictList()
    {
        HeaderText.Text = $"Conflicts: {_session.Conflicts.Count}";

        var items = _session.Conflicts
            .OrderBy(conflict => conflict.IsResolved)
            .ThenBy(conflict => _session.Conflicts.IndexOf(conflict))
            .Select((conflict, index) => new CurationConflictListItem(conflict, index))
            .ToList();

        ConflictListBox.ItemsSource = items;

        if (_session.CurrentConflict is not null)
        {
            ConflictListBox.SelectedItem = items.FirstOrDefault(item =>
                ReferenceEquals(item.Conflict, _session.CurrentConflict));
        }
    }

    private CurationLineRowView[] BuildDocumentRows()
    {
        return new CurationDocumentView(
                _session.LineAlignments,
                _session.Conflicts,
                _session.CurrentConflict)
            .Rows
            .ToArray();
    }

    private CurationLineRowView BuildDocumentRow(int index)
    {
        return new CurationLineRowView(
            _session.LineAlignments[index],
            _session.Conflicts,
            _session.CurrentConflict);
    }

    private void RefreshAllRows()
    {
        _rows.Clear();

        foreach (var row in BuildDocumentRows())
            _rows.Add(row);
    }

    private void ShowCurrentConflict()
    {
        var conflict = _session.CurrentConflict;

        if (conflict is null)
        {
            ConflictKindText.Text = "No conflicts";
            ResolutionText.Text = "";
            return;
        }

        ConflictKindText.Text = conflict.Kind.ToString();
        ResolutionText.Text = conflict.Resolution.ToString();

        RefreshRowsForSelectionChange(_lastSelectedConflict, conflict);
        _lastSelectedConflict = conflict;
        SelectCurrentConflictListItem();
    }

    private void RefreshRowsForSelectionChange(
        CurationConflict? previousConflict,
        CurationConflict? currentConflict)
    {
        var previousIndex = previousConflict is null ? -1 : FindRowIndex(previousConflict);
        var currentIndex = currentConflict is null ? -1 : FindRowIndex(currentConflict);

        ReplaceRow(previousIndex);

        if (currentIndex != previousIndex)
            ReplaceRow(currentIndex);
    }

    private void ReplaceRowForConflict(CurationConflict? conflict)
    {
        if (conflict is null)
            return;

        ReplaceRow(FindRowIndex(conflict));
    }

    private void ReplaceRow(int index)
    {
        if (index < 0 || index >= _session.LineAlignments.Count)
            return;

        var replacement = BuildDocumentRow(index);
        var visibleIndex = FindVisibleRowIndex(index);
        var isDisplayable = IsDisplayable(replacement);

        if (visibleIndex >= 0)
        {
            if (isDisplayable)
                _rows[visibleIndex] = replacement;
            else
                _rows.RemoveAt(visibleIndex);

            return;
        }

        if (!isDisplayable)
            return;

        _rows.Insert(FindVisibleInsertIndex(index), replacement);
    }

    private int FindRowIndex(CurationConflict conflict)
    {
        for (var index = 0; index < _session.LineAlignments.Count; index++)
        {
            if (CurationConflictLocator.BelongsToLine(conflict, _session.LineAlignments[index]))
                return index;
        }

        return -1;
    }

    private int FindVisibleRowIndex(int alignmentIndex)
    {
        var alignment = _session.LineAlignments[alignmentIndex];

        for (var rowIndex = 0; rowIndex < _rows.Count; rowIndex++)
        {
            if (ReferenceEquals(_rows[rowIndex].Alignment, alignment))
                return rowIndex;
        }

        return -1;
    }

    private int FindVisibleInsertIndex(int alignmentIndex)
    {
        for (var rowIndex = 0; rowIndex < _rows.Count; rowIndex++)
        {
            var rowAlignmentIndex = _session.LineAlignments.IndexOf(_rows[rowIndex].Alignment);
            if (rowAlignmentIndex > alignmentIndex)
                return rowIndex;
        }

        return _rows.Count;
    }

    private static bool IsDisplayable(CurationLineRowView row)
    {
        return row.HasConflict || row.Variants.Any(variant => variant.Line.Tokens.Count > 0);
    }

    private void SelectCurrentConflictListItem()
    {
        if (ConflictListBox.ItemsSource is not System.Collections.IEnumerable items)
            return;

        foreach (var item in items)
        {
            if (item is CurationConflictListItem conflictItem &&
                ReferenceEquals(conflictItem.Conflict, _session.CurrentConflict))
            {
                ConflictListBox.SelectedItem = conflictItem;
                return;
            }
        }
    }

    private void ConflictListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ConflictListBox.SelectedItem is not CurationConflictListItem item)
            return;

        var index = _session.Conflicts.IndexOf(item.Conflict);
        if (index < 0)
            return;

        _session.GoTo(index);
        ShowCurrentConflict();
    }

    private void PreviousButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _session.Previous();
        ShowCurrentConflict();
    }

    private void NextButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _session.Next();
        ShowCurrentConflict();
    }

    private void UseAButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ResolveCurrent(CurationResolutionKind.UseA);
    }

    private void UseBButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ResolveCurrent(CurationResolutionKind.UseB);
    }

    private void UseNeitherButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ResolveCurrent(CurationResolutionKind.UseNeither);
    }

    private void UseAFromContext_OnClick(object? sender, RoutedEventArgs e)
    {
        ResolveFromContext(sender, CurationResolutionKind.UseA);
    }

    private void UseBFromContext_OnClick(object? sender, RoutedEventArgs e)
    {
        ResolveFromContext(sender, CurationResolutionKind.UseB);
    }

    private void UseNeitherFromContext_OnClick(object? sender, RoutedEventArgs e)
    {
        ResolveFromContext(sender, CurationResolutionKind.UseNeither);
    }

    private void CurationToken_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border { DataContext: CurationTokenView { Conflict: not null } tokenView })
            return;

        var index = _session.Conflicts.IndexOf(tokenView.Conflict);
        if (index < 0)
            return;

        _session.GoTo(index);
        ShowCurrentConflict();
    }

    private void ResolveCurrent(CurationResolutionKind resolution)
    {
        _session.ResolveCurrent(resolution);
        _lastSelectedConflict = null;
        RefreshConflictList();
        RefreshAllRows();
        ShowCurrentConflict();
    }

    private void ResolveFromContext(object? sender, CurationResolutionKind resolution)
    {
        var tokenView = GetTokenViewFromContext(sender);
        if (tokenView?.Conflict is null)
            return;

        var index = _session.Conflicts.IndexOf(tokenView.Conflict);
        if (index >= 0)
            _session.GoTo(index);

        ResolveCurrent(resolution);
    }

    private static CurationTokenView? GetTokenViewFromContext(object? sender)
    {
        if (sender is not MenuItem menuItem)
            return null;

        if (menuItem.DataContext is CurationTokenView directTokenView)
            return directTokenView;

        if (menuItem.Parent is ContextMenu { PlacementTarget.DataContext: CurationTokenView placementTokenView })
            return placementTokenView;

        return null;
    }
}
