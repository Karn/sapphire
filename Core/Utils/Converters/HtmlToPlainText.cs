using APIWrapper.Client;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Core.Utils.Converters {
    public sealed class HtmlToPlainText : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value != null) {
                value = CreateRequest.GetPlainTextFromHtml(value.ToString());
            }
            return value;
        }


        //string ConvertBlockQuotes(string html) {
        //    //html = html.Replace("<blockquote>", "&#32;&#32;&#32;&#32;&#32;&#32;&#32;&#32;");
        //    //html = html.Replace("</blockquote>", "");
        //    //html = html.Replace("<p>", "");
        //    //html = html.Replace("</p>", "");
        //    var multiplier = 0;
        //    string pasrsedCode = "";
        //    html = html.Replace("<blockquote>", "ã<blockquote>");
        //    var indents = html.Split('ã');
        //    foreach (var indent in indents) {
        //        string attach = "";
        //        for (int x = 0; x < multiplier; x++) {
        //            attach += "&#32;&#32;&#32;&#32;&#32;&#32;&#32;&#32;";
        //        }
        //        pasrsedCode += indent.Replace("<blockquote>", "" + attach);
        //    }

        //    return pasrsedCode.Replace("ã", "");
        //}

        string FormatCaption(string caption) {
            string tab = "&#32;&#32;&#32;&#32;&#32;&#32;&#32;&#32;";
            caption = caption.Replace("<p>", "").Replace("</p>", ""); ;
            int x = 0;
            while (caption.Contains("<blockquote>")) {
                caption = ReplaceFirst(caption, "<blockquote>", Repeat(tab, x));
                x++;
            }
            while (caption.Contains("</blockquote>")) {
                caption = ReplaceFirst(caption, "</blockquote>", Repeat(tab, x));
                x--;
            }
            Debug.WriteLine(caption);
            //caption = caption.Replace("<blockquote><p>", "<blockquote>");
            //caption = caption.Replace("<blockquote><p>", "<blockquote>");


            return caption;
        }

        string ReplaceFirst(string text, string search, string replace) {
            int pos = text.IndexOf(search);
            if (pos < 0) {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }

        string Repeat(string val, int count) {
            string y = string.Empty;
            for (int x = 0; x < count; x++) {
                y += val;
            }
            return y;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}