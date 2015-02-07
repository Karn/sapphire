using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Core.Utils.Converters {
    public sealed class IsNotFullVersion : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return (AppLicenseHandler.IsTrial) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
