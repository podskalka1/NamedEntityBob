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

    private readonly SelectionManager _selectionManager = new();
    private readonly SelectionController _selectionController;
    private DocumentEditor? _documentEditor;

    public MainWindow()
    {
        InitializeComponent();
        _selectionController = new SelectionController(_selectionManager);
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

        _documentView = new DocumentView(Document, _selectionManager);
        _documentEditor = new DocumentEditor(Document, _selectionManager);
        _documentEditor.DocumentChanged = RefreshDocumentView;
        NeContextMenu.SetEditor(_documentEditor);

        ParagraphsItemsControl.ItemsSource = _documentView.Paragraphs;
    }

    private void InlineNode_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border)
            return;

        if (border.DataContext is not InlineNodeView node)
            return;

        _selectionController.HandlePointerPressed(border, node, e);
    }

    private async void OpenLastDocument()
    {
        var path = AppState.LoadLastFile();

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return;

        Document = await Document.OpenDocument(path);

        _documentView = new DocumentView(Document, _selectionManager);
        _documentEditor = new DocumentEditor(Document, _selectionManager);
        _documentEditor.DocumentChanged = RefreshDocumentView;
        NeContextMenu.SetEditor(_documentEditor);

        ParagraphsItemsControl.ItemsSource = _documentView.Paragraphs;
    }

    private void RefreshDocumentView()
    {
        if (_document is null)
            return;

        _selectionManager.Clear();
        _documentView = new DocumentView(_document, _selectionManager);
        _documentEditor = new DocumentEditor(_document, _selectionManager);
        _documentEditor.DocumentChanged = RefreshDocumentView;
        NeContextMenu.SetEditor(_documentEditor);

        ParagraphsItemsControl.ItemsSource = null;
        ParagraphsItemsControl.ItemsSource = _documentView.Paragraphs;
    }
}