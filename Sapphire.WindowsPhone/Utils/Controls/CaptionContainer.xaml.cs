using HtmlAgilityPack;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;

namespace Sapphire.Utils.Controls {
    public sealed partial class CaptionContainer : UserControl {

        private static Regex stripHtmlRegex = new Regex(@"<(?!\/?(blockquote|a|p|img)(?=>|\s.*>))\/?.*?>", RegexOptions.IgnoreCase);

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
                    if (!string.IsNullOrWhiteSpace(t.InnerHtml)) {
                        Debug.WriteLine("Type {0}, Inner: {1}", t.Name, t.InnerHtml);
                        if (t.Name.ToLower() == "blockquote") {
                            control.Children.Add(new Border() { BorderThickness = new Thickness(2, 0, 0, 0), BorderBrush = App.Current.Resources["WindowBackground"] as SolidColorBrush, Margin = new Thickness(16, 0, 0, 0), Child = GenerateElements(t.ChildNodes) });
                        } else if (t.Name.ToLower() == "a") {
                            control.Children.Add(new HyperlinkButton() { Style = App.Current.Resources["HyperlinkButtonStyle"] as Style, Foreground = App.Current.Resources["HyperlinkColor"] as SolidColorBrush, Content = t.InnerText, NavigateUri = new Uri(t.Attributes.AttributesWithName("href").First().Value) });
                        } else if (t.Name.ToLower() == "img") {
                            control.Children.Add(new HyperlinkButton() { Style = App.Current.Resources["HyperlinkButtonStyle"] as Style, Foreground = App.Current.Resources["HyperlinkColor"] as SolidColorBrush, Content = "(Image)", NavigateUri = new Uri(t.Attributes.AttributesWithName("src").First().Value) });
                        } else {
                            control.Children.Add(new TextBlock() { Style = App.Current.Resources["BodyTextBlockStyle"] as Style, Text = t.InnerText, TextWrapping = TextWrapping.Wrap });
                        }
                    }
                }
                Debug.WriteLine("-------");
            } catch { }

            return control;
        }
    }
}
