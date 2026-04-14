using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Media;
using bakalarka5.Core.Models;

namespace bakalarka5.Core.Views;

public abstract class InlineNodeView : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public InlineNode Model { get; }

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

    public virtual IBrush BorderBrush => IsSelected ? Brushes.White : Brushes.Transparent;
    public virtual Thickness BorderThickness => IsSelected ? new Thickness(1) : new Thickness(0);

    protected InlineNodeView(InlineNode model)
    {
        Model = model;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}