using System;
using System.Globalization;
using Avalonia.Data.Converters;
using ItemNotes.Domain.Enums;

namespace ItemNotes.UI.Converters
{
    public sealed class NoteColorToTextConverter : IValueConverter
    {
        public static readonly NoteColorToTextConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is NoteColor c
                ? c switch
                {
                    NoteColor.Yellow => "Sarı",
                    NoteColor.Green  => "Yeşil",
                    NoteColor.Red    => "Kırmızı",
                    _ => c.ToString()
                }
                : null;

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => value is string s ? s.ToLowerInvariant() switch
            {
                "sarı"   => NoteColor.Yellow,
                "yeşil"  => NoteColor.Green,
                "kırmızı"=> NoteColor.Red,
                _ => null
            } : null;
    }
}