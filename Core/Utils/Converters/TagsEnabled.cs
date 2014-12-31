using APIWrapper.Content;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Core.Utils.Converters {
    public sealed class TagsEnabled : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return (UserStore.TagsInPosts && !string.IsNullOrWhiteSpace(value.ToString())) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
