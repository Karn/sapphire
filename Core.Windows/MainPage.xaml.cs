using APIWrapper.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Core {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        public MainPage() {
            this.InitializeComponent();
        }

        private void AccountDetails_Tapped(object sender, TappedRoutedEventArgs e) {
            //if (UserStore.CurrentBlog != null) {
            //    switch (((StackPanel)sender).Tag.ToString()) {
            //        case "Posts":
            //            if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/posts"))
            //                DiagnosticsManager.LogException(null, TAG, "Failed to navigate to current blogs posts.");
            //            break;
            //        case "Likes":
            //            if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/user/likes"))
            //                DiagnosticsManager.LogException(null, TAG, "Failed to navigate to current blogs likes.");
            //            break;
            //        case "Followers":
            //        case "Following":
            //            if (!Frame.Navigate(typeof(Pages.FollowersFollowing), ((StackPanel)sender).Tag.ToString()))
            //                DiagnosticsManager.LogException(null, TAG, "Failed to navigate to Following.");
            //            break;
            //    }
            //}
        }

        private void ManageBlogs_Tapped(object sender, TappedRoutedEventArgs e) {
            //if (!Frame.Navigate(typeof(Pages.Blogs)))
            //    DiagnosticsManager.LogException(null, TAG, "Failed to navigate to blog selection.");
        }

        private void Inbox_Tapped(object sender, TappedRoutedEventArgs e) {
            //if (UserStore.CurrentBlog != null) {
            //    if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/posts/submission")) {
            //        DiagnosticsManager.LogException(null, TAG, "Failed to navigate to inbox.");
            //    }
            //}
        }

        private void Drafts_Tapped(object sender, TappedRoutedEventArgs e) {
            //if (UserStore.CurrentBlog != null) {
            //    if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/posts/draft")) {
            //        DiagnosticsManager.LogException(null, TAG, "Failed to navigate to drafts.");
            //    }
            //}
        }

        private void Queue_Tapped(object sender, TappedRoutedEventArgs e) {
            //if (UserStore.CurrentBlog != null) {
            //    if (!Frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/blog/" + UserStore.CurrentBlog.Name + ".tumblr.com/posts/queue")) {
            //        DiagnosticsManager.LogException(null, TAG, "Failed to navigate to queue.");
            //    }
            //}
        }

        private void Favs_List_Tapped(object sender, TappedRoutedEventArgs e) {
            //if (UserStore.CurrentBlog != null) {
            //    if (!Frame.Navigate(typeof(Pages.FavBlogs)))
            //        DiagnosticsManager.LogException(null, TAG, "Failed to navigate to favorite blogs.");
            //}
        }
    }
}
