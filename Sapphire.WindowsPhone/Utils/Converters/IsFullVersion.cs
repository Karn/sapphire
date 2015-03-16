using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Sapphire.Utils.Converters {
    public sealed class IsFullVersion : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return (Core.Utils.AppLicenseHandler.IsTrial) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
