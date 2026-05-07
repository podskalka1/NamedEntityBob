using System.Linq;
using Avalonia;
using Avalonia.Media;
using bakalarka5.Core.Curation;

namespace bakalarka5.UI.CurationUI;

public class CurationTokenView
{
    public CurationToken Token { get; }
    public AnnotatorSide Side { get; }
    public CurationConflict? Conflict { get; }

    private readonly CurationConflict? _currentConflict;

    public string Text => Token.Text;

    public string AnnotationText =>
        Token.AnnotationPath.Count == 0
            ? ""
            : string.Join("/", Token.AnnotationPath.Select(layer => layer.Type));

    public bool HasAnnotation => AnnotationText.Length > 0;
    public bool HasConflict => Conflict is not null;
    public bool IsCurrentConflict => Conflict is not null && ReferenceEquals(Conflict, _currentConflict);

    public IBrush Background
    {
        get
        {
            if (Conflict is null)
                return AnnotationBrush;

            if (Conflict.IsResolved)
                return new SolidColorBrush(Color.FromRgb(207, 232, 211));

            return Side == AnnotatorSide.A
                ? new SolidColorBrush(Color.FromRgb(255, 236, 166))
                : new SolidColorBrush(Color.FromRgb(255, 213, 213));
        }
    }

    public IBrush BorderBrush =>
        IsCurrentConflict
            ? new SolidColorBrush(Color.FromRgb(45, 92, 180))
            : HasConflict
                ? new SolidColorBrush(Color.FromRgb(130, 95, 30))
                : Brushes.Transparent;

    public Thickness BorderThickness => HasConflict ? new Thickness(IsCurrentConflict ? 2 : 1) : new Thickness(0);

    public string ToolTip
    {
        get
        {
            if (Conflict is null)
                return AnnotationText;

            var resolution = Conflict.IsResolved ? Conflict.Resolution.ToString() : "Unresolved";
            return $"{Conflict.Kind} ({resolution})";
        }
    }

    private IBrush AnnotationBrush
    {
        get
        {
            if (Token.AnnotationPath.Count == 0)
                return Brushes.Transparent;

            var type = Token.AnnotationPath[^1].Type;
            if (string.IsNullOrEmpty(type))
                return Brushes.Transparent;

            return new SolidColorBrush(DetermineColor(type[0]));
        }
    }

    public CurationTokenView(
        CurationToken token,
        AnnotatorSide side,
        CurationConflict? conflict,
        CurationConflict? currentConflict)
    {
        Token = token;
        Side = side;
        Conflict = conflict;
        _currentConflict = currentConflict;
    }

    private static Color DetermineColor(char value)
    {
        return char.ToLower(value) switch
        {
            'a' => Colors.DarkGoldenrod,
            'g' => Colors.DarkGreen,
            'i' => Colors.Brown,
            'm' => Colors.DarkBlue,
            'n' => Colors.DarkRed,
            'o' => Colors.DarkOrchid,
            'p' => Colors.DarkCyan,
            't' => Colors.DarkSlateGray,
            _ => Colors.Transparent
        };
    }
}
