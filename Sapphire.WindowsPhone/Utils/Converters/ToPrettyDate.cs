using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Sapphire.Utils.Converters {
    public sealed class ToPrettyDate : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, string language) {
            return DateTime.Parse(value.ToString()).ToString("MMMM dd");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
