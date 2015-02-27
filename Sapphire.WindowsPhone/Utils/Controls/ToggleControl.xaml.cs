using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Sapphire.Utils.Controls {
    public partial class ToggleControl : ToggleButton {
        public ToggleControl() {
            InitializeComponent();
        }

        public static readonly DependencyProperty EnabledUncheckedProperty =
            DependencyProperty.Register(
            "EnabledUnchecked",
            typeof(ImageSource),
            typeof(ToggleControl),
            new PropertyMetadata(App.Current.Resources["LikeAsset"] as BitmapImage));

        public ImageSource EnabledUnchecked {
            get { return (ImageSource)GetValue(EnabledUncheckedProperty); }
            set { SetValue(EnabledUncheckedProperty, value); }
        }

        static void onEnabledUncheckedChangedCallback(
            DependencyObject dobj,
            DependencyPropertyChangedEventArgs args) {
            //do something if needed
        }

        static void onDisabledUncheckedChangedCallback(
            DependencyObject dobj,
            DependencyPropertyChangedEventArgs args) {
            //do something if needed
        }


        public static readonly DependencyProperty EnabledCheckedProperty =
            DependencyProperty.Register(
            "EnabledChecked",
            typeof(ImageSource),
            typeof(ToggleControl),
            new PropertyMetadata(App.Current.Resources["LikedAsset"] as BitmapImage));

        public ImageSource EnabledChecked {
            get { return (ImageSource)GetValue(EnabledCheckedProperty); }
            set { SetValue(EnabledCheckedProperty, value); }
        }

        static void onEnabledCheckedChangedCallback(
            DependencyObject dobj,
            DependencyPropertyChangedEventArgs args) {
            //do something if needed
        }

        private void ToggleButton_CheckedChanged(object sender, RoutedEventArgs e) {
            ChangeImage();
        }

        private void ToggleButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            ChangeImage();
        }

        private void ToggleButton_Loaded(object sender, RoutedEventArgs e) {
            ChangeImage();
        }

        private void ChangeImage() {
            if (IsEnabled) {
                if (IsChecked == true)
                    ButtonImage.Source = EnabledChecked;
                else
                    ButtonImage.Source = EnabledUnchecked;
            }
        }
    }
}
