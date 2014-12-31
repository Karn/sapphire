using APIWrapper.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;

namespace Core.Utils.Misc {
    public class CaptionFormatter {

        static int QuoteCount = 0;

        public static string GetText(TextBlock element) {
            if (element != null)
                return element.GetValue(ArticleContentProperty) as string;
            return string.Empty;
        }

        public static void SetText(TextBlock element, string value) {
            if (element != null)
                element.SetValue(ArticleContentProperty, value);
        }

        public static readonly DependencyProperty ArticleContentProperty =
            DependencyProperty.RegisterAttached(
                "Text",
                typeof(string),
                typeof(CaptionFormatter),
                new PropertyMetadata(null, OnInlineListPropertyChanged));

        private static void OnInlineListPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var tb = obj as TextBlock;
            if (tb == null) return;
            string text = e.NewValue as string;
            QuoteCount = 0;
            if (!string.IsNullOrWhiteSpace(text)) {
                tb.Inlines.Clear();
                //text = text.ToLower().Replace("<p>", "").Replace("</p>", "").Replace("</br>", "");
                text = text.ToLower().Replace("<blockquote>", "{♥○").Replace("</blockquote>", "}♥○");
                if (text.Contains("♥○") || text.Contains("<a"))
                    AddInlineControls(tb, SplitSpace(text));
                else
                    tb.Inlines.Add(GetRunControl(text));
            }
        }

        private static void AddInlineControls(TextBlock textBlock, string[] quotes) {
            foreach (var tag in quotes) {
                if (tag.Contains("<a")) {
                    textBlock.Inlines.Add(GetHyperLink(tag));
                    textBlock.Inlines.Add(GetRunControl(" "));
                } else {
                    textBlock.Inlines.Add(GetRunControl(tag));
                }
            }
        }

        private static Hyperlink GetHyperLink(string uri) {
            Hyperlink hyper = new Hyperlink();
            if (uri.Contains("class=\"tumblr_blog\"")) {
                uri = CreateRequest.GetPlainTextFromHtml(uri);
                hyper.Inlines.Add(GetRunControl(uri + "\n    "));
                hyper.Click += BlogName_Click;
            } else if (uri.StartsWith("<a href=")) {
                var link = Find(uri).First();
                hyper.NavigateUri = new Uri(link.Href);
                hyper.Inlines.Add(GetRunControl(link.Text));
            }
            hyper.Foreground = new SolidColorBrush(Color.FromArgb(191, 40, 52, 64));
            return hyper;
        }

        private static void BlogName_Click(Hyperlink sender, HyperlinkClickEventArgs args) {
            var blogName = ((Run)(((Hyperlink)sender).Inlines.First())).Text.ToString().Replace(" ", "").Trim(':', '\n', ' ');
            var frame = Window.Current.Content as Frame;
            if (!frame.Navigate(typeof(Pages.BlogDetails), blogName))
                throw new Exception("NavFail");
        }

        private static Run GetRunControl(string text) {
            Run run = new Run();
            run.Text = CreateRequest.GetPlainTextFromHtml(text);
            if (run.Text.Contains('{')) {
                QuoteCount++;
                run.Text = run.Text.Replace("{", GenerateBlock(QuoteCount));
            } else if (run.Text.Contains('}')) {
                QuoteCount--;
                run.Text = run.Text.Replace("}", GenerateBlock(QuoteCount));
            }
            return run;
        }

        private static string GenerateBlock(int count) {
            string spaces = "";
            for (int x = 0; x < count; x++)
                spaces += "    ";
            return spaces;
        }

        private static string[] SplitSpace(string val) {
            string[] splittedVal = val.Split(new string[] { "♥○", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            return splittedVal;
        }

        public static List<LinkItem> Find(string file) {
            List<LinkItem> list = new List<LinkItem>();

            // 1.
            // Find all matches in file.
            MatchCollection m1 = Regex.Matches(file, @"(<a.*?>.*?</a>)",
                RegexOptions.Singleline);

            // 2.
            // Loop over each match.
            foreach (Match m in m1) {
                string value = m.Groups[1].Value;
                LinkItem i = new LinkItem();

                // 3.
                // Get href attribute.
                Match m2 = Regex.Match(value, @"href=\""(.*?)\""",
                RegexOptions.Singleline);
                if (m2.Success) {
                    i.Href = m2.Groups[1].Value;
                }

                // 4.
                // Remove inner tags from text.
                string t = Regex.Replace(value, @"\s*<.*?>\s*", "",
                RegexOptions.Singleline);
                i.Text = t;

                list.Add(i);
            }
            return list;
        }

        public struct LinkItem {
            public string Href;
            public string Text;

            public override string ToString() {
                return Href + "\n\t" + Text;
            }
        }

    }
}
