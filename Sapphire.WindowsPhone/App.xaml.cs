using Core.AuthenticationManager;
using Core.Content;
using Core.Utils;
using Sapphire.Shared.Common;
using Sapphire.Shared.Pages;
using Sapphire.Utils.Misc;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

namespace Sapphire {

    public sealed partial class App : Application {

		public static ResourceLoader LocaleResources = new ResourceLoader();

		private static StatusBar statusBar;
		private TransitionCollection transitions;

		public ContinuationManager ContinuationManager { get; private set; }

		public App() {
			this.InitializeComponent();
			this.Suspending += this.OnSuspending;

			new UserPreferences();
			Log.i("Initialized user preferences.");

			RequestedTheme = ApplicationTheme.Light;
		}

		protected override async void OnLaunched(LaunchActivatedEventArgs e) {

			Frame rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active.
			if (rootFrame == null) {
				rootFrame = new Frame();

				// Associate the frame with a SuspensionManager key.
				SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

				rootFrame.CacheSize = 1;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
					try {
						await SuspensionManager.RestoreAsync();
					} catch (SuspensionManagerException) { }
				}

				Window.Current.Content = rootFrame;
			}

			if (rootFrame.Content == null) {

				rootFrame.ContentTransitions = null;
				//rootFrame.Navigated += this.RootFrame_FirstNavigated;

				if (statusBar == null)
					statusBar = StatusBar.GetForCurrentView();
				statusBar.ForegroundColor = Colors.White;

				new AppLicenseHandler();
				new Authentication();
				
				if (Authentication.AuthenticatedTokens.Count != 0 && Authentication.AuthenticatedSecretTokens.Count != 0) {

					Analytics.GetInstance().SendEvent("Initialized analytics platform.");

					if (!rootFrame.Navigate(typeof(MainView), e.Arguments))
						throw new Exception("Failed to create initial page");
				} else {
					if (!rootFrame.Navigate(typeof(Login), "first"))
						throw new Exception("Failed to create initial page");
				}
			}

			Window.Current.Activate();
		}

		private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e) {
			var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() { DefaultNavigationTransitionInfo = new ContinuumNavigationTransitionInfo() } };
			rootFrame.Navigated -= this.RootFrame_FirstNavigated;
		}

		private async void OnSuspending(object sender, SuspendingEventArgs e) {
			var deferral = e.SuspendingOperation.GetDeferral();
			await SuspensionManager.SaveAsync();
			deferral.Complete();
		}

		protected override void OnActivated(IActivatedEventArgs e) {
			base.OnActivated(e);

			if (statusBar == null)
				statusBar = StatusBar.GetForCurrentView();

			ContinuationManager = new ContinuationManager();

			var continuationEventArgs = e as IContinuationActivatedEventArgs;
			if (continuationEventArgs != null)
				ContinuationManager.Continue(continuationEventArgs);

			Window.Current.Activate();
		}

		public static async void DisplayStatus(string message = "") {
			statusBar.ProgressIndicator.Text = message;
			await statusBar.ProgressIndicator.ShowAsync();
		}

		public static async void HideStatus() {
			await statusBar.ProgressIndicator.HideAsync();
		}

		public static void Alert(string message) {
			var dialog = (AlertDialog)((Grid)((Page)((Frame)Window.Current.Content).Content).FindName("LayoutRoot")).FindName("PageAlertDialog");
			dialog.DisplayMessage(message);
		}
	}
}
