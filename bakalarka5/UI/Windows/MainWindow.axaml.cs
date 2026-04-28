using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using bakalarka5.Core.Annotation;
using bakalarka5.Core.DocumentModel;
using bakalarka5.Core.Persistence;
using bakalarka5.Core.Selection;
using bakalarka5.UI.DocumentUI;
using bakalarka5.UI.Menus;

namespace bakalarka5.UI.Windows;

public partial class MainWindow : Window
{
    private Document? _document;
    private DocumentView? _documentView;
    private DocumentEditor? _documentEditor;

    private readonly SelectionManager _selectionManager = new();
    private readonly SelectionController _selectionController;
    
    private bool _isModified;

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

        var lineCount = 0;
        foreach (var paragraph in _document.Paragraphs)
        {
            foreach (var line in paragraph.Lines)
            {
                lineCount++;
            }
        }
        Console.Out.WriteLine(lineCount);
        
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
        _isModified = false;
        UpdateWindowTitle();
        _documentView = new DocumentView(document, _selectionManager);
        _documentEditor = new DocumentEditor(document, _selectionManager);
        
        _documentEditor.DocumentChanged = () =>
        {
            _isModified = true;
            UpdateWindowTitle();
        };
        
        NeContextMenu.SetEditor(_documentEditor);
        ParagraphsItemsControl.ItemsSource = _documentView.Paragraphs;
    }

    private void SettingsMenuItem(object? sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow();
        settingsWindow.Show(this); // non-modal, owned by MainWindow
    }

    private async void FileSaveMenuItem(object? sender, RoutedEventArgs e)
    {
        if (_document is null)
            return;

        if (_document.FilePath is null)
        {
            await SaveDocumentAs();
            return;
        }

        await _document.Save();
        _isModified = false;
        UpdateWindowTitle();
    }

    private async void FileSaveAsMenuItem(object? sender, RoutedEventArgs e)
    {
        await SaveDocumentAs();
    }

    private async Task SaveDocumentAs()
    {
        if (_document is null)
            return;

        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save document as",
            SuggestedFileName = "document.ne",
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

        var path = file.Path.LocalPath;
        await _document.SaveAs(path);
        _isModified = false;
        UpdateWindowTitle();
        AppState.SaveLastFile(path);
    }
    
    private void UpdateWindowTitle()
    {
        var docTitle = _document?.DocumentTitle ?? "No Document";

        var fileName =
            _document?.FilePath is null
                ? "Unsaved"
                : Path.GetFileName(_document.FilePath);

        var modified = _isModified ? " *" : "";

        Title = $"bakalarka5 - {docTitle} - {fileName}{modified}";
    }
}