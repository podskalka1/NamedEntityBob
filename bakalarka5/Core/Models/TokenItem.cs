using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace bakalarka5.Core.Models;
public class TokenItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Text { get; init; } = "";

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

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}