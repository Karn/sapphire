using APIWrapper.Content;
using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Core.Utils.Converters {
    public sealed class NameToFavBrush : IValueConverter {

        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value.ToString() != null) {
                if (UserStorageUtils.FavBlogList.Any(b => b.Name == value.ToString()))
                    return App.Current.Resources["DefaultFavAsset"] as BitmapImage;
            }
            return App.Current.Resources["DefaultUnfavAsset"] as BitmapImage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
