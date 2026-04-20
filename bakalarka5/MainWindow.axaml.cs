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
    private DocumentEditor? _documentEditor;

    private readonly SelectionManager _selectionManager = new();
    private readonly SelectionController _selectionController;

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

        if (document.FilePath is not null)
            AppState.SaveLastFile(document.FilePath);

        LoadDocumentIntoView(document);
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

        var document = await Document.OpenDocument(path);
        LoadDocumentIntoView(document);
    }

    private void LoadDocumentIntoView(Document document)
    {
        _selectionManager.Clear();

        Document = document;
        _documentView = new DocumentView(document, _selectionManager);
        _documentEditor = new DocumentEditor(document, _selectionManager);

        NeContextMenu.SetEditor(_documentEditor);
        ParagraphsItemsControl.ItemsSource = _documentView.Paragraphs;
    }
}