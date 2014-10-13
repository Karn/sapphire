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
            if (value != null) {
                value = RequestHandler.GetPlainTextFromHtml(value.ToString());
                Debug.WriteLine(value);
            }
            return value;
        }


        string ConvertBlockQuotes(string html) {
            //html = html.Replace("<blockquote>", "&#32;&#32;&#32;&#32;&#32;&#32;&#32;&#32;");
            //html = html.Replace("</blockquote>", "");
            //html = html.Replace("<p>", "");
            //html = html.Replace("</p>", "");
            var multiplier = 0;
            string pasrsedCode = "";
            html = html.Replace("<blockquote>", "ã<blockquote>");
            var indents = html.Split('ã');
            foreach (var indent in indents) {
                string attach = "";
                for (int x = 0; x < multiplier; x++) {
                    attach += "&#32;&#32;&#32;&#32;&#32;&#32;&#32;&#32;";
                }
                pasrsedCode += indent.Replace("<blockquote>", "" + attach);
            }

            return pasrsedCode.Replace("ã", "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
