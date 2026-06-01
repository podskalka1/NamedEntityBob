using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using bakalarka5.Core.DocumentModel;

namespace bakalarka5.UI.Windows;

public static class CurationDocumentPicker
{
    public static async Task<(Document A, Document B)?> PickDocuments(Window owner)
    {
        var topLevel = TopLevel.GetTopLevel(owner);
        if (topLevel is null)
            return null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select two annotation files",
            AllowMultiple = true,
            FileTypeFilter = AnnotationFileTypes()
        });

        if (files.Count == 0)
            return null;

        var pathA = files[0].Path.LocalPath;
        var pathB = files.Count >= 2
            ? files[1].Path.LocalPath
            : await PickSecondDocumentPath(topLevel);

        if (pathB is null)
            return null;

        var documentA = await Document.OpenDocument(pathA);
        var documentB = await Document.OpenDocument(pathB);

        return (documentA, documentB);
    }

    private static async Task<string?> PickSecondDocumentPath(TopLevel topLevel)
    {
        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select second annotation file",
            AllowMultiple = false,
            FileTypeFilter = AnnotationFileTypes()
        });

        return files.Count == 0
            ? null
            : files[0].Path.LocalPath;
    }

    private static FilePickerFileType[] AnnotationFileTypes()
    {
        return
        [
            new FilePickerFileType("Named Entities")
            {
                Patterns = ["*.ne"]
            },
            FilePickerFileTypes.All
        ];
    }
}
