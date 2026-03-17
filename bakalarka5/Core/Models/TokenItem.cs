using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;

namespace bakalarka5.Core.Models;

public class TokenItem : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public string Text { get; set; } = "";

    private string? _tag;
    public string? Tag
    {
        get => _tag;
        set
        {
            if (_tag == value) return;
            _tag = value;
            OnPropertyChanged();
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

    public IBrush Background
    {
        get => new  SolidColorBrush(_determineColor());
        set;
    }
    
    public IBrush BorderBrush => IsSelected ? Brushes.White : Brushes.Transparent;
    public Thickness BorderThickness => IsSelected ? new Thickness(1) : new Thickness(0);

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    
    public TokenItem()
    {
        
        // var rand = new Random();
        // // Background = new SolidColorBrush(Colors.Black);
        // Background = new SolidColorBrush(Color.FromRgb(
        //     (byte)rand.Next(256),
        //     (byte)rand.Next(256),
        //     (byte)rand.Next(256)
        // ));
    }

    private Color _determineColor()
    {
        if (Tag is null) return Colors.Black;
        var colorDefiningCharacter = char.ToLower(Tag[0]);
        return colorDefiningCharacter switch
        {
            'a' => Colors.Yellow,
            'g' => Colors.Lime,
            'i' => Colors.Orange,
            'm' => Colors.Aqua,
            'n' => Colors.IndianRed,
            'o' => Colors.HotPink,
            'p' => Colors.CadetBlue,
            't' => Colors.Gray,
            _ => Colors.Transparent
        };
    }
}