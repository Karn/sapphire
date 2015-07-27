using Core.AuthenticationManager;
using Core.Client;
using Core.Content;
using Core.Content.Model;
using Core.Utils;
using Sapphire.Utils.Misc;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;


// Use setter to set item source of posts

namespace Sapphire.Utils.Controls {
	public sealed partial class PostView : UserControl {

		private static object options;

		private ScrollViewer sv;

		public static ImageSource PlayIcon = App.Current.Resources["PlayIcon"] as BitmapImage;
		public static ImageSource PauseIcon = App.Current.Resources["PauseIcon"] as BitmapImage;

		CancellationTokenSource cts = new CancellationTokenSource();

		public bool IsSinglePost;

		public string LastPostID;

		public bool PostsLoading;

		private string ShareURI;

		bool _updating = false;

		public PostView() {
			this.InitializeComponent();
		}

		#region Properties

		public readonly DependencyProperty URLProperty = DependencyProperty.Register("Url",
			typeof(string),
			typeof(PostView),
			new PropertyMetadata(string.Empty));

		public string URL {
			get { return (string)this.GetValue(URLProperty); }
			set { this.SetValue(URLProperty, value); }
		}

		public readonly DependencyProperty AffectHeaderProperty = DependencyProperty.Register("HeaderProperty",
			typeof(bool),
			typeof(PostView),
			new PropertyMetadata(false));

		public bool AffectHeader {
			get { return (bool)this.GetValue(AffectHeaderProperty); }
			set { this.SetValue(AffectHeaderProperty, value); }
		}

		#endregion

		public void LoadPosts(string url = "") {
			if (!PostsLoading) {
				PostsLoading = true;
				if (!string.IsNullOrWhiteSpace(url))
					Posts.ItemsSource = new IncrementalPostLoader(url);
				else {
					Posts.ItemsSource = new IncrementalPostLoader(URL);
				}

				PostsLoading = false;
			}
		}

		public int FeedItemCount() {
			if (Posts.Items.Count == 1 && !IsSinglePost)
				return 0;

			return Posts.Items.Count;
		}

		public async void LoadSpecificPost(string post_id) {
			App.DisplayStatus(App.LocaleResources.GetString("LoadingPost"));
			IsSinglePost = true;
			scrollViewer.SizeChanged -= scrollViewer_SizeChanged;
			scrollViewer.ViewChanged -= scrollViewer_ViewChanged;
			textBlock1.Visibility = Visibility.Collapsed;
			//Posts.ItemsSource = await CreateRequest.RetrievePosts("", "", post_id);
			App.HideStatus();
		}

		public void ScrollToTop() {
			try {
				Posts.ScrollIntoView(Posts.Items.First());
			} catch { }
		}

		private void GoToBlog(object sender, TappedRoutedEventArgs e) {
			if (((FrameworkElement)sender).Tag != null) {
				var frame = Window.Current.Content as Frame;
				if (!frame.Navigate(typeof(Pages.BlogDetails), ((FrameworkElement)sender).Tag.ToString()))
					throw new Exception("NavFail");
			}
		}

		private void GoToPostDetails(object sender, TappedRoutedEventArgs e) {
			if (!IsSinglePost && ((FrameworkElement)sender).Tag != null) {
				var frame = Window.Current.Content as Frame;
				if (!frame.Navigate(typeof(Pages.DetailedPost), ((FrameworkElement)sender).Tag.ToString()))
					Debug.WriteLine("Failed to Navigate");
			}
		}

