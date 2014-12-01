using APIWrapper.Utils;
using APIWrapper.Client;
using APIWrapper.Content;
using APIWrapper.Content.Model;
using Core.Utils.Misc;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.IO;


// Use setter to set item source of posts

namespace Core.Utils.Controls {
    public sealed partial class PostView : UserControl {

        private static string TAG = "PostView";

        private ScrollViewer sv;

        private bool AlreadyHasContent;

        public static ImageBrush RebloggedBrush = new ImageBrush { ImageSource = App.Current.Resources["RebloggedAsset"] as BitmapImage };
        public static ImageBrush LikeBrush = new ImageBrush { ImageSource = App.Current.Resources["LikeAsset"] as BitmapImage };
        public static ImageBrush LikeFullBrush = new ImageBrush { ImageSource = App.Current.Resources["LikedAsset"] as BitmapImage };

        public static ImageSource PlayIcon = App.Current.Resources["PlayIcon"] as BitmapImage;
        public static ImageSource PauseIcon = App.Current.Resources["PauseIcon"] as BitmapImage;


        public bool IsSinglePost;

        public string FirstPostID;
        public string LastPostID;

        int offset = 0;

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

        public readonly DependencyProperty AffectHeaderProperty = DependencyProperty.Register("HeaderProperty",
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

        public void LoadPosts(bool EnforceContentLoad = false) {
            if (EnforceContentLoad || !AlreadyHasContent) {
                if (!PostsLoading) {
                    PostsLoading = true;
                    offset = 0;
                    Posts.ItemsSource = new IncrementalPostLoader(URL, offset);

                    if (!AlreadyHasContent)
                        AlreadyHasContent = true;
                    PostsLoading = false;
                }
            }
        }

        public async void LoadSpecificPost(string post_id) {
            IsSinglePost = true;
            MainPage.sb.ProgressIndicator.Text = "Loading posts...";
            await MainPage.sb.ProgressIndicator.ShowAsync();
            Posts.ItemsSource = await CreateRequest.RetrievePost(post_id);
            await MainPage.sb.ProgressIndicator.HideAsync();
        }

        public void ScrollToTop() {
            Debug.WriteLine("Scrolling to top.");
            //sv.ChangeView(null, 60.0, null);
            Posts.ScrollIntoView(Posts.Items.First());
        }

        private void GoToBlog(object sender, TappedRoutedEventArgs e) {
            var frame = Window.Current.Content as Frame;
            if (!frame.Navigate(typeof(Pages.BlogDetails), ((TextBlock)(e.OriginalSource)).Text.Split(' ')[0]))
                throw new Exception("Navigation Failed");
        }

        private void LikeButton_Loaded(object sender, RoutedEventArgs e) {
            if ((bool)(((ToggleButton)sender).IsChecked)) {
                ((ToggleButton)sender).Background = LikeFullBrush;
            } else {
                ((ToggleButton)sender).Background = LikeBrush;
            }
        }
        private async void LikeButton_Click(object sender, RoutedEventArgs e) {
            try {
                var x = ((ToggleButton)sender);
                var notes = ((TextBlock)((Grid)((StackPanel)x.Parent).Parent).FindName("NoteInfo"));

                var id = x.Tag.ToString();
                var reblogKey = ((StackPanel)(x).Parent).Tag.ToString();

                bool result = ((bool)(x.IsChecked)) ? await CreateRequest.LikePost(id, reblogKey) : await CreateRequest.UnlikePost(id, reblogKey);

                if (result) {
                    try {
                        if ((bool)(x.IsChecked)) {
                            notes.Text = (int.Parse(notes.Text) + 1).ToString();
                            x.Background = LikeFullBrush;
                        } else {
                            notes.Text = (int.Parse(notes.Text) - 1).ToString();
                            x.Background = LikeBrush;
                        }
                    } catch (Exception ex2) {
                        DebugHandler.Error("Failed to update note count. ", ex2.StackTrace);
                    }
                } else {
                    MainPage.AlertFlyout.DisplayMessage("Failed to like post.");
                }
            } catch (Exception ex) {
                DebugHandler.Error("Failed to like post. ", ex.StackTrace);
                MainPage.AlertFlyout.DisplayMessage("Failed to like post.");
            }
        }

        private async void ReblogButton_Click(object sender, RoutedEventArgs e) {
            if (UserStore.OneClickReblog) {
                try {
                    var x = ((Button)sender);
                    var notes = ((TextBlock)((Grid)((StackPanel)x.Parent).Parent).FindName("NoteInfo"));

                    var id = x.Tag.ToString();
                    var reblogKey = ((StackPanel)x.Parent).Tag.ToString();

                    if (await CreateRequest.ReblogPost(id, reblogKey)) {
                        x.Background = RebloggedBrush;
                        notes.Text = (int.Parse(notes.Text) + 1).ToString();
                    } else
                        MainPage.AlertFlyout.DisplayMessage("Failed to reblog post.");
                } catch (Exception ex) {
                    DebugHandler.Error("Failed to reblog post", ex.StackTrace);
                }
            } else {
                var frame = Window.Current.Content as Frame;
                if (!frame.Navigate(typeof(Pages.ReblogPage), ((Button)sender).Tag.ToString() + "," + ((StackPanel)((Button)sender).Parent).Tag.ToString()))
                    throw new Exception("Navigation Failed");
            }

        }

        private async void PostDraftButton_Click(object sender, RoutedEventArgs e) {
            var post = (Post)((Button)sender).Tag;

            if (await CreateRequest.PostDraft(post.id)) {
                MainPage.AlertFlyout.DisplayMessage("Created post.");
                var items = Posts.ItemsSource as ObservableCollection<Post>;
                items.Remove(post);
            } else
                MainPage.AlertFlyout.DisplayMessage("Failed to create post.");

        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var post = (Post)((Button)sender).Tag;

            if (await CreateRequest.DeletePost(post.id)) {
                var items = Posts.ItemsSource as ObservableCollection<Post>;
                items.Remove(post);
            } else
                MainPage.AlertFlyout.DisplayMessage("Failed to delete post.");
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

        private async void AppBarButton_Click(object sender, RoutedEventArgs e) {
            ((AppBarButton)sender).Focus(FocusState.Programmatic);

            var grid = (Grid)((AppBarButton)sender).Parent;

            var player = ((MediaElement)grid.FindName("Player"));

            if (((AppBarButton)sender).Tag.ToString() == "stopped") {
                if (player.Tag != null && player.Source == null) {
                    var mp4 = await CreateRequest.GenerateMP4FromGIF(player.Tag.ToString());
                    player.Source = new Uri(mp4);
                } else {
                    player.AutoPlay = true;
                    player.Play();
                }

                ((AppBarButton)sender).Icon = new SymbolIcon { Symbol = Symbol.Pause };
                ((AppBarButton)sender).Tag = "playing";
            } else {
                player.AutoPlay = false;
                player.Stop();
                ((AppBarButton)sender).Icon = new SymbolIcon { Symbol = Symbol.Play };
                ((AppBarButton)sender).Tag = "stopped";
            }
        }

        private void PlayButton_LostFocus(object sender, RoutedEventArgs e) {
            var grid = (Grid)((AppBarButton)sender).Parent;

            var player = ((MediaElement)grid.FindName("Player"));

            player.AutoPlay = false;
            player.Stop();
            ((AppBarButton)sender).Icon = new SymbolIcon { Symbol = Symbol.Play };
            ((AppBarButton)sender).Tag = "stopped";
        }

        private void scrollViewer_Loaded(object sender, RoutedEventArgs e) {
            scrollViewer.ChangeView(null, 60.0, null);
        }

        public void RefeshPosts() {
            LastPostID = "";
            FirstPostID = "";
            var x = Posts.ItemsSource as ObservableCollection<Post>;
            x.Clear();
            LoadPosts();
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
                        textBlock1.Visibility = Visibility.Collapsed;
                    } else {
                        textBlock1.Opacity = 0.3;
                        textBlock1.Visibility = Visibility.Visible;
                    }

                    if (sv.VerticalOffset != 0.0 && sv.VerticalOffset != 120)
                        _isPullRefresh = true;

                    if (!e.IsIntermediate) {
                        if (sv.VerticalOffset == 0.0 && _isPullRefresh) {
                            Debug.WriteLine("Refreshing feed.");
                            LastPostID = "";
                            FirstPostID = "";
                            var x = Posts.ItemsSource as ObservableCollection<Post>;
                            x.Clear();
                            LoadPosts(true);
                        }

                        sv.ChangeView(null, 60.0, null);
                        _isPullRefresh = false;
                    }

                    _updating = false;
                }
            } else {
                ((Grid)textBlock1.Parent).Visibility = Visibility.Collapsed;
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
            try {

                // create the blank file in specified folder
                var imageFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Sapphire", CreationCollisionOption.OpenIfExists);

                var path = DateTime.Now.ToFileTimeUtc().ToString() + Path.GetExtension(fileUri.ToString());

                var file = await imageFolder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);

                // create the downloader instance and prepare for downloading the file
                var backgroundDownloader = new BackgroundDownloader();
                backgroundDownloader.CostPolicy = BackgroundTransferCostPolicy.Always;
                var downloadOperation = backgroundDownloader.CreateDownload(fileUri, file);
                // start the download operation asynchronously
                await downloadOperation.StartAsync();

                MainPage.AlertFlyout.DisplayMessage("Image saved.");
                return true;
            } catch (Exception ex) {
                MainPage.AlertFlyout.DisplayMessage("Unable to save photo.");
                DiagnosticsManager.LogException(ex, TAG, "Unable to save media to photos folder.");
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
                DebugHandler.Error("Failed to save image to temp storage.", e.StackTrace);
                return new Uri("");
            }
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

