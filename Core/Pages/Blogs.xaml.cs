using APIWrapper.Content;
using APIWrapper.Content.Model;
using Core.Shared.Common;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Blogs : Page {

        public Blogs() {
            this.InitializeComponent();

            List.ItemsSource = UserStorageUtils.UserBlogs;
            
        }

        private void SelectBlogButton_Tapped(object sender, TappedRoutedEventArgs e) {
            UserStorageUtils.CurrentBlog = ((Button)sender).Tag as Blog;
            MainPage.SwitchedBlog = true;
            Frame.GoBack();
        }
    }
}
