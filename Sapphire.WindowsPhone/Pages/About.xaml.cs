﻿using Sapphire.Shared.Common;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Sapphire.Pages {
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class About : Page {
        private NavigationHelper navigationHelper;
        public About() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            AppVersion.Text = string.Format("{0}.{1}.{2}.{3}",
                Package.Current.Id.Version.Major.ToString(),
                Package.Current.Id.Version.Minor.ToString(),
                Package.Current.Id.Version.Build.ToString(),
                Package.Current.Id.Version.Revision.ToString());
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

    }
}