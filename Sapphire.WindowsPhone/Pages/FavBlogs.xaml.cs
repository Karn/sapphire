using Core.Content;
using Core.Content.Model;
using Sapphire.Shared.Common;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Sapphire.Pages {
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class FavBlogs : Page {

		private NavigationHelper navigationHelper;

		public FavBlogs() {
			this.InitializeComponent();

			this.navigationHelper = new NavigationHelper(this);
			this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
			this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

			List.ItemsSource = UserPreferences.FavBlogList;
		}

		#region NavigationHelper registration

		public NavigationHelper NavigationHelper {
			get { return this.navigationHelper; }
		}

		private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
		}

		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) {
		}



		protected override void OnNavigatedTo(NavigationEventArgs e) {
			this.navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e) {
			this.navigationHelper.OnNavigatedFrom(e);
		}

		#endregion


		private void SelectBlogButton_Tapped(object sender, TappedRoutedEventArgs e) {
			UserPreferences.CurrentBlog = ((Button)sender).Tag as Blog;
			MainView.SwitchedBlog = true;
			Frame.GoBack();
		}

		private void ViewButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if (((Button)sender).Tag != null) {
				var frame = Window.Current.Content as Frame;
				if (!frame.Navigate(typeof(BlogDetails), ((Button)sender).Tag.ToString()))
					throw new Exception("Navigation Failed");
			}
		}
	}
}
