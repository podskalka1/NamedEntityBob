using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
    private List<ParagraphItem> _paragraphs = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private async void FileOpenMenuItem(object? sender, RoutedEventArgs e)
    {
        var text = await OpenFile();

        if (string.IsNullOrWhiteSpace(text))
            return;

        XDocument doc = XDocument.Parse(text);

        _paragraphs = new List<ParagraphItem>();

        foreach (var p in doc.Descendants("p"))
        {
            var paragraph = new ParagraphItem();
            ParseNodesIntoTokens(p.Nodes(), paragraph.Tokens);
            _paragraphs.Add(paragraph);
        }

        ParagraphsItemsControl.ItemsSource = null;
        ParagraphsItemsControl.ItemsSource = _paragraphs;
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
                if (element.Name.LocalName == "ne")
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

    private void AddTextAsTokens(string text, List<TokenItem> tokens, string? entityType)
    {
        var parts = Regex.Matches(text, @"\S+|\s+");

        foreach (Match part in parts)
        {
            tokens.Add(new TokenItem
            {
                Text = part.Value,
                Tag = string.IsNullOrWhiteSpace(part.Value) ? null : entityType
            });
        }
    }

    private async Task<string?> OpenFile()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open document",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("Named Entities")
                {
                    Patterns = ["*.ne"]
                },
                FilePickerFileTypes.All
            ]
        });

        var file = files.FirstOrDefault();
        if (file is null)
            return null;

        await using var stream = await file.OpenReadAsync();
        using var reader = new StreamReader(stream);
        var text = await reader.ReadToEndAsync();
        return text;
    }
}