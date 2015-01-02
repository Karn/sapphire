using APIWrapper.Client;
using APIWrapper.Content;
using APIWrapper.Content.Model;
using APIWrapper.Utils;
using Core.Utils.Misc;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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


// Use setter to set item source of posts

namespace Core.Utils.Controls {
    public sealed partial class PostFeedControl : UserControl {

        private static string TAG = "PostView";

        private static object options;

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

        public PostFeedControl() {
            this.InitializeComponent();
        }

        public readonly DependencyProperty URLProperty = DependencyProperty.Register("Url",
            typeof(string),
            typeof(PostFeedControl),
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
            typeof(PostFeedControl),
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
            Posts.ItemsSource = await CreateRequest.RetrievePost(post_id);
        }

        public void ScrollToTop() {
            Posts.ScrollIntoView(Posts.Items.First());
        }

        private void GoToBlog(object sender, TappedRoutedEventArgs e) {
            if (((FrameworkElement)sender).Tag != null) {
                var frame = Window.Current.Content as Frame;
                if (!frame.Navigate(typeof(Pages.BlogDetails), ((FrameworkElement)sender).Tag.ToString()))
                    throw new Exception("NavFail");
            }
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
                App.DisplayStatus("Liking post...");
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
                        DiagnosticsManager.LogException(ex2, TAG, "Failed to update note count.");
                    }
                } else {
                    MainPage.AlertFlyout.DisplayMessage("Unable to like post.");
                }
                App.HideStatus();
            } catch (Exception ex) {
                DiagnosticsManager.LogException(ex, TAG, "Unable to like post.");
                MainPage.AlertFlyout.DisplayMessage("Unable to like post.");
            }
        }

        private async void ReblogButton_Click(object sender, RoutedEventArgs e) {
            if (UserStore.OneClickReblog) {
                try {
                    App.DisplayStatus("Reblogging post...");
                    var x = ((Button)sender);
                    var notes = ((TextBlock)((Grid)((StackPanel)x.Parent).Parent).FindName("NoteInfo"));

                    var id = x.Tag.ToString();
                    var reblogKey = ((StackPanel)x.Parent).Tag.ToString();

                    if (await CreateRequest.ReblogPost(id, reblogKey)) {
                        x.Background = RebloggedBrush;
                        notes.Text = (int.Parse(notes.Text) + 1).ToString();
                    } else
                        MainPage.AlertFlyout.DisplayMessage("Failed to reblog post.");
                    App.HideStatus();
                } catch (Exception ex) {
                    DiagnosticsManager.LogException(ex, TAG, "Failed to reblog post");
                }
            } else {
                var frame = Window.Current.Content as Frame;
                if (!frame.Navigate(typeof(Pages.ReblogPage), ((Button)sender).Tag.ToString() + "," + ((StackPanel)((Button)sender).Parent).Tag.ToString()))
                    throw new Exception("Navigation Failed");
            }

        }
        private void ReplyButton_Click(object sender, RoutedEventArgs e) {
            var frame = Window.Current.Content as Frame;
            if (!frame.Navigate(typeof(Pages.ReblogPage), "Reply: " + ((Button)sender).Tag.ToString() + "," + ((StackPanel)((Button)sender).Parent).Tag.ToString()))
                throw new Exception("Navigation Failed");
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
            App.DisplayStatus("Deleting post...");
            var post = (Post)((Button)sender).Tag;

            if (await CreateRequest.DeletePost(post.id)) {
                var items = Posts.ItemsSource as ObservableCollection<Post>;
                items.Remove(post);
            } else
                MainPage.AlertFlyout.DisplayMessage("Failed to delete post.");
            App.HideStatus();
        }

        private void ShareButton_Tapped(object sender, TappedRoutedEventArgs e) {
            ShareURI = ((string)((Image)sender).Tag);
            Debug.WriteLine(ShareURI);
            DataTransferManager.GetForCurrentView().DataRequested += PostView_DataRequested;

            DataTransferManager.ShowShareUI();
        }

        void PostView_DataRequested(DataTransferManager sender, DataRequestedEventArgs args) {
            var LinkToShare = new Uri(ShareURI, UriKind.Absolute);
            string extension = Path.GetExtension(LinkToShare.AbsolutePath);

            DataPackage dp = args.Request.Data;
            dp.Properties.Title = "Look at this post on Tumblr!";
            dp.Properties.FileTypes.Add(extension);

            dp.SetWebLink(LinkToShare);
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e) {
            try {
                ((AppBarButton)sender).Focus(FocusState.Programmatic);

                var grid = (Grid)((AppBarButton)sender).Parent;

                var player = ((MediaElement)grid.FindName("Player"));

                if (((AppBarButton)sender).Tag.ToString() == "stopped") {
                    if (player.Tag != null && player.Source == null) {
                        App.DisplayStatus("Downloading GIF...");
                        var mp4 = await CreateRequest.GenerateMP4FromGIF(player.Tag.ToString());
                        if (!string.IsNullOrWhiteSpace(mp4))
                            player.Source = new Uri(mp4);
                        else {
                            DiagnosticsManager.LogException(null, TAG, "Failed to retrieve GIF url.");
                            MainPage.AlertFlyout.DisplayMessage("Unable to load animated Image.");
                            App.HideStatus();
                            return;
                        }
                        App.HideStatus();
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
            } catch (Exception ex) {
                DiagnosticsManager.LogException(ex, TAG, "Failed to load GIF.");
                MainPage.AlertFlyout.DisplayMessage("Unable to load animated Image.");
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
                        textBlock1.Text = "Release to refresh";
                    } else {
                        textBlock1.Text = "Pull to refresh";
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

        private async Task<bool> SaveFileAsync(Uri fileUri) {
            try {
                App.DisplayStatus("Downloading image...");
                // create the blank file in specified folder
                var imageFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Sapphire", CreationCollisionOption.OpenIfExists);
                var path = Path.GetRandomFileName().ToLower() + Path.GetExtension(fileUri.ToString());
                Debug.WriteLine(path);
                var file = await imageFolder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);

                // create the downloader instance and prepare for downloading the file
                var backgroundDownloader = new BackgroundDownloader() { CostPolicy = BackgroundTransferCostPolicy.Always, Method = "GET" };
                var downloadOperation = backgroundDownloader.CreateDownload(fileUri, file);
                await downloadOperation.StartAsync();

                MainPage.AlertFlyout.DisplayMessage("Image saved.");
                App.HideStatus();
                return true;
            } catch (Exception ex) {
                MainPage.AlertFlyout.DisplayMessage("Unable to save photo.");
                DiagnosticsManager.LogException(ex, TAG, "Unable to save media to photos folder.");
                return false;
            }
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

        private void MediatedAdControl_AdSdkError(object sender, Microsoft.AdMediator.Core.Events.AdFailedEventArgs e) {
            Debug.WriteLine("Failed to load ad: {0}", e.ToString());
        }

        private void MediatedAdControl_AdMediatorError(object sender, Microsoft.AdMediator.Core.Events.AdMediatorFailedEventArgs e) {
            Debug.WriteLine("Failed to load ad: {0}", e.ToString());
        }

        private void MediatedAdControl_AdSdkEvent(object sender, Microsoft.AdMediator.Core.Events.AdSdkEventArgs e) {

        }

        private void OtherOptionsButton_Tapped(object sender, TappedRoutedEventArgs e) {
            options = sender;
            FrameworkElement element = sender as FrameworkElement;
            if (element == null) return;

            // If the menu was attached properly, we just need to call this handy method
            FlyoutBase.ShowAttachedFlyout(element);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e) {
            FrameworkElement element = options as FrameworkElement;
            if (element == null) return;

            // If the menu was attached properly, we just need to call this handy method
            var f = FlyoutBase.GetAttachedFlyout(element);
            f.Hide();
        }

        private async void OtherOptions_Tapped(object sender, TappedRoutedEventArgs e) {
            var selectedItem = sender as TextBlock;

            if (selectedItem != null) {
                if (selectedItem.Text.ToString().ToLowerInvariant() == "Save to phone".ToLowerInvariant()) {
                    Debug.WriteLine("Trying to save :" + selectedItem.Tag.ToString());
                    var saved = await SaveFileAsync(new Uri(selectedItem.Tag.ToString()));
                    Debug.WriteLine(saved);
                } else if (selectedItem.Text.ToString().ToLowerInvariant() == "Post details".ToLowerInvariant()) {
                    if (!IsSinglePost) {
                        var frame = Window.Current.Content as Frame;
                        if (!frame.Navigate(typeof(Pages.PostDetails), selectedItem.Tag.ToString())) {
                            Debug.WriteLine("Failed to Navigate");
                        }
                    }
                } else if (selectedItem.Text.ToString().ToLowerInvariant() == "Share".ToLowerInvariant()) {
                    ShareURI = selectedItem.Tag.ToString();
                    Debug.WriteLine(ShareURI);
                    DataTransferManager.GetForCurrentView().DataRequested += PostView_DataRequested;
                    DataTransferManager.ShowShareUI();
                } else if (selectedItem.Text.ToString().ToLowerInvariant() == "Open in browser".ToLowerInvariant()) {
                    await Windows.System.Launcher.LaunchUriAsync(new Uri(selectedItem.Tag.ToString()));
                }
                Grid_Tapped(null, null);
            }
        }

        private void Image_Holding(object sender, HoldingRoutedEventArgs e) {
            Debug.WriteLine("Tapped.");
            options = sender;
            FrameworkElement element = options as FrameworkElement;
            if (element == null) return;

            // If the menu was attached properly, we just need to call this handy method
            FlyoutBase.ShowAttachedFlyout(element);
        }
    }
}
