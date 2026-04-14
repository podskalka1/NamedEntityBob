namespace bakalarka5.Core.Models;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class TypeItem : InlineContainerNode, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string? _type;
    public string? Type
    {
        get => _type;
        set
        {
            if (_type == value) return;
            _type = value;
            OnPropertyChanged();
        }
    }

    public TypeItem(string? type = null)
    {
        _type = type;
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}