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
using Windows.Storage;
using Windows.Networking.BackgroundTransfer;

// Use setter to set item source of posts

namespace Core.Utils.Controls {
    public sealed partial class PostView : UserControl {

        private ScrollViewer sv;

        private ObservableCollection<API.Content.Post> PostItems = new ObservableCollection<API.Content.Post>();

        public int offset = 0;
        private bool AlreadyHasContent;

        public static bool isAnimating;

        public static ImageBrush RebloggedBrush = new ImageBrush { ImageSource = App.Current.Resources["RebloggedAsset"] as BitmapImage };
        public static ImageBrush LikeBrush = new ImageBrush { ImageSource = App.Current.Resources["LikeAsset"] as BitmapImage };
        public static ImageBrush LikeFullBrush = new ImageBrush { ImageSource = App.Current.Resources["LikedAsset"] as BitmapImage };

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
                        if (Utils.IAPHander.ShowAds && posts.Count > 5)
                            PostItems.Add(new Post { type = "advert" });
                        Posts.ItemsSource = PostItems;
                    } catch (Exception e) {
                        DebugHandler.Error("Error awaiting posts. ", e.StackTrace);
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
            sv.ChangeView(null, 60.0, null, false);
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
                DebugHandler.Error("Failed to reblog post. ", ex.StackTrace);
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
                DebugHandler.Error("Failed to reblog post", ex.StackTrace);
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

        private async void AppBarButton_Click(object sender, RoutedEventArgs e) {

            var _GIFPlaceHolder = ((Image)((Grid)((AppBarButton)sender).Parent).FindName("GIFPlaceHolder"));
            var _GIF = ((Image)((Grid)((AppBarButton)sender).Parent).FindName("GIF"));

            if (((AppBarButton)sender).Tag.ToString() == "Download") {
                ((AppBarButton)sender).Icon = new SymbolIcon(Symbol.Play);
                ((AppBarButton)sender).Tag = "Play";
                ((AppBarButton)sender).IsEnabled = false;
                await AnimateImg(_GIFPlaceHolder, _GIF);
                ((AppBarButton)sender).IsEnabled = true;
            } else {
                var animator = XamlAnimatedGif.AnimationBehavior.GetAnimator(_GIF);
                animator.Play();
                ((AppBarButton)sender).Visibility = Visibility.Collapsed;
            }
        }

        async Task AnimateImg(Image _GIFPlaceHolder, Image _GIF) {
            try {
                Debug.WriteLine("Source:" + XamlAnimatedGif.AnimationBehavior.GetSourceUri(_GIF));

                if (_GIFPlaceHolder.Visibility == Visibility.Visible) {
                    var path = await FileToTempAsync(new Uri(_GIFPlaceHolder.Tag.ToString()));

                    _GIF.SetValue(XamlAnimatedGif.AnimationBehavior.SourceUriProperty, path);

                    _GIFPlaceHolder.Visibility = Visibility.Collapsed;
                }

                //var animator = XamlAnimatedGif.AnimationBehavior.GetAnimator(_GIF);
                //animator.Play();
            } catch (Exception ex) {
                Debug.WriteLine(ex.Data);
            }
        }


        private void scrollViewer_Loaded(object sender, RoutedEventArgs e) {
            scrollViewer.ChangeView(null, 60.0, null);
        }

        public void RefeshPosts() {
            LastPostID = "";
            PostItems.Clear();
            LoadPosts(true);
        }

        private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) {
            Posts.Width = e.NewSize.Width;
            Posts.Height = e.NewSize.Height;
            scrollViewer.ChangeView(null, 60.0, null);
        }

        bool _isPullRefresh = false;
        bool _updating = false;
        private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
            if (!IsSinglePost) {
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
                        _isPullRefresh = false;
                        sv.ChangeView(null, 60.0, null);
                    }

