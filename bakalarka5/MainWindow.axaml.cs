using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using bakalarka5.Core.Models;

namespace bakalarka5;

public partial class MainWindow : Window
{

    public MainWindow()
    {
        InitializeComponent();
    }
    
    private Document _document;

    private Document Document
    {
        get { return _document; }
        set => _document = value;
    }
    
    private async void FileOpenMenuItem(object? sender, RoutedEventArgs e)
    {
        Document = await Document.OpenDocument(this);

        ParagraphsItemsControl.ItemsSource = Document.Paragraphs;
    }

    private void Token_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border)
            return;

        if (border.DataContext is not TokenItem token)
            return;
        
        token.IsSelected = !token.IsSelected;

        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed == true)
        {
            var menu = BuildTokenContextMenu(token);
            border.ContextMenu = menu;
            menu.Open(border);
            token.IsSelected = !token.IsSelected;
        }
    }
    private ContextMenu BuildTokenContextMenu(TokenItem token)
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
                token.Tag = tag;
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
            token.Tag = null;
        };

        items.Add(clearItem);

        return new ContextMenu
        {
            ItemsSource = items
        };
    }
    
}