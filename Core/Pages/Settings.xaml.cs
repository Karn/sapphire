using API.Content;
using API.Data;
using Core.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//Below is how to get application package details
//http://code.msdn.microsoft.com/wpapps/Package-sample-46e239fa



// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public Settings() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            if (!Utils.IAPHander.ShowAds) {
                RemoveAdsButton.Content = "Ads have been removed! Thanks!";
                RemoveAdsButton.IsTapEnabled = false;
                RemoveAdsButton.Background = new SolidColorBrush(Color.FromArgb(255, 51, 63, 74));
            }

            if (Config.SelectedTheme == "Dark")
                ThemeSwitch.IsOn = true;
            EnableNotifications.IsOn = UserData.AreNotificationsEnabled;
            EnableOneClickReblog.IsOn = UserData.IsOneClickReblog;

            MainPage.ErrorFlyout = _ErrorFlyout;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) {

        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void RemoveAds_Click(object sender, RoutedEventArgs e) {
            if (Utils.IAPHander.ShowAds)
                await Utils.IAPHander.RemoveAds();
        }

        private async void ReviewButton_Click(object sender, RoutedEventArgs e) {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=d9b787e4-616a-40ea-bdb4-c81523cb0733"));
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e) {
            if (((ToggleSwitch)sender).IsOn) {
                Config.SelectedTheme = "Dark";
            } else {
                Config.SelectedTheme = "Light";
            }
        }

        private void EnableNotifications_Toggled(object sender, RoutedEventArgs e) {
            if (((ToggleSwitch)sender).IsOn) {
                UserData.AreNotificationsEnabled = true;
            } else {
                UserData.AreNotificationsEnabled = false;
            }
        }

        private async void ClearCacheButton_Click(object sender, RoutedEventArgs e) {
            try {
                var x = await(await ApplicationData.Current.TemporaryFolder.GetFolderAsync("Gifs")).GetFilesAsync();
                foreach (var y in x) {
                    await y.DeleteAsync();
                }
                MainPage.ErrorFlyout.DisplayMessage("Folder has been cleared.");
                ((Button)sender).IsTapEnabled = false;
            } catch (FileNotFoundException ex) {
                MainPage.ErrorFlyout.DisplayMessage("Nothing to delete.");
                ((Button)sender).IsTapEnabled = false;
            }
        }

        private void EnableOneClickReblog_Toggled(object sender, RoutedEventArgs e) {
            if (((ToggleSwitch)sender).IsOn) {
                UserData.IsOneClickReblog = true;
            } else {
                UserData.IsOneClickReblog = false;
            }
        }

    }
}
