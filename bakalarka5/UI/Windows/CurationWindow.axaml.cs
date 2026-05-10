using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using bakalarka5.Core.Curation;
using bakalarka5.Core.DocumentModel;
using bakalarka5.UI.CurationUI;

namespace bakalarka5.UI.Windows;

public partial class CurationWindow : Window
{
    private CurationSession _session;
    private readonly ObservableCollection<CurationLineRowView> _rows = new();

    private CurationConflict? _lastSelectedConflict;
    private string? _savePath;

    public CurationWindow(Document documentA, Document documentB)
    {
        InitializeComponent();

        _session = new CurationSession(documentA, documentB);

        DocumentRowsItemsControl.ItemsSource = _rows;
        LoadSession(documentA, documentB);
    }

    private void LoadSession(Document documentA, Document documentB)
    {
        _session = new CurationSession(documentA, documentB);
        _savePath = null;
        _lastSelectedConflict = null;
        RefreshConflictList();
        RefreshAllRows();
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

    private void UseAFromContext_OnClick(object? sender, RoutedEventArgs e)
    {
        ResolveFromContext(sender, CurationResolutionKind.UseA);
    }

    private void UseBFromContext_OnClick(object? sender, RoutedEventArgs e)
    {
        ResolveFromContext(sender, CurationResolutionKind.UseB);
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

    private async void FileOpenMenuItem(object? sender, RoutedEventArgs e)
    {
        var documents = await OpenCurationDocuments();
        if (documents is null)
            return;

        LoadSession(documents.Value.A, documents.Value.B);
    }

    private async void FileSaveMenuItem(object? sender, RoutedEventArgs e)
    {
        if (_savePath is null)
        {
            await SaveAs();
            return;
        }

        await SaveToPath(_savePath);
    }

    private async void FileSaveAsMenuItem(object? sender, RoutedEventArgs e)
    {
        await SaveAs();
    }

    private async Task SaveAs()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save curated document as",
            SuggestedFileName = "curated.ne",
            FileTypeChoices =
            [
                new FilePickerFileType("Named Entities")
                {
                    Patterns = ["*.ne"]
                },
                FilePickerFileTypes.All
            ]
        });

        if (file is null)
            return;

        _savePath = file.Path.LocalPath;
        await SaveToPath(_savePath);
    }

    private async Task SaveToPath(string path)
    {
        if (!await ResolveRemainingConflictsForSave())
            return;

        await File.WriteAllTextAsync(path, _session.SerializeCuratedDocument());
    }

    private async Task<bool> ResolveRemainingConflictsForSave()
    {
        if (_session.Conflicts.Count == 0)
            return true;

        var dialog = new ResolveRemainingConflictsWindow(_session.Conflicts.Count);
        var resolution = await dialog.ShowDialog<CurationResolutionKind?>(this);

        if (resolution is null)
            return false;

        _session.ResolveAll(resolution.Value);
        _lastSelectedConflict = null;
        RefreshConflictList();
        RefreshAllRows();
        ShowCurrentConflict();

        return true;
    }

    private async Task<(Document A, Document B)?> OpenCurationDocuments()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select two annotation files",
            AllowMultiple = true,
            FileTypeFilter =
            [
                new FilePickerFileType("Named Entities")
                {
                    Patterns = ["*.ne"]
                },
                FilePickerFileTypes.All
            ]
        });

        if (files.Count < 2)
            return null;

        var documentA = await Document.OpenDocument(files[0].Path.LocalPath);
        var documentB = await Document.OpenDocument(files[1].Path.LocalPath);

        return (documentA, documentB);
    }
}
