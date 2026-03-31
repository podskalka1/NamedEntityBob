using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace bakalarka5.Core.Views;

public class TagDisplayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var tag = value as string;
        return string.IsNullOrWhiteSpace(tag) ? "" : $"[{tag}]";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}