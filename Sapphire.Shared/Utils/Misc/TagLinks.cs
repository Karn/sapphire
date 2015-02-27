using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace Sapphire.Shared.Utils.Misc {
    public class TagLinks {
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
                typeof(TagLinks),
                new PropertyMetadata(null, OnInlineListPropertyChanged));

        private static void OnInlineListPropertyChanged(DependencyObject obj,
             DependencyPropertyChangedEventArgs e) {
            var tb = obj as TextBlock;
            if (tb == null)
                return;
            string text = e.NewValue as string;
            tb.Inlines.Clear();
            if (text != null) {
                if (text.Contains("#"))
                    AddInlineControls(tb, SplitSpace(text));
                else
                    tb.Inlines.Add(GetRunControl(text));
            }
        }

        private static void AddInlineControls(TextBlock textBlock, string[] tags) {
            foreach (var tag in tags) {
                textBlock.Inlines.Add(GetHyperLink("#" + tag));
                textBlock.Inlines.Add(GetRunControl(" "));
            }
        }

        private static Hyperlink GetHyperLink(string uri) {
            Hyperlink hyper = new Hyperlink();
            hyper.Click += Hyper_Click;
            hyper.Inlines.Add(GetRunControl(uri));
            return hyper;
        }

        private static void Hyper_Click(Hyperlink sender, HyperlinkClickEventArgs args) {
            var tag = ((Run)(((Hyperlink)sender).Inlines.First())).Text.ToString().TrimStart('#');
            var frame = Window.Current.Content as Frame;
#if WINDOWS_PHONE_APP
            if (!frame.Navigate(typeof(Sapphire.Pages.PostsPage), "https://api.tumblr.com/v2/tagged?tag=" + tag)) {
                Debug.WriteLine("Failed to Navigate");
            }
#endif
        }

        private static Run GetRunControl(string text) {
            Run run = new Run();
            run.Text = text;
            return run;
        }

        private static string[] SplitSpace(string val) {
            string[] splittedVal = val.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
            return splittedVal;
        }
    }
}
