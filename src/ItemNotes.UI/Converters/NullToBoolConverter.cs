using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ItemNotes.UI.Converters
{
    /// <summary>
    /// Null referansların boolean ifadeye dönüştürülmesi için kullanılan converter.
    /// Varsayılan olarak null değil ise true döner; invert ayarlandığında tam tersi.
    /// </summary>
    public class NullToBoolConverter : IValueConverter
    {
        /// <summary>
        /// Sonucun terslenmesini sağlar.
        /// </summary>
        public bool Invert { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool hasValue = value != null;
            return Invert ? !hasValue : hasValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}