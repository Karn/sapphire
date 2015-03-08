using Core.Client;
using Core.Content;
using Sapphire.Shared.Common;
using Sapphire.Utils.Controls;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Sapphire.Pages {
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class DetailedPost : Page {
		private NavigationHelper navigationHelper;

		private string postId = "0";

		public DetailedPost() {
			this.InitializeComponent();

			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
		}

		public NavigationHelper NavigationHelper {
			get { return this.navigationHelper; }
		}

		private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
			postId = e.NavigationParameter.ToString();
			Debug.WriteLine(postId);
		}

		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) { }

		#region NavigationHelper registration

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e) {
			this.navigationHelper.OnNavigatedFrom(e);
		}
		#endregion

		private void GoToBlog(object sender, TappedRoutedEventArgs e) {
			if (((FrameworkElement)sender).Tag != null) {
				var frame = Window.Current.Content as Frame;
				if (!frame.Navigate(typeof(BlogDetails), ((FrameworkElement)sender).Tag.ToString()))
					throw new Exception("NavFail");
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

					if (await CreateRequest.ReblogPost(id, reblogKey)) {
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

		private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
			//App.DisplayStatus(App.LocaleResources.GetString("DeletingPost"));
			//var post_id = ((FrameworkElement)sender).Tag.ToString();

			//if (await CreateRequest.DeletePost(post_id)) {
			//	var items = Posts.ItemsSource as ObservableCollection<Post>;
			//	items.Remove(items.Where(x => x.id == post_id).First());
			//} else
			//	App.Alert("Failed to delete post.");
			//App.HideStatus();
		}

	}
}
