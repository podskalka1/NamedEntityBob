using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using bakalarka5.Core.Models;

namespace bakalarka5;

public partial class MainWindow : Window
{
    private List<TokenItem> _tokens = new();
    
    public MainWindow()
    {
        InitializeComponent();
        
        // _tokens = new List<TokenItem>
        // {
        //     new() { Text = "John" },
        //     new() { Text = "lives" },
        //     new() { Text = "in" },
        //     new() { Text = "Bratislava" }
        // };
        //
        // TokensItemsControl.ItemsSource = _tokens;
    }

    private async void FileOpenMenuItem(object? sender, RoutedEventArgs e)
    {
        var text = await OpenFile();

        if (string.IsNullOrWhiteSpace(text))
            return;

        XDocument doc = XDocument.Parse(text);

        var tokens = new List<TokenItem>();

        foreach (var p in doc.Descendants("p"))
        {
            ParseNodesIntoTokens(p.Nodes(), tokens);
            tokens.Add(new TokenItem { Text = "\n" });
        }

        TokensItemsControl.ItemsSource = tokens;
    }
    
    private async Task<string?> OpenFile()
    {
        // Get the TopLevel for this window/control
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return null;

        // Open native file picker
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open document",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Named Entities")
                {
                    Patterns = ["*.ne"]
                    //there is possibility to add something called MimeTypes which is relevant for Apple devices
                },
                FilePickerFileTypes.All
            ]
        });

        var file = files.FirstOrDefault();
        if (file is null)
            return null; // user cancelled

        // Read the file
        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();
        return text;
    }

    private void TokenButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is TokenItem token)
        {
            token.Tag = token.Tag == "PERSON" ? null : "PERSON";
            RefreshTokens();
        }
        
    }
    private void RefreshTokens()
    {
        TokensItemsControl.ItemsSource = null;
        TokensItemsControl.ItemsSource = _tokens;
    }
    
    private void RenderTokens()
    {
        TokensPanel.Children.Clear();

        foreach (var token in _tokens)
        {
            var button = new Button
            {
                Content = token.Text,
                Margin = new Thickness(2),
                Padding = new Thickness(4, 2),
                Tag = token
            };

            if (token.Tag == "PERSON")
            {
                button.Classes.Add("person");
            }

            button.Click += TokenButton_Click_Manual;
            TokensPanel.Children.Add(button);
        }
    }

    private void TokenButton_Click_Manual(object? sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is TokenItem token)
        {
            token.Tag = token.Tag == "PERSON" ? null : "PERSON";
            RenderTokens();
        }
    }
    
    private void AddTextAsTokens(string text, List<TokenItem> tokens, string? entityType)
    {
        var parts = System.Text.RegularExpressions.Regex.Matches(text, @"\S+|\s+");

        foreach (System.Text.RegularExpressions.Match part in parts)
        {
            tokens.Add(new TokenItem
            {
                Text = part.Value,
                Tag = string.IsNullOrWhiteSpace(part.Value) ? null : entityType
            });
        }
    }
    private void ParseNodesIntoTokens(IEnumerable<XNode> nodes, List<TokenItem> tokens, string? entityType = null)
    {
        foreach (var node in nodes)
        {
            if (node is XText textNode)
            {
                AddTextAsTokens(textNode.Value, tokens, entityType);
            }
            else if (node is XElement element)
            {
                if (element.Name == "ne")
                {
                    var type = (string?)element.Attribute("type");
                    ParseNodesIntoTokens(element.Nodes(), tokens, type);
                }
                else
                {
                    ParseNodesIntoTokens(element.Nodes(), tokens, entityType);
                }
            }
        }
    }
}