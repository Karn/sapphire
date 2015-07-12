using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Sapphire.Utils.Controls {
    public sealed partial class NewPostDialog : UserControl {

        public bool IsActive = false;

        public NewPostDialog() {
            this.InitializeComponent();
        }

        public void AnimateIn() {
            ToggleVisibility.Begin();
        }
        public void AnimateOut() {
            ToggleVisibilityOut.Begin();
        }

        private void PostType_Tapped(object sender, TappedRoutedEventArgs e) {
            var frame = Window.Current.Content as Frame;
            if (!frame.Navigate(typeof(Pages.CreatePost), (((Image)(e.OriginalSource)).Name).ToString()))
                throw new Exception("Navigation Failed");
        }

        private void ToggleVisibilityOut_Completed(object sender, object e) {
            PostButtons.Visibility = Visibility.Collapsed;
        }

        public void CreatePost_Click(object sender, RoutedEventArgs e) {
            ((Button)sender).Focus(FocusState.Pointer);
            if (PostButtons.Visibility == Visibility.Collapsed) {
                BG.Visibility = Visibility.Visible;
                CreatePostIcon.RenderTransform = new CompositeTransform() { Rotation = 45 };
                CreatePostFill.Fill = new SolidColorBrush(Color.FromArgb(255, 180, 75, 75));
                PostButtons.Visibility = Visibility.Visible;
                AnimateIn();
                IsActive = true;
            } else
                CreatePost_LostFocus(null, null);
        }

        public void CreatePost_LostFocus(object sender, RoutedEventArgs e) {
            CreatePostIcon.RenderTransform = new CompositeTransform() { Rotation = 0 };
            CreatePostFill.Fill = App.Current.Resources["ColorPrimary"] as SolidColorBrush;
            AnimateOut();
            BG.Visibility = Visibility.Collapsed;
            IsActive = false;
        }
    }
}
