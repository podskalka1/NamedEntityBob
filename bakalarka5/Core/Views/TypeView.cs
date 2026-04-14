using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
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

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BorderBrush));
            OnPropertyChanged(nameof(BorderThickness));
        }
    }

    public IBrush Background => new SolidColorBrush(DetermineColor());
    public IBrush BorderBrush => IsSelected ? Brushes.White : Brushes.Transparent;
    public Thickness BorderThickness => IsSelected ? new Thickness(1) : new Thickness(0);

    public TypeView(TypeItem model) : base(model)
    {
        Children = new ObservableCollection<InlineNodeView>(
            model.Children.Select(ViewFactory.Create)
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