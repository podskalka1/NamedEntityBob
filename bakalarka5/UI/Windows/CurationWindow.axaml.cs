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

    public CurationWindow(Document documentA, Document documentB)
    {
        InitializeComponent();

        _session = new CurationSession(documentA, documentB);

        RefreshConflictList();
        RefreshDocumentViews();
        ShowCurrentConflict();
    }

    private void RefreshConflictList()
    {
        HeaderText.Text = $"Conflicts: {_session.Conflicts.Count}";

        ConflictListBox.ItemsSource = _session.Conflicts
            .Select((conflict, index) => new CurationConflictListItem(conflict, index))
            .ToList();

        if (_session.Conflicts.Count > 0)
            ConflictListBox.SelectedIndex = _session.CurrentConflictIndex;
    }

    private void RefreshDocumentViews()
    {
        DocumentAItemsControl.ItemsSource = new CurationDocumentView(
            _session.DocumentA,
            AnnotatorSide.A,
            _session.Conflicts,
            _session.CurrentConflict).Lines;

        DocumentBItemsControl.ItemsSource = new CurationDocumentView(
            _session.DocumentB,
            AnnotatorSide.B,
            _session.Conflicts,
            _session.CurrentConflict).Lines;
    }

    private void ShowCurrentConflict()
    {
        var conflict = _session.CurrentConflict;

        if (conflict is null)
        {
            ConflictKindText.Text = "No conflicts";
            ResolutionText.Text = "";
            RefreshDocumentViews();
            return;
        }

        ConflictKindText.Text = conflict.Kind.ToString();
        ResolutionText.Text = conflict.Resolution.ToString();

        ConflictListBox.SelectedIndex = _session.CurrentConflictIndex;
        RefreshDocumentViews();
    }

    private void ConflictListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ConflictListBox.SelectedIndex < 0)
            return;

        _session.GoTo(ConflictListBox.SelectedIndex);
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
        RefreshConflictList();
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
