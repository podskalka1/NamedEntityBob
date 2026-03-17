using System;
using System.Runtime.InteropServices;
using Avalonia.Media;

namespace bakalarka5.Core.Models;

public class TokenItem
{
    public string Text { get; set; } = "";

    public string? Tag { get; set; }
    public bool IsSelected { get; set; }

    public IBrush Background
    {
        get => new  SolidColorBrush(_determineColor());
        set;
    }

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
            _ => Colors.Black
        };
    }
}