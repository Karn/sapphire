﻿using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Sapphire.Utils.Converters {
    public sealed class FollowingToSign : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if ((bool)value) {
                return "-";
            }
            return "+";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
