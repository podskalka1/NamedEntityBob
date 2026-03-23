using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using bakalarka5.Core.Models;

namespace bakalarka5;

public class Document
{
    private static MainWindow _mainWindow;
    public List<ParagraphItem> Paragraphs { get; private set; }

    private Document(List<ParagraphItem> paragraphs)
    {
        Paragraphs = paragraphs;
    }

    public static async Task<Document> OpenDocument(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
        
        var text = await OpenFile();

        if (string.IsNullOrWhiteSpace(text))
            return null;

        var xdoc = XDocument.Parse(text);

        var paragraphs = new List<ParagraphItem>();

        foreach (var p in xdoc.Descendants("p"))
        {
            paragraphs.Add(ParseParagraph(p));
        }

        return new Document(paragraphs);
    }
    
    private static async Task<string?> OpenFile()
    {
        var topLevel = TopLevel.GetTopLevel(_mainWindow);
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

    private static ParagraphItem ParseParagraph(XElement pElement)
    {
        var paragraph = new ParagraphItem();
        var currentLine = new LineItem();

        paragraph.Lines.Add(currentLine);

        ParseNodesIntoLines(pElement.Nodes(), paragraph, ref currentLine);

        return paragraph;
    }

    private static void ParseNodesIntoLines(IEnumerable<XNode> nodes, ParagraphItem paragraph, ref LineItem currentLine, string? entityType = null)
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

    private static void AddTextToLines(string text, ParagraphItem paragraph, ref LineItem currentLine, string? entityType)
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

    private static void AddTextAsTokens(string text, List<TokenItem> tokens, string? entityType)
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

    
}