        private void AdControl_ErrorOccurred(object sender, Microsoft.Advertising.Mobile.Common.AdErrorEventArgs e) {
            ((Microsoft.Advertising.Mobile.UI.AdControl)sender).Visibility = Visibility.Collapsed;
            ((AdDuplex.Universal.Controls.WinPhone.XAML.AdControl)((Grid)((Microsoft.Advertising.Mobile.UI.AdControl)sender).Parent).FindName("adDuplexAd")).Visibility = Visibility.Visible;
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e) {
            var audioPlayer = ((MediaElement)(((StackPanel)(((AppBarButton)sender).Parent)).FindName("AudioPlayer")));
            if (audioPlayer.Tag.ToString() == "Stopped" || audioPlayer.Tag.ToString() == "Paused") {
                audioPlayer.Play();
                audioPlayer.Tag = "Playing";
                Debug.WriteLine("Playing: " + audioPlayer.Source);
                ((AppBarButton)sender).Icon = new SymbolIcon() { Symbol = Symbol.Pause };
            } else {
                audioPlayer.Pause();
                audioPlayer.Tag = "Paused";
                ((AppBarButton)sender).Icon = new SymbolIcon() { Symbol = Symbol.Play };
            }

        }

        private void StopButton_Click(object sender, RoutedEventArgs e) {
            var audioPlayer = ((MediaElement)(((StackPanel)(((AppBarButton)sender).Parent)).FindName("AudioPlayer")));
            if (audioPlayer.Tag.ToString() == "Playing" || audioPlayer.Tag.ToString() == "Paused") {
                audioPlayer.Stop();
                ((AppBarButton)(((StackPanel)(((AppBarButton)sender).Parent)).FindName("PlayButton"))).Icon = new SymbolIcon() { Symbol = Symbol.Play };
            }
        }
        private void TagPanel_Loaded(object sender, RoutedEventArgs e) {
            ((StackPanel)sender).Width = (Window.Current.Bounds.Width - 12) / 5;
        }

