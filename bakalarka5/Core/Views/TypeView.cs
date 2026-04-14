using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia.Media;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public class TypeView : InlineNodeView
{
    public TypeItem TypeModel => (TypeItem)Model;

    public string? Tag
    {
        get => TypeModel.Type;
        set
        {
            if (TypeModel.Type == value) return;
            TypeModel.Type = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Background));
        }
    }

    public ObservableCollection<InlineNodeView> Children { get; }

    public IBrush Background => new SolidColorBrush(DetermineColor());

    public TypeView(TypeItem model, SelectionManager selectionManager)
        : base(model, selectionManager)
    {
        Children = new ObservableCollection<InlineNodeView>(
            model.Children.Select(child => ViewFactory.Create(child, selectionManager))
        );

        model.PropertyChanged += ModelOnPropertyChanged;
    }

    private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TypeItem.Type))
        {
            OnPropertyChanged(nameof(Tag));
            OnPropertyChanged(nameof(Background));
        }
    }

    private Color DetermineColor()
    {
        if (Tag is null) return Colors.Black;

        var c = char.ToLower(Tag[0]);
        return c switch
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