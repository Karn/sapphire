using Core.AuthenticationManager;
using Core.Content.Model.DatabaseHelpers;
using Core.Utils;
using Sapphire.Shared.Pages;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Sapphire {
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application {

		public static ResourceLoader LocaleResources = new ResourceLoader();

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App() {
			this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs e) {

#if DEBUG
			// Show graphics profiling information while debugging.
			if (System.Diagnostics.Debugger.IsAttached) {
				// Display the current frame rate counters.
				this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif

			Frame rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (rootFrame == null) {
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();
				// Set the default language
				rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

				rootFrame.NavigationFailed += OnNavigationFailed;

				if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
					//TODO: Load state from previously suspended application
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
			}

			if (rootFrame.Content == null) {
				new Authentication();
				if (DatabaseController.GetInstance().GetAccounts().Count != 0) {

					if (!rootFrame.Navigate(typeof(MainView), e.Arguments))
						throw new Exception("Failed to create initial page");
				} else {
					if (!rootFrame.Navigate(typeof(Login), "first"))
						throw new Exception("Failed to create initial page");
				}
			}
			// Ensure the current window is active
			Window.Current.Activate();
		}

		/// <summary>
		/// Invoked when Navigation to a certain page fails
		/// </summary>
		/// <param name="sender">The Frame which failed navigation</param>
		/// <param name="e">Details about the navigation failure</param>
		void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private void OnSuspending(object sender, SuspendingEventArgs e) {
			var deferral = e.SuspendingOperation.GetDeferral();
			//TODO: Save application state and stop any background activity
			deferral.Complete();
		}

		public static void DisplayStatus(string message = "") {
			//statusBar.ProgressIndicator.Text = message;
			//await statusBar.ProgressIndicator.ShowAsync();
		}

		public static void HideStatus() {
			//await statusBar.ProgressIndicator.HideAsync();
		}

		public static void Alert(string message) {
			var d = (AlertDialog)((Grid)((Page)((Frame)Window.Current.Content).Content).FindName("LayoutRoot")).FindName("PageAlertDialog");
			d.DisplayMessage(message);
		}
	}
}