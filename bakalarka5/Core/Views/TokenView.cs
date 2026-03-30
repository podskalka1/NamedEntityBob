using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public class TokenView : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public TokenItem Model { get; }

    public string Text => Model.Text;

    public string? Tag
    {
        get => Model.Tag;
        set
        {
            if (Model.Tag == value) return;
            Model.Tag = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Background));
        }
    }

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

    public TokenView(TokenItem model)
    {
        Model = model;
        Model.PropertyChanged += ModelOnPropertyChanged;
    }

    private void ModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TokenItem.Tag))
        {
            OnPropertyChanged(nameof(Tag));
            OnPropertyChanged(nameof(Background));
        }

        if (e.PropertyName == nameof(TokenItem.Text))
        {
            OnPropertyChanged(nameof(Text));
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

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}