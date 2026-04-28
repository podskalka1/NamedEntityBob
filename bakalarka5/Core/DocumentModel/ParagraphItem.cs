using System.Collections.ObjectModel;

namespace bakalarka5.Core.DocumentModel;

public class ParagraphItem
{
    public Document? Document { get; internal set; }

    public ObservableCollection<LineItem> Lines { get; } = [];
}