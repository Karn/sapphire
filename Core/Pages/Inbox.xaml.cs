using APIWrapper.Client;
using APIWrapper.Content;
using APIWrapper.Content.Model;
using Core.Shared.Common;
using Core.Utils.Misc;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
	public sealed partial class Inbox : Page {
		private NavigationHelper navigationHelper;

		public static string TAG = "Inbox";

		public Inbox() {
			this.InitializeComponent();

			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

			Posts.ItemsSource = new IncrementalPostLoader("https://api.tumblr.com/v2/blog/" + UserUtils.CurrentBlog.Name + ".tumblr.com/posts/submission");
		}

		/// <summary>
		/// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
		/// </summary>
		public NavigationHelper NavigationHelper {
			get { return this.navigationHelper; }
		}

		private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
		}

		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) { }

		#region NavigationHelper registration

		protected override void OnNavigatedTo(NavigationEventArgs e) {
			this.navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e) {
			this.navigationHelper.OnNavigatedFrom(e);
		}

		#endregion


		private void GoToBlog(object sender, TappedRoutedEventArgs e) {
			if (((FrameworkElement)sender).Tag != null) {
				var frame = Window.Current.Content as Frame;
				if (!frame.Navigate(typeof(BlogDetails), ((FrameworkElement)sender).Tag.ToString().Split(' ')[0]))
					throw new Exception("Navigation Failed");
			}
		}

		private void RefreshButton_Tapped(object sender, TappedRoutedEventArgs e) {

		}

		private void ReplyButton_Click(object sender, RoutedEventArgs e) {
			var frame = Window.Current.Content as Frame;
			if (!frame.Navigate(typeof(ReblogPage), new object[] { sender, "Reply: " + ((FrameworkElement)sender).Tag.ToString() + "," + "" }))
				throw new Exception("Navigation Failed");
		}

		private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
			App.DisplayStatus(App.LocaleResources.GetString("DeletingPost"));
			var post_id = ((FrameworkElement)sender).Tag.ToString();

			if (await CreateRequest.DeletePost(post_id)) {
				var items = Posts.ItemsSource as ObservableCollection<Post>;
				items.Remove(items.Where(x => x.id == post_id).First());
			} else
				App.Alert("Failed to delete post.");
			App.HideStatus();
		}
	}
}
