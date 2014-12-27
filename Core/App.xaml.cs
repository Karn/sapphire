﻿using APIWrapper.AuthenticationManager;
using APIWrapper.Client;
using APIWrapper.Content;
using Core.Common;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Pivot Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace Core {
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application {
        private TransitionCollection transitions;

        static StatusBar statusBar;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        /// 
        public ContinuationManager ContinuationManager { get; private set; }

        public App() {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;

            new UserStore();

            if (UserStore.SelectedTheme == "Dark") {
                RequestedTheme = ApplicationTheme.Dark;
            } else {
                RequestedTheme = ApplicationTheme.Light;
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e) {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached) {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active.
            if (rootFrame == null) {
                // Create a Frame to act as the navigation context and navigate to the first page.
                rootFrame = new Frame();

                // Associate the frame with a SuspensionManager key.
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                // TODO: Change this value to a cache size that is appropriate for your application.
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
                    // Restore the saved session state only when appropriate.
                    try {
                        await SuspensionManager.RestoreAsync();
                    } catch (SuspensionManagerException) {
                        // Something went wrong restoring state.
                        // Assume there is no state and continue.
                    }
                }

                // Place the frame in the current Window.
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null) {
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null) {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions) {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;

                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

                statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = Color.FromArgb(255, 40, 52, 64);
                statusBar.ForegroundColor = Color.FromArgb(255, 255, 255, 255);

                new Utils.AppLicenseHandler();
                new Authentication();
                new RequestBuilder();
                if (Authentication.AuthenticatedTokens.Count != 0 && Authentication.AuthenticatedSecretTokens.Count != 0) {
                    new APIWrapper.Utils.DiagnosticsManager(Current);
                    if (!rootFrame.Navigate(typeof(MainPage), e.Arguments)) {
                        throw new Exception("Failed to create initial page");
                    }
                } else {
                    if (!rootFrame.Navigate(typeof(Pages.Login), "first")) {
                        throw new Exception("Failed to create initial page");
                    }
                }

            }

            // Ensure the current window is active.
            Window.Current.Activate();
        }

        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e) {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e) {
            var deferral = e.SuspendingOperation.GetDeferral();
            await SuspensionManager.SaveAsync();
            deferral.Complete();
        }

        protected override void OnActivated(IActivatedEventArgs e) {
            base.OnActivated(e);

            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            StatusBar.GetForCurrentView().BackgroundColor = Color.FromArgb(255, 40, 52, 64);
            StatusBar.GetForCurrentView().ForegroundColor = Color.FromArgb(255, 255, 255, 255);

            ContinuationManager = new ContinuationManager();

            //Frame rootFrame = CreateRootFrame();
            //await RestoreStatusAsync(e.PreviousExecutionState);

            //if (rootFrame.Content == null) {
            //    rootFrame.Navigate(typeof(MainPage));
            //}

            var continuationEventArgs = e as IContinuationActivatedEventArgs;
            if (continuationEventArgs != null) {
                //Frame scenarioFrame = MainPage.Current.FindName("ScenarioFrame") as Frame;
                //if (scenarioFrame != null) {
                // Call ContinuationManager to handle continuation activation
                ContinuationManager.Continue(continuationEventArgs);
                //}
            }

            Window.Current.Activate();
        }

        public static async void DisplayStatus(string message = "") {
            statusBar.ProgressIndicator.Text = message;
            await statusBar.ProgressIndicator.ShowAsync();
        }

        public static async void HideStatus() {
            await statusBar.ProgressIndicator.HideAsync();
        }
    }
}
