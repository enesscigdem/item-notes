using Avalonia.Data.Converters;
using Avalonia.Media;
using ItemNotes.Domain.Enums;
using System;
using System.Globalization;

namespace ItemNotes.UI.Converters
{
    /// <summary>
    /// NoteColor enum değerlerini renk fırçasına çeviren converter.
    /// </summary>
    public class NoteColorToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is NoteColor color)
            {
                return color switch
                {
                    NoteColor.Yellow => new SolidColorBrush(Colors.LightYellow),
                    NoteColor.Green => new SolidColorBrush(Colors.LightGreen),
                    NoteColor.Red => new SolidColorBrush(Colors.LightCoral),
                    _ => new SolidColorBrush(Colors.LightYellow)
                };
            }
            return new SolidColorBrush(Colors.LightYellow);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}