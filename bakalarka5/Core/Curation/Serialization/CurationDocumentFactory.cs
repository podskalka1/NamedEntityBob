using System;
using System.Collections.Generic;
using System.Linq;
using bakalarka5.Core.Annotation;
using bakalarka5.Core.DocumentModel;

namespace bakalarka5.Core.Curation;

public static class CurationDocumentFactory
{
    public static CurationDocument FromDocument(Document document, AnnotatorSide side)
    {
        var result = new CurationDocument();

        int lineIndex = 0;
        int typeItemId = 0;

        foreach (var paragraph in document.Paragraphs)
        {
            foreach (var line in paragraph.Lines)
            {
                var cLine = new CurationLine
                {
                    OriginalIndex = lineIndex++
                };

                int tokenIndex = 0;
                var path = new List<CurationAnnotationLayer>();

                foreach (var node in line.Children)
                {
                    ExtractTokens(node, cLine, ref tokenIndex, path, ref typeItemId);
                }

                result.Lines.Add(cLine);
            }
        }

        return result;
    }

    private static void ExtractTokens(
        InlineNode node,
        CurationLine line,
        ref int tokenIndex,
        List<CurationAnnotationLayer> annotationPath,
        ref int typeItemId)
    {
        switch (node)
        {
            case TokenItem token:
                line.Tokens.Add(new CurationToken
                {
                    Text = token.Text,
                    OriginalLineIndex = line.OriginalIndex,
                    OriginalTokenIndex = tokenIndex++,
                    AnnotationPath = annotationPath
                        .Select(layer => new CurationAnnotationLayer
                        {
                            Type = layer.Type,
                            InstanceId = layer.InstanceId
                        })
                        .ToList()
                });
                break;

            case TypeItem typeItem:
                annotationPath.Add(new CurationAnnotationLayer
                {
                    Type = typeItem.Type,
                    InstanceId = typeItemId++
                });

                foreach (var child in typeItem.Children)
                {
                    ExtractTokens(child, line, ref tokenIndex, annotationPath, ref typeItemId);
                }

                annotationPath.RemoveAt(annotationPath.Count - 1);
                break;

            default:
                throw new NotSupportedException($"Unknown InlineNode: {node.GetType().Name}");
        }
    }
}