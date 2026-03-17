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

        var doc = XDocument.Parse(text);

        var paragraphs = new List<ParagraphItem>();

        foreach (var p in doc.Descendants("p"))
        {
            var paragraph = ParseParagraph(p);
            paragraphs.Add(paragraph);
        }

        ParagraphsItemsControl.ItemsSource = null;
        ParagraphsItemsControl.ItemsSource = paragraphs;
    }
    
    private ParagraphItem ParseParagraph(XElement pElement)
    {
        var paragraph = new ParagraphItem();
        var currentLine = new LineItem();

        paragraph.Lines.Add(currentLine);

        ParseNodesIntoLines(pElement.Nodes(), paragraph, ref currentLine);

        return paragraph;
    }
    
    private void ParseNodesIntoLines(IEnumerable<XNode> nodes, ParagraphItem paragraph, ref LineItem currentLine, string? entityType = null)
    {
        foreach (var node in nodes)
        {
            if (node is XText textNode)
            {
                AddTextToLines(textNode.Value, paragraph, ref currentLine, entityType);
            }
            else if (node is XElement element)
            {
                if (element.Name.LocalName == "ne")
                {
                    var type = (string?)element.Attribute("type");
                    ParseNodesIntoLines(element.Nodes(), paragraph, ref currentLine, type);
                }
                else
                {
                    ParseNodesIntoLines(element.Nodes(), paragraph, ref currentLine, entityType);
                }
            }
        }
    }
    
    private void AddTextToLines(string text, ParagraphItem paragraph, ref LineItem currentLine, string? entityType)
    {
        var parts = text.Split('\n');

        for (var i = 0; i < parts.Length; i++)
        {
            AddTextAsTokens(parts[i], currentLine.Tokens, entityType);

            if (i < parts.Length - 1)
            {
                currentLine = new LineItem();
                paragraph.Lines.Add(currentLine);
            }
        }
    }
    
    private void AddTextAsTokens(string text, List<TokenItem> tokens, string? entityType)
    {
        var parts = Regex.Matches(text, @"\S+|\s+");

        foreach (Match part in parts)
        {
            if (part.Value.Length == 0)
                continue;
            
            if (part.Value == " ")
                continue;
            
            tokens.Add(new TokenItem
            {
                Text = part.Value,
                Tag = string.IsNullOrWhiteSpace(part.Value) ? null : entityType
            });
        }
    }
    // private void AddTextAsTokens(string text, List<TokenItem> tokens, string? entityType)
    // {
    //     var parts = text.Split();
    //
    //     foreach (var part in parts)
    //     {
    //         if (part.Length == 0)
    //             continue;
    //
    //         tokens.Add(new TokenItem
    //         {
    //             Text = part,
    //         });
    //     }
    // }

    private async Task<string?> OpenFile()
    {
        var topLevel = GetTopLevel(this);
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