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

    //TODO refactor
    private async void FileOpenMenuItem(object? sender, RoutedEventArgs e)
    {
        Document = await Document.OpenDocument(this);
        if (Document == null)
            return;

        ParagraphsItemsControl.ItemsSource = Document.Paragraphs;
    }

    private void Token_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Border border)
            return;

        if (border.DataContext is not TokenItem token)
            return;

        token.IsSelected = !token.IsSelected;
    }
}