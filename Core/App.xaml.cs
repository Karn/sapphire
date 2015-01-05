using APIWrapper.AuthenticationManager;
using APIWrapper.Content;
using Core.Shared.Common;
using Core.Utils.Misc;
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

namespace Core {

    public sealed partial class App : Application {
        private TransitionCollection transitions;

        private static StatusBar statusBar;

        public ContinuationManager ContinuationManager { get; private set; }

        public App() {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;

            new UserStore();

            RequestedTheme = UserStore.SelectedTheme == "Dark" ?
                ApplicationTheme.Dark :
                ApplicationTheme.Light;
        }

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

                if (Authentication.AuthenticatedTokens.Count != 0 && Authentication.AuthenticatedSecretTokens.Count != 0) {
                    new APIWrapper.Utils.DiagnosticsManager(Current);
                    if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                        throw new Exception("Failed to create initial page");
                } else {
                    if (!rootFrame.Navigate(typeof(Pages.Login), "first"))
                        throw new Exception("Failed to create initial page");
                }
            }

            Window.Current.Activate();
        }

        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e) {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

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
    }
}
