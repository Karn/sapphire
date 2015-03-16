using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Sapphire.Utils.Converters {
    public sealed class FollowingToBG : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if ((bool)value) {
                return new SolidColorBrush(Color.FromArgb(100, 240, 240, 240));
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
