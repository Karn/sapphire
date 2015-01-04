using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Core.Utils.Converters {
    public sealed class LikedToImage : IValueConverter {

        public static ImageBrush LikeBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Likes.png")) };
        public static ImageBrush LikeFullBrush = new ImageBrush() { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Liked.png")) };


        public object Convert(object value, Type targetType, object parameter, string language) {
            if (!(bool)value) {
                return LikeBrush;
            }
            return LikeFullBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