        private void Tag_Tapped(object sender, TappedRoutedEventArgs e) {
            var frame = Window.Current.Content as Frame;
            if (!frame.Navigate(typeof(Pages.PostsPage), "https://api.tumblr.com/v2/tagged?tag=" + ((StackPanel)sender).Tag.ToString())) {
                Debug.WriteLine("Failed to Navigate");
            }
        }

        private void BottomContainer_Loader(object sender, RoutedEventArgs e) {
            if (UserStore.TagsInPosts) {
                ((GridView)((StackPanel)sender).FindName("TagPanel")).Visibility = Visibility.Visible;
            }
        }

        private void CommandPanel_Loaded(object sender, RoutedEventArgs e) {
            var g = ((Grid)sender);
            if (g.Tag != null) {
                if (g.Tag.ToString() == "draft") {
                    ((StackPanel)(g.FindName("NormalCommands"))).Visibility = Visibility.Collapsed;
                    ((StackPanel)(g.FindName("DraftCommands"))).Visibility = Visibility.Visible;
                }
            }
        }

        private void Caption_Tapped(object sender, TappedRoutedEventArgs e) {
            if (((TextBlock)sender).MaxHeight == 300)
                ((TextBlock)sender).MaxHeight = 9999;
            else
                ((TextBlock)sender).MaxHeight = 300;
        }

        private void WebView_Loaded(object sender, RoutedEventArgs e) {
            if (((WebView)sender).Tag != null)
                ((WebView)sender).NavigateToString(((WebView)sender).Tag.ToString());
        }

        private async void DirectURIVideoElement_Loaded(object sender, RoutedEventArgs e) {
            if (((MediaElement)sender).Tag != null) {
                var x = ((MediaElement)sender).Tag.ToString().Split('=');
                Debug.WriteLine(x[1]);
                var url = await YouTube.GetVideoUriAsync(x[1], YouTubeQuality.QualityMedium);

                if (url != null) {
                    ((MediaElement)sender).Source = url.Uri;
                }
                ((MediaElement)sender).Visibility = Visibility.Collapsed;
            }
        }
    }
}
