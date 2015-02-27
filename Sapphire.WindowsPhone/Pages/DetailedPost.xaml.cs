using Sapphire.Shared.Common;
using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
	}
}
