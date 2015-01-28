using APIWrapper.Content;
using APIWrapper.Content.Model;
using Core.Shared.Common;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FavBlogs : Page {

        public FavBlogs() {
            this.InitializeComponent();

            List.ItemsSource = UserStorageUtils.FavBlogList;

        }

        private void SelectBlogButton_Tapped(object sender, TappedRoutedEventArgs e) {
            UserStorageUtils.CurrentBlog = ((Button)sender).Tag as Blog;
            MainPage.SwitchedBlog = true;
            Frame.GoBack();
        }

        private void ViewButton_Tapped(object sender, TappedRoutedEventArgs e) {
            if (((Button)sender).Tag != null) {
                var frame = Window.Current.Content as Frame;
                if (!frame.Navigate(typeof(BlogDetails), ((Button)sender).Tag.ToString()))
                    throw new Exception("Navigation Failed");
            }
        }
    }
}