                    _updating = false;
                }
            } else {
                ((Grid)textBlock1.Parent).Visibility = Visibility.Collapsed;
                ((Grid)BottomPull.Parent).Visibility = Visibility.Collapsed;
            }
        }

        private async void OtherOptionsButton_Click(object sender, RoutedEventArgs e) {
            MenuFlyoutItem selectedItem = sender as MenuFlyoutItem;

            if (selectedItem != null) {
                if (selectedItem.Text.ToString().ToLowerInvariant() == "Save to phone".ToLowerInvariant()) {
                    Debug.WriteLine("Trying to save :" + selectedItem.Tag.ToString());
                    Debug.WriteLine(await SaveFileAsync(new Uri(selectedItem.Tag.ToString())));
                } else if (selectedItem.Text.ToString().ToLowerInvariant() == "Share".ToLowerInvariant()) {
                    ShareURI = selectedItem.Tag.ToString();
                    Debug.WriteLine(ShareURI);
                    DataTransferManager.GetForCurrentView().DataRequested += PostView_DataRequested;
                    DataTransferManager.ShowShareUI();
                } else if (selectedItem.Text.ToString().ToLowerInvariant() == "Open in browser".ToLowerInvariant()) {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(selectedItem.Tag.ToString()));
                }
            }
        }

        private async Task<bool> SaveFileAsync(Uri fileUri) {
            Debug.WriteLine("Saving..");
            // create the blank file in specified folder
            try {
                var StorageFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Sapphire", CreationCollisionOption.OpenIfExists);

                var file = await StorageFolder.CreateFileAsync(fileUri.ToString().Split('/').Last(), CreationCollisionOption.ReplaceExisting);

                // create the downloader instance and prepare for downloading the file
                var backgroundDownloader = new BackgroundDownloader();
                var downloadOperation = backgroundDownloader.CreateDownload(fileUri, file);

                // start the download operation asynchronously
                var result = await downloadOperation.StartAsync();
                //var frame =  
                MainPage.ErrorFlyout.DisplayMessage("Image saved.");
                return true;
            } catch (Exception e) {
                MainPage.ErrorFlyout.DisplayMessage("Unable to save photo: " + e.Message);
                return false;
            }
        }

        private async Task<Uri> FileToTempAsync(Uri fileUri) {
            Debug.WriteLine("Saving..");
            // create the blank file in specified folder
            try {
                var StorageFolder = await ApplicationData.Current.TemporaryFolder.CreateFolderAsync("Gifs", CreationCollisionOption.OpenIfExists);

                var file = await StorageFolder.CreateFileAsync(fileUri.ToString().Split('/').Last(), CreationCollisionOption.ReplaceExisting);

                // create the downloader instance and prepare for downloading the file
                var backgroundDownloader = new BackgroundDownloader();
                var downloadOperation = backgroundDownloader.CreateDownload(fileUri, file);

                // start the download operation asynchronously
                var result = await downloadOperation.StartAsync();
                //var frame =  
                Debug.WriteLine(file.Path);
                //MainPage.ErrorFlyout.DisplayMessage("Image saved.");
                return new Uri("ms-appdata:///temp/Gifs/" + fileUri.ToString().Split('/').Last());
            } catch (Exception e) {
                //MainPage.ErrorFlyout.DisplayMessage("Unable to save photo: " + e.Message);
                return new Uri("");
            }
        }

        private void PlayerControl_Loaded(object sender, RoutedEventArgs e) {
            ((AudioPlayer)sender).LoadControl();
        }

        private void GIF_Tapped(object sender, TappedRoutedEventArgs e) {
            var animator = XamlAnimatedGif.AnimationBehavior.GetAnimator((Image)sender);
            animator.Pause();
            var button = ((AppBarButton)((Grid)((Image)sender).Parent).FindName("PlayButton"));
            button.Visibility = Visibility.Visible;
        }

        private void GIF_Loaded(object sender, RoutedEventArgs e) {
            var button = ((AppBarButton)((Grid)((Image)sender).Parent).FindName("PlayButton"));
            button.IsEnabled = true;
        }
    }
}
