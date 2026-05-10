using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bakalarka5.Core.Curation;

public static class CurationDocumentSerializer
{
    public static string Serialize(CurationDocument document, string title = "Curated")
    {
        var builder = new StringBuilder();

        builder.AppendLine($"""<doc title="{EscapeXmlAttribute(title)}">""");
        builder.AppendLine("<p>");

        foreach (var line in document.Lines)
        {
            if (line.Tokens.Count == 0)
                continue;

            builder.AppendLine(SerializeLine(line));
        }

        builder.AppendLine("</p>");
        builder.AppendLine("</doc>");

        return builder.ToString();
    }

    private static string SerializeLine(CurationLine line)
    {
        var builder = new StringBuilder();
        var open = new List<CurationAnnotationLayer>();
        var wroteToken = false;

        foreach (var token in line.Tokens)
        {
            var commonDepth = CommonPrefixLength(open, token.AnnotationPath);

            for (var index = open.Count - 1; index >= commonDepth; index--)
                builder.Append("</ne>");

            open.RemoveRange(commonDepth, open.Count - commonDepth);

            for (var index = commonDepth; index < token.AnnotationPath.Count; index++)
            {
                var layer = token.AnnotationPath[index];
                builder.Append($"""<ne type="{EscapeXmlAttribute(layer.Type)}">""");
                open.Add(layer);
            }

            if (wroteToken)
                builder.Append(' ');

            builder.Append(EscapeXmlText(token.Text));
            wroteToken = true;
        }

        for (var index = open.Count - 1; index >= 0; index--)
            builder.Append("</ne>");

        return builder.ToString();
    }

    private static int CommonPrefixLength(
        IReadOnlyList<CurationAnnotationLayer> a,
        IReadOnlyList<CurationAnnotationLayer> b)
    {
        var length = 0;

        while (length < a.Count && length < b.Count &&
               a[length].Type == b[length].Type &&
               a[length].InstanceId == b[length].InstanceId)
        {
            length++;
        }

        return length;
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
}
