using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;
using Windows.UI.Xaml.Documents;

namespace Sapphire.Utils.Controls {
    public sealed partial class CaptionContainer : UserControl {

        private static Regex stripHtmlRegex = new Regex(@"<(?!\/?(blockquote|a|img)(?=>|\s.*>))\/?.*?>", RegexOptions.IgnoreCase);

        public UIElement Container { get; set; }

        public readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption",
            typeof(string),
            typeof(CaptionContainer),
            new PropertyMetadata(string.Empty, new PropertyChangedCallback(OnCaptionChanged)));

        public string Caption {
            get { return (string)this.GetValue(CaptionProperty); }
            set { this.SetValue(CaptionProperty, value); }
        }

        private static void OnCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {

        }

        public CaptionContainer() {
            this.InitializeComponent();

            Container = Content;
            Loaded += CaptionContainer_Loaded;
        }

        private void CaptionContainer_Loaded(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(Caption)) {

                var text = FilterTags(Caption);
                Debug.WriteLine("Caption: {0}", text);

                HtmlDocument x = new HtmlDocument();
                x.LoadHtml(text);

                this.LayoutRoot.Child = GenerateElements(x.DocumentNode.ChildNodes);
            }
        }

        private UIElement GenerateElements() {
            throw new NotImplementedException();
        }

        public static string FilterTags(string input) {
            return stripHtmlRegex.Replace(input, string.Empty);
        }

        public static StackPanel GenerateElements(HtmlNodeCollection nodes) {
            var control = new StackPanel();
            try {
                foreach (HtmlNode t in nodes) {
                    if (!string.IsNullOrWhiteSpace(t.InnerText) && t.InnerText.Trim() != ":") {
                        Debug.WriteLine("Type {0}, Inner: {1}", t.Name, t.InnerHtml);
                        if (t.Name.ToLower() == "blockquote") {
                            control.Children.Add(new Border() { BorderThickness = new Thickness(2, 0, 0, 0), BorderBrush = App.Current.Resources["WindowBackground"] as SolidColorBrush, Margin = new Thickness(16, 0, 0, 0), Child = GenerateElements(t.ChildNodes) });
                        } else {
                            TextBlock tb = new TextBlock() { Style = App.Current.Resources["BodyTextBlockStyle"] as Style, TextWrapping = TextWrapping.Wrap };
                            if (t.Name.ToLower() == "a") {
                                var link = new Hyperlink();
                                link.NavigateUri = new Uri(t.Attributes.AttributesWithName("href").First().Value);
                                link.Inlines.Add(new Run() { Text = t.InnerText.Trim() });

                                tb.Inlines.Add(link);
                            } else if (t.Name.ToLower() == "img") {
                                var link = new Hyperlink();
                                link.NavigateUri = new Uri(t.Attributes.AttributesWithName("href").First().Value);
                                link.Inlines.Add(new Run() { Text = "[image]" });

                                tb.Inlines.Add(link);
                            } else {
                                tb.Inlines.Add(new Run() { Text = t.InnerText.Trim() });
                            }
                            control.Children.Add(tb);
                        }
                    }
                }
                Debug.WriteLine("-------");
            } catch { }

            return control;
        }
    }
}
