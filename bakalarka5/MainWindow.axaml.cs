using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using bakalarka5.Core.Models;
using bakalarka5.Core.Views;

namespace bakalarka5;

public partial class MainWindow : Window
{
    private TokenView? _contextToken;
    
    public MainWindow()
    {
        InitializeComponent();
        OpenLastDocument();
    }
    
    private Document _document;
    private DocumentView? _documentView;

    private Document Document
    {
        get { return _document; }
        set => _document = value;
    }
    
    private async void FileOpenMenuItem(object? sender, RoutedEventArgs e)
    {
        Document = await Document.OpenDocument(this);
        
        if (Document.FilePath is not null)
            AppState.SaveLastFile(Document.FilePath);
        
        _documentView = new DocumentView(Document);
        ParagraphsItemsControl.ItemsSource = _documentView.Paragraphs;
    }

    private void Token_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border)
            return;

        if (border.DataContext is not TokenView token)
            return;
        
        token.IsSelected = !token.IsSelected;
        
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            NeContextMenu.OpenMenu(border, token);
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