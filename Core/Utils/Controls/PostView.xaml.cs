using API.Content;
using API.Authentication;
using API.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Linq;
using Windows.UI.Popups;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;
using API;

// Use setter to set item source of posts

namespace Core.Utils.Controls {
    public sealed partial class PostView : UserControl {

        private static bool EnableAds {
            get {
                return (string.IsNullOrEmpty(UserData.EnableAds) ? true : (UserData.EnableAds.Contains("T") ? true : false));
            }
        }

        private ScrollViewer sv;

        private ObservableCollection<API.Content.Post> PostItems = new ObservableCollection<API.Content.Post>();

        public int offset = 0;
        private bool AlreadyHasContent;

        public static bool isAnimating;

        public static ImageBrush RebloggedBrush = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Reblogged.png")) };
        public static ImageBrush LikeBrush = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Likes.png")) };
        public static ImageBrush LikeFullBrush = new ImageBrush { ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Liked.png")) };

        public bool IsSinglePost;

        public string LastPostID { get; set; }

        public bool PostsLoading;

        private string ShareURI;

        public PostView() {
            this.InitializeComponent();
        }

        public readonly DependencyProperty URLProperty = DependencyProperty.Register("Url",
            typeof(string),
            typeof(PostView),
            new PropertyMetadata(string.Empty));

        public string URL {
            get {
                return (string)this.GetValue(URLProperty);
            }
            set {
                this.SetValue(URLProperty, value);
            }
        }

        public readonly DependencyProperty AffectHeaderProperty = DependencyProperty.Register("Url",
            typeof(bool),
            typeof(PostView),
            new PropertyMetadata(false));

        public bool AffectHeader {
            get {
                return (bool)this.GetValue(AffectHeaderProperty);
            }
            set {
                this.SetValue(AffectHeaderProperty, value);
            }
        }

        public async void LoadPosts(bool EnforceContentLoad = false) {
            if ((EnforceContentLoad || !AlreadyHasContent)) {
                if (!PostsLoading) {
                    PostsLoading = true;

                    MainPage.sb = Windows.UI.ViewManagement.StatusBar.GetForCurrentView();
                    MainPage.sb.ForegroundColor = Color.FromArgb(255, 255, 255, 255);
                    MainPage.sb.ProgressIndicator.Text = "Loading posts...";
                    await MainPage.sb.ProgressIndicator.ShowAsync();
                    try {
                        var posts = new List<Post>();
                        if (string.IsNullOrEmpty(LastPostID))
                            posts = await RequestHandler.RetrievePosts(URL);
                        else
                            posts = await RequestHandler.RetrievePosts(URL, LastPostID);

                        if (posts != null) {
                            foreach (var x in posts) {
                                PostItems.Add(x);
                            }
                            LastPostID = PostItems.Last().id;
                        } else {
                            MainPage.ErrorFlyout.DisplayMessage("Unable to deserialize posts.");
                        }
                        if (Utils.IAPHander.ShowAds)
                            PostItems.Add(new Post { type = "advert" });
                        Posts.ItemsSource = PostItems;
                    } catch (Exception e) {
                        DebugHandler.ErrorLog.Add("Error awaiting posts: " + DateTime.UtcNow.TimeOfDay + ". Ex: " + e);
                        MainPage.ErrorFlyout.DisplayMessage("Load failed due to exception." + e.Message);
                    }

                    await MainPage.sb.ProgressIndicator.HideAsync();
                    if (!AlreadyHasContent)
                        AlreadyHasContent = true;

                    if (AffectHeader)
                        MainPage.CreateButtonIntoView_.Begin();
                    PostsLoading = false;
                }
            }
        }

        public async void LoadSpecificPost(string post_id) {
            IsSinglePost = true;
            MainPage.sb.ProgressIndicator.Text = "Loading posts...";
            await MainPage.sb.ProgressIndicator.ShowAsync();
            foreach (var x in await RequestHandler.RetrievePost(post_id)) {
                Debug.WriteLine("Adding");
                PostItems.Add(x);
            }
            Posts.ItemsSource = PostItems;
            await MainPage.sb.ProgressIndicator.HideAsync();
        }

        public void ScrollToTop() {
            sv.ChangeView(null, 0, null, false);
        }

        private void GoToBlog(object sender, TappedRoutedEventArgs e) {
            var frame = Window.Current.Content as Frame;
            if (!frame.Navigate(typeof(Pages.BlogDetails), ((TextBlock)(e.OriginalSource)).Text.Split(' ')[0]))
                throw new Exception("Navigation Failed");
        }

        private async void LikeButton_Click(object sender, RoutedEventArgs e) {
            try {
                var x = ((ToggleButton)sender);
                var notes = ((TextBlock)((Grid)((StackPanel)x.Parent).Parent).FindName("NoteInfo"));

                var id = x.Tag.ToString();
                var reblogKey = ((StackPanel)(x).Parent).Tag.ToString();

                bool result = ((bool)(x.IsChecked)) ? await RequestHandler.LikePost(id, reblogKey) : await RequestHandler.UnlikePost(id, reblogKey);

                if (result) {
                    if ((bool)(x.IsChecked)) {
                        notes.Text = (int.Parse(notes.Text) + 1).ToString();
                        x.Background = LikeFullBrush;
                    } else {
                        notes.Text = (int.Parse(notes.Text) - 1).ToString();
                        x.Background = LikeBrush;
                    }
                } else
                    MainPage.ErrorFlyout.DisplayMessage("Failed to like post.");
            } catch (Exception ex) {
                DebugHandler.ErrorLog.Add("Failed to reblog post: " + ex.ToString());
                MainPage.ErrorFlyout.DisplayMessage("Failed to reblog post.");
            }
        }

        private async void ReblogButton_Click(object sender, RoutedEventArgs e) {
            //if (!UserData.OneClickReblog)
            //{
            //    var frame = Window.Current.Content as Frame;
            //    if (!frame.Navigate(typeof(Pages.CreatePost)))
            //        throw new Exception("Navigation Failed");
            //}
            try {
                var x = ((Button)sender);
                var notes = ((TextBlock)((Grid)((StackPanel)x.Parent).Parent).FindName("NoteInfo"));

                var id = x.Tag.ToString();
                var reblogKey = ((StackPanel)x.Parent).Tag.ToString();

                if (await RequestHandler.ReblogPost(id, reblogKey)) {
                    x.Background = RebloggedBrush;
                    notes.Text = (int.Parse(notes.Text) + 1).ToString();
                } else
                    MainPage.ErrorFlyout.DisplayMessage("Failed to reblog post.");
            } catch (Exception ex) {
                DebugHandler.ErrorLog.Add("Failed to reblog post: " + ex.ToString());
                MainPage.ErrorFlyout.DisplayMessage("Failed to reblog post.");
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var post = ((API.Content.Post)((Button)sender).Tag);

            if (await RequestHandler.DeletePost(post.id))
                PostItems.Remove(post);
            else
                MainPage.ErrorFlyout.DisplayMessage("Failed to delete post.");
        }

        private void PostDetail_Tapped(object sender, TappedRoutedEventArgs e) {
            if (!IsSinglePost) {
                var frame = Window.Current.Content as Frame;
                if (!frame.Navigate(typeof(Pages.PostDetails), ((StackPanel)sender).Tag)) {
                    Debug.WriteLine("Failed to Navigate");
                }
            }
        }

        private void ShareButton_Tapped(object sender, TappedRoutedEventArgs e) {
            ShareURI = ((string)((Image)sender).Tag);
            Debug.WriteLine(ShareURI);
            DataTransferManager.GetForCurrentView().DataRequested += PostView_DataRequested;

            DataTransferManager.ShowShareUI();
        }

        void PostView_DataRequested(DataTransferManager sender, DataRequestedEventArgs args) {
            var LinkToShare = new Uri(ShareURI, UriKind.Absolute);
            string extension = System.IO.Path.GetExtension(LinkToShare.AbsolutePath);

            DataPackage dp = args.Request.Data;
            dp.Properties.Title = "Look at this post on Tumblr!";
            dp.Properties.FileTypes.Add(extension);

            dp.SetWebLink(LinkToShare);
        }

        private void LikeButton_Loaded(object sender, RoutedEventArgs e) {
            if ((bool)(((ToggleButton)sender).IsChecked)) {
                ((ToggleButton)sender).Background = LikeFullBrush;
            } else {
                ((ToggleButton)sender).Background = LikeBrush;
            }
        }

        private void PlayMedia(object sender, RoutedEventArgs e) {
            var Media = ((MediaElement)((StackPanel)((AppBarToggleButton)sender).Parent).FindName("Media"));
            if (Media.CurrentState != MediaElementState.Playing) {
                Media.Play();
                Debug.WriteLine("Playing");
            } else {
                Media.Pause();
                Debug.WriteLine("Paused");
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e) {
            //var url = ((AppBarButton)sender).Tag.ToString();
            //Debug.WriteLine(url);
            //var view = ((GifView)((Grid)((AppBarButton)sender).Parent).Children.Last());
            //view.LoadGIF(url);
            //view.Visibility = Visibility.Visible;

            //((AppBarButton)sender).Visibility = Visibility.Collapsed;

        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e) {
            try {
                var button = ((HyperlinkButton)((Grid)((Image)sender).Parent).Children.ElementAt(1));
                button.IsEnabled = true;
            } catch (Exception ex) {
                DebugHandler.ErrorLog.Add("Failed to open an image: " + ex);
            }
        }

        private void scrollViewer_Loaded(object sender, RoutedEventArgs e) {
            scrollViewer.ChangeView(null, 60.0, null);
        }

        private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) {
            Posts.Width = e.NewSize.Width;
            Posts.Height = e.NewSize.Height;
            scrollViewer.ChangeView(null, 60.0, null);
        }

        bool _isPullRefresh = false;
        bool _updating = false;
        private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
            if (sv == null)
                sv = sender as ScrollViewer;

            if (!_updating) {
                _updating = true;
                if (sv.VerticalOffset == 0.0) {
                    textBlock1.Opacity = 0.7;
                    textBlock2.Visibility = Visibility.Visible;
                    textBlock1.Visibility = Visibility.Collapsed;
                } else {
                    textBlock1.Opacity = 0.3;
                    textBlock2.Visibility = Visibility.Collapsed;
                    textBlock1.Visibility = Visibility.Visible;
                }

                if (sv.VerticalOffset != 0.0 && sv.VerticalOffset != 120)
                    _isPullRefresh = true;

                if (!e.IsIntermediate) {
                    if (sv.VerticalOffset == 0.0 && _isPullRefresh) {
                        //Refresh the feed
                        //offset = 0;
                        Debug.WriteLine("Refreshing feed.");
                        LastPostID = "";
                        PostItems.Clear();
                        LoadPosts(true);
                    } else if (sv.VerticalOffset == 120.0 && _isPullRefresh) {
                        Debug.WriteLine("Loading more items to feed. " + LastPostID);
                        LoadPosts(true);
                    }
                    //_isPullRefresh = false;
                    sv.ChangeView(null, 60.0, null);
                }

                _updating = false;
            }
        }

    }
}
