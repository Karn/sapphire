using APIWrapper.Content.Model;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Core.Utils.Converters {
    public sealed class NotesToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            var notes = value as List<Post.Note>;
            if (notes != null) {
                if (notes.Count != 0)
                    return Visibility.Visible;
                }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
