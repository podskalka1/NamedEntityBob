using System.Collections.Generic;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using bakalarka5.Core.Models;
using bakalarka5.Core.Views;

namespace bakalarka5;

public partial class MainWindow : Window
{

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
            var menu = BuildTokenContextMenu(token);
            border.ContextMenu = menu;
            menu.Open(border);
        }
    }
    private ContextMenu BuildTokenContextMenu(TokenView tokenView)
    {
        var items = new List<MenuItem>();

        foreach (var tag in TagSet.TagSetNe)
        {
            var item = new MenuItem
            {
                Header = tag
            };

            item.Click += (_, _) =>
            {
                tokenView.Tag = tag;
                tokenView.IsSelected = false;
            };

            items.Add(item);
        }

        items.Add(new MenuItem { Header = "-" });

        var clearItem = new MenuItem
        {
            Header = "Clear tag"
        };

        clearItem.Click += (_, _) =>
        {
            tokenView.Tag = null;
            tokenView.IsSelected = false;
        };

        items.Add(clearItem);

        return new ContextMenu
        {
            ItemsSource = items
        };
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