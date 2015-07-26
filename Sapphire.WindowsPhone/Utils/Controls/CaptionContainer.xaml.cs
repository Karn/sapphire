using Core.Client;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Sapphire.Utils.Controls {
    public sealed partial class CaptionContainer : UserControl {

        private static Regex stripHtmlRegex = new Regex(@"<(?!\/?(blockquote|a)(?=>|\s.*>))\/?.*?>", RegexOptions.IgnoreCase);

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
            var captionContainer = d as CaptionContainer;
            var text = FilterTags(e.NewValue.ToString());

            captionContainer.Container = GenerateElements(text);
        }

        public CaptionContainer() {
            this.InitializeComponent();

            Container = Content;
            Loaded += CaptionContainer_Loaded;
        }

        private void CaptionContainer_Loaded(object sender, RoutedEventArgs e) {
            if (!string.IsNullOrEmpty(Caption)) {
                Debug.WriteLine("Caption: {0}", Caption);

                var text = FilterTags(Caption);
                this.LayoutRoot.Child = GenerateElements(text);
            }
        }

        public static string FilterTags(string input) {
            return stripHtmlRegex.Replace(input, string.Empty);
        }

        public static StackPanel GenerateElements(string input) {
            var control = new StackPanel();
            try {
                HtmlDocument x = new HtmlDocument();
                x.LoadHtml(input);

                foreach (HtmlNode t in x.DocumentNode.Descendants()) {
                    if (t.Name.ToLower() == "blockquote") {
                        control.Children.Add(new Border() { BorderThickness = new Thickness(2, 0, 0, 0), BorderBrush = new SolidColorBrush(Colors.Black), Margin = new Thickness(8, 0, 0, 0), Child = GenerateElements(t.InnerHtml.ToString()) });
                    } else {
                        control.Children.Add(new TextBlock() { Style = App.Current.Resources["Caption"] as Style, Text = CreateRequest.GetPlainTextFromHtml(t.InnerHtml) });
                    }
                }
            } catch { }

            return control;
        }
    }
}
