using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using bakalarka5.Core.Models;
using bakalarka5.Core.Views;

namespace bakalarka5;

public partial class MainWindow : Window
{
    private Document? _document;
    private DocumentView? _documentView;

    public MainWindow()
    {
        InitializeComponent();
        OpenLastDocument();
    }

    private Document Document
    {
        get => _document ?? throw new InvalidOperationException("Document has not been loaded yet.");
        set => _document = value;
    }

    private async void FileOpenMenuItem(object? sender, RoutedEventArgs e)
    {
        var document = await Document.OpenDocument(this);

        if (document is null)
            return;

        Document = document;

        if (Document.FilePath is not null)
            AppState.SaveLastFile(Document.FilePath);

        _documentView = new DocumentView(Document);
        ParagraphsItemsControl.ItemsSource = _documentView.Paragraphs;
    }

    private void InlineNode_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border)
            return;

        if (border.DataContext is not InlineNodeView node)
            return;

        node.IsSelected = !node.IsSelected;

        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            NeContextMenu.OpenMenu(border, node);
        }
    }

    private async void OpenLastDocument()
    {
        var path = AppState.LoadLastFile();

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return;

        Document = await Document.OpenDocument(path);

        _documentView = new DocumentView(Document);
        ParagraphsItemsControl.ItemsSource = _documentView.Paragraphs;
    }
}