using API;
using API.Authentication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Core.Utils.Converters {
    public sealed class HtmlToPlainText : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value != null)
                return RequestHandler.GetPlainTextFromHtml(value.ToString());
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
