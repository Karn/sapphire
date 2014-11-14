using API;
using API.Content;
using API.Data;
using API.Utils;
using Core.Utils.Misc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


// Use setter to set item source of posts

namespace Core.Utils.Controls {
    public sealed partial class PostView : UserControl {

        private ScrollViewer sv;

        private bool AlreadyHasContent;

        public static ImageBrush RebloggedBrush = new ImageBrush { ImageSource = App.Current.Resources["RebloggedAsset"] as BitmapImage };
        public static ImageBrush LikeBrush = new ImageBrush { ImageSource = App.Current.Resources["LikeAsset"] as BitmapImage };
        public static ImageBrush LikeFullBrush = new ImageBrush { ImageSource = App.Current.Resources["LikedAsset"] as BitmapImage };

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
            Posts.ItemsSource = await RequestHandler.RetrievePost(post_id);
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
            if (UserData.IsOneClickReblog) {
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
            } else {
                var frame = Window.Current.Content as Frame;
                if (!frame.Navigate(typeof(Pages.ReblogPage), ((Button)sender).Tag.ToString() + "," + ((StackPanel)((Button)sender).Parent).Tag.ToString()))
                    throw new Exception("Navigation Failed");
            }

        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var post = ((API.Content.Post)((Button)sender).Tag);

            if (await RequestHandler.DeletePost(post.id)) {
                var items = Posts.ItemsSource as ObservableCollection<Post>;
                items.Remove(post);
                //Posts.ItemsSource = items;
            } else
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
            } catch (Exception ex) {
                Debug.WriteLine(ex.Data);
            }
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
            Debug.WriteLine("Saving..");
            // create the blank file in specified folder
            try {
                var imageFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Sapphire", CreationCollisionOption.OpenIfExists);

                var file = await imageFolder.CreateFileAsync(fileUri.ToString().Split('/').Last(), CreationCollisionOption.ReplaceExisting);

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
                DebugHandler.Error("Failed to save image to photo library.", e.StackTrace);
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

        private async void MediaElement_Loaded(object sender, RoutedEventArgs e) {
            //if (((StackPanel)(((MediaElement)sender).Parent)).Tag != null) {
            //    var url = ((StackPanel)(((MediaElement)sender).Parent)).Tag.ToString();
            //    Debug.WriteLine(url);
            //    string pathToFile = FileToTempAsync(new Uri(url)).ToString();

            //    HttpClient WebClient = new HttpClient();
            //    var response = await WebClient.GetAsync(new Uri(url + "?api_key=" + Config.ConsumerKey));
            //    Debug.WriteLine(await response.Content.ReadAsStringAsync());

            //    Debug.WriteLine(pathToFile);
            //}
            //((AppBarButton)(((StackPanel)(((MediaElement)sender).Parent)).FindName("PlayButton"))).IsEnabled = true;
            //((AppBarButton)(((StackPanel)(((MediaElement)sender).Parent)).FindName("StopButton"))).IsEnabled = true;
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
    }
}