		private async void LikeButton_Click(object sender, RoutedEventArgs e) {
			try {
				App.DisplayStatus(App.LocaleResources.GetString("UpdatingLike"));
				var x = ((ToggleButton)sender);
				var notes = ((Run)((TextBlock)((Grid)((StackPanel)x.Parent).Parent).FindName("NoteInfo")).Inlines.First());

				var id = x.Tag.ToString();
				var reblogKey = ((StackPanel)(x).Parent).Tag.ToString();

				bool result = ((bool)(x.IsChecked)) ? await CreateRequest.LikePost(id, reblogKey) : await CreateRequest.UnlikePost(id, reblogKey);

				if (result) {
					try {
						notes.Text = ((bool)(x.IsChecked)) ? (int.Parse(notes.Text) + 1).ToString() :
							(int.Parse(notes.Text) - 1).ToString();
					} catch (Exception ex2) {
					}
				} else {
					((ToggleButton)sender).IsChecked = ((bool)(x.IsChecked)) ? false : true;
					App.Alert("Unable to like post.");
				}
				App.HideStatus();
			} catch (Exception ex) {
				App.Alert("Unable to like post.");
			}
		}

		private async void ReblogButton_Click(object sender, RoutedEventArgs e) {
			if (((ToggleControl)sender).IsChecked == true)
				((ToggleControl)sender).IsChecked = true;
			if (UserPreferences.OneClickReblog) {
				try {
					App.DisplayStatus(App.LocaleResources.GetString("RebloggingPost"));
					var x = ((ToggleControl)sender);
					var notes = ((Run)((TextBlock)((Grid)((StackPanel)x.Parent).Parent).FindName("NoteInfo")).Inlines.First());

					var id = x.Tag.ToString();
					var reblogKey = ((StackPanel)x.Parent).Tag.ToString();
					var tags = UserPreferences.DefaultTags;
					if (!string.IsNullOrEmpty(tags)) {
						tags = tags.Replace(" #", ", ");
						tags = Authentication.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length) : tags.Substring(0, tags.Length)));
					}
					if (await CreateRequest.ReblogPost(id, reblogKey, "", tags)) {
						notes.Text = (int.Parse(notes.Text) + 1).ToString();
						((ToggleControl)sender).IsChecked = true;
					} else {
						((ToggleButton)sender).IsChecked = ((bool)(x.IsChecked)) ? false : true;
						App.Alert("Failed to reblog post.");
					}
					App.HideStatus();
				} catch (Exception ex) {
					((ToggleButton)sender).IsChecked = ((bool)(((ToggleControl)sender).IsChecked)) ? false : true;
				}
			} else {
				var frame = Window.Current.Content as Frame;
				if (!frame.Navigate(typeof(Pages.ReblogPage), new object[] { sender, ((ToggleControl)sender).Tag.ToString() + "," + ((StackPanel)((ToggleControl)sender).Parent).Tag.ToString() }))
					throw new Exception("Navigation Failed");
			}
		}

		private async void PostDraftButton_Click(object sender, RoutedEventArgs e) {
			var post = (Post)((FrameworkElement)sender).Tag;

			if (await CreateRequest.PostDraft(post.id)) {
				App.Alert(App.LocaleResources.GetString("PostCreated"));
				var items = (Posts.ItemsSource as ObservableCollection<Post>).Remove(post);
			} else
				App.Alert(App.LocaleResources.GetString("PostCreationFailed"));
		}

		private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
			App.DisplayStatus(App.LocaleResources.GetString("DeletingPost"));
			var post_id = ((FrameworkElement)sender).Tag.ToString();

			if (await CreateRequest.DeletePost(post_id)) {
				var items = Posts.ItemsSource as ObservableCollection<Post>;
				items.Remove(items.Where(x => x.id == post_id).First());
			} else
				App.Alert(App.LocaleResources.GetString("DeleteFailed"));
			App.HideStatus();
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
						App.DisplayStatus(App.LocaleResources.GetString("LoadingGIF"));
						var mp4 = await CreateRequest.GetConvertedGIFUri(player.Tag.ToString());
						if (!string.IsNullOrWhiteSpace(mp4))
							player.Source = new Uri(mp4);
						else {
							App.Alert(App.LocaleResources.GetString("GIFLoadFailed"));
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
				App.Alert(App.LocaleResources.GetString("GIFLoadFailed"));
			}
		}

		private void PlayButton_LostFocus(object sender, RoutedEventArgs e) {
			try {
				var grid = (Grid)((AppBarButton)sender).Parent;

				var player = ((MediaElement)grid.FindName("Player"));

				player.AutoPlay = false;
				player.Stop();
				((AppBarButton)sender).Icon = new SymbolIcon { Symbol = Symbol.Play };
				((AppBarButton)sender).Tag = "stopped";
			} catch (Exception ex) {
			}
		}

		public void RefreshPosts() {
			LastPostID = "";
			var x = Posts.ItemsSource as ObservableCollection<Post>;
			if (x != null)
				x.Clear();
			LoadPosts();
		}

		private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e) {
			Posts.Width = e.NewSize.Width;
			Posts.Height = e.NewSize.Height;
			scrollViewer.ChangeView(null, 60.0, null);
		}

		private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e) {
			if (!IsSinglePost) {
				if (sv == null)
					sv = sender as ScrollViewer;

				if (!_updating) {
					_updating = true;
					if (sv.VerticalOffset == 0.0) {
						textBlock1.Text = App.LocaleResources.GetString("ReleaseToRefresh");
					} else {
						textBlock1.Text = App.LocaleResources.GetString("PullToRefresh");
					}

					if (!e.IsIntermediate) {
						if (sv.VerticalOffset == 0.0) {
							RefreshPosts();
						}

						sv.ChangeView(null, 60.0, null);
					}

					_updating = false;
				}
			} else {
				((Grid)textBlock1.Parent).Visibility = Visibility.Collapsed;
			}
		}

		private async void SaveFileAsync(Uri fileUri) {
			try {
				App.DisplayStatus(App.LocaleResources.GetString("DownloadingImage"));
				// create the blank file in specified folder
				var imageFolder = await KnownFolders.PicturesLibrary.CreateFolderAsync("Sapphire", CreationCollisionOption.OpenIfExists);
				var path = Path.GetRandomFileName().ToLower() + Path.GetExtension(fileUri.ToString());
				var file = await imageFolder.CreateFileAsync(path, CreationCollisionOption.ReplaceExisting);

				// create the downloader instance and prepare for downloading the file
				var backgroundDownloader = new BackgroundDownloader() { CostPolicy = BackgroundTransferCostPolicy.Always, Method = "GET" };
				var downloadOperation = backgroundDownloader.CreateDownload(fileUri, file);
				await downloadOperation.StartAsync();

				App.HideStatus();
			} catch (Exception ex) {
                Debug.WriteLine("Unable to save media to photos folder.");
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
			if (((FrameworkElement)sender).MaxHeight == 300)
				((FrameworkElement)sender).MaxHeight = 9999;
			else
				((FrameworkElement)sender).MaxHeight = 300;
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
				if (selectedItem.Text.ToString() == "save to phone") {
					SaveFileAsync(new Uri(selectedItem.Tag.ToString()));
				} else if (selectedItem.Text.ToString() == "post details") {
					GoToPostDetails(selectedItem, null);
				} else if (selectedItem.Text.ToString() == "share") {
					ShareURI = selectedItem.Tag.ToString();
					DataTransferManager.GetForCurrentView().DataRequested += PostView_DataRequested;
					DataTransferManager.ShowShareUI();
				} else if (selectedItem.Text.ToString() == "open in browser") {
					await Windows.System.Launcher.LaunchUriAsync(new Uri(selectedItem.Tag.ToString()));
				}
				Grid_Tapped(null, null);
			}
		}

		private void Image_Holding(object sender, HoldingRoutedEventArgs e) {
			options = sender;
			FrameworkElement element = options as FrameworkElement;
			if (element == null) return;

			FlyoutBase.ShowAttachedFlyout(element);
		}

		private void VariableSizedWrapGrid_Loaded(object sender, RoutedEventArgs e) {
			((VariableSizedWrapGrid)sender).ItemWidth = (((VariableSizedWrapGrid)sender).ActualWidth) / 6;
		}
	}
}
