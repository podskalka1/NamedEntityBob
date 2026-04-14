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
    protected SelectionManager SelectionManager { get; }

    public bool IsSelected => SelectionManager.Current?.Contains(Model) == true;

    protected InlineNodeView(InlineNode model, SelectionManager selectionManager)
    {
        Model = model;
        SelectionManager = selectionManager;
        SelectionManager.PropertyChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(IsSelected));
            OnPropertyChanged(nameof(BorderBrush));
            OnPropertyChanged(nameof(BorderThickness));
        };
    }

    public virtual IBrush BorderBrush => IsSelected ? Brushes.White : Brushes.Transparent;
    public virtual Thickness BorderThickness => IsSelected ? new Thickness(1) : new Thickness(0);

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}