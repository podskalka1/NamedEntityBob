using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using bakalarka5.Core.Annotation;
using bakalarka5.UI.Windows;

namespace bakalarka5.Core.DocumentModel;

public class Document
{
    private readonly XElement _rootTemplate;

    public string? FilePath { get; private set; }

    public ObservableCollection<ParagraphItem> Paragraphs { get; private set; }
    
    public string DocumentTitle =>
        (string?)_rootTemplate.Attribute("title") ?? "Untitled";

    private Document(
        ObservableCollection<ParagraphItem> paragraphs,
        XElement rootTemplate,
        string? filePath = null)
    {
        Paragraphs = paragraphs;
        FilePath = filePath;
        _rootTemplate = rootTemplate;

        foreach (var paragraph in Paragraphs)
        {
            paragraph.Document = this;
        }
    }

    public static async Task<Document?> OpenDocument(MainWindow mainWindow)
    {
        var fileResult = await OpenFile(mainWindow);

        if (fileResult is null)
            return null;

        var (path, text) = fileResult.Value;
        return Parse(text, path);
    }

    public static async Task<Document> OpenDocument(string path)
    {
        var text = await File.ReadAllTextAsync(path);
        return Parse(text, path);
    }

    public async Task Save()
    {
        if (FilePath is null)
            throw new InvalidOperationException("Document has no file path.");

        await SaveAs(FilePath);
    }

    public async Task SaveAs(string path)
    {
        await File.WriteAllTextAsync(path, SerializeDocument());
        FilePath = path;
    }

    private string SerializeDocument()
    {
        var builder = new StringBuilder();

        builder.AppendLine(GetRootOpeningTag());

        foreach (var paragraph in Paragraphs)
        {
            SerializeParagraph(builder, paragraph);
        }

        builder.AppendLine($"</{_rootTemplate.Name.LocalName}>");

        return builder.ToString();
    }

    private string GetRootOpeningTag()
    {
        var root = new XElement(_rootTemplate);
        root.RemoveNodes();

        var text = root.ToString(SaveOptions.DisableFormatting);

        if (text.EndsWith(" />"))
            return text[..^3] + ">";

        if (text.EndsWith("/>"))
            return text[..^2] + ">";

        return text;
    }

    private static void SerializeParagraph(StringBuilder builder, ParagraphItem paragraph)
    {
        builder.AppendLine("<p>");

        foreach (var line in paragraph.Lines)
        {
            var serializedLine = SerializeContainer(line);

            if (string.IsNullOrWhiteSpace(serializedLine))
                continue;

            builder.AppendLine(serializedLine);
        }

        builder.AppendLine("</p>");
    }

    private static string SerializeContainer(InlineContainerNode container)
    {
        return string.Join(" ", container.Children.Select(SerializeNode));
    }

    private static string SerializeNode(InlineNode node)
    {
        if (node is TokenItem token)
            return EscapeXmlText(token.Text);

        if (node is TypeItem typeItem)
        {
            var type = EscapeXmlAttribute(typeItem.Type ?? "");
            var content = SerializeContainer(typeItem);

            return $"""<ne type="{type}">{content}</ne>""";
        }

        throw new InvalidOperationException($"Unknown inline node type: {node.GetType().Name}");
    }

    private static string EscapeXmlText(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    private static string EscapeXmlAttribute(string text)
    {
        return EscapeXmlText(text)
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    private static Document Parse(string text, string? filePath = null)
    {
        var xDocument = XDocument.Parse(text);

        if (xDocument.Root is null)
            throw new InvalidOperationException("Document has no root element.");

        var rootTemplate = new XElement(xDocument.Root);
        rootTemplate.RemoveNodes();

        var paragraphs = new ObservableCollection<ParagraphItem>();

        foreach (var p in xDocument.Descendants("p"))
        {
            paragraphs.Add(ParseParagraph(p));
        }

        return new Document(paragraphs, rootTemplate, filePath);
    }

    private static async Task<(string path, string text)?> OpenFile(MainWindow mainWindow)
    {
        var topLevel = TopLevel.GetTopLevel(mainWindow);
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

        return (file.Path.LocalPath, text);
    }

    private static ParagraphItem ParseParagraph(XElement pElement)
    {
        var paragraph = new ParagraphItem();
        var currentLine = new LineItem();

        paragraph.Lines.Add(currentLine);

        ParseNodesIntoLines(pElement.Nodes(), paragraph, ref currentLine, currentLine);

        foreach (var line in paragraph.Lines)
        {
            line.Paragraph = paragraph;
        }

        return paragraph;
    }

    private static void ParseNodesIntoLines(
        IEnumerable<XNode> nodes,
        ParagraphItem paragraph,
        ref LineItem currentLine,
        InlineContainerNode currentContainer)
    {
        foreach (var node in nodes)
        {
            if (node is XText textNode)
            {
                AddTextToLines(textNode.Value, paragraph, ref currentLine, ref currentContainer);
            }
            else if (node is XElement element)
            {
                if (element.Name.LocalName == "ne")
                {
                    var type = (string?)element.Attribute("type");
                    var typeItem = new TypeItem(type);
                    currentContainer.AddChild(typeItem);

                    ParseNodesIntoLines(element.Nodes(), paragraph, ref currentLine, typeItem);
                }
                else
                {
                    ParseNodesIntoLines(element.Nodes(), paragraph, ref currentLine, currentContainer);
                }
            }
        }
    }

    private static void AddTextToLines(
        string text,
        ParagraphItem paragraph,
        ref LineItem currentLine,
        ref InlineContainerNode currentContainer)
    {
        var parts = text.Split('\n');

        for (var i = 0; i < parts.Length; i++)
        {
            AddTextAsTokens(parts[i], currentContainer);

            if (i < parts.Length - 1)
            {
                currentLine = new LineItem();
                paragraph.Lines.Add(currentLine);
                currentContainer = currentLine;
            }
        }
    }

    private static void AddTextAsTokens(string text, InlineContainerNode container)
    {
        var parts = Regex.Matches(text, @"\S+|\s+");

        foreach (Match part in parts)
        {
            if (part.Value.Length == 0)
                continue;

            if (part.Value == " ")
                continue;

            container.AddChild(new TokenItem
            {
                Text = part.Value
            });
        }
    }
    
    public static async Task<Document> LoadFromPath(string path)
    {
        var text = await File.ReadAllTextAsync(path);
        return Parse(text, path);
    }
}