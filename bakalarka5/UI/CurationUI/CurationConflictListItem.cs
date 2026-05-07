using Avalonia.Media;
using bakalarka5.Core.Curation;

namespace bakalarka5.UI.CurationUI;

public class CurationConflictListItem
{
    public int Index { get; }
    public CurationConflict Conflict { get; }

    public string Label
    {
        get
        {
            var status = Conflict.IsResolved ? "Resolved" : "Open";
            return $"{Index + 1}. {Conflict.Kind} - {status}";
        }
    }

    public IBrush Background =>
        Conflict.IsResolved
            ? new SolidColorBrush(Color.FromRgb(207, 232, 211))
            : Brushes.Transparent;

    public CurationConflictListItem(CurationConflict conflict, int index)
    {
        Conflict = conflict;
        Index = index;
    }
}
