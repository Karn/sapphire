using Core.AuthenticationManager;
using Core.Content;
using Core.Content.Model;
using Core.Content.Model.DatabaseHelpers;
using Sapphire.Shared.Common;
using Sapphire.Shared.Pages;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Sapphire.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountManager : Page {
        private NavigationHelper navigationHelper;

        private static object flyoutControl;

        public AccountManager() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            BlogList.ItemsSource = DatabaseController.GetInstance().GetBlogs();
            List.ItemsSource = DatabaseController.GetInstance().GetAccounts();
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e) {
            //Fix navigating
            if (!Frame.Navigate(typeof(MainView))) {
                throw new Exception();
            }
            e.Handled = true;
            return;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper {
            get { return this.navigationHelper; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e) {
            if (e.NavigationParameter != null) {
                if (e.NavigationParameter.ToString() == "1")
                    Items.SelectedIndex = 1;
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e) {
        }

        #region NavigationHelper registration
        protected override void OnNavigatedTo(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e) {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void AddAccount_Click(object sender, RoutedEventArgs e) {
            if (!Frame.Navigate(typeof(Login))) {
                throw new Exception();
            }
            List.ItemsSource = DatabaseController.GetInstance().GetAccounts();
        }

        private void Image_Holding(object sender, HoldingRoutedEventArgs e) {
            flyoutControl = sender;
            FrameworkElement element = flyoutControl as FrameworkElement;
            if (element == null) return;

            FlyoutBase.ShowAttachedFlyout(element);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e) {
            FrameworkElement element = flyoutControl as FrameworkElement;
            if (element == null) return;

            // If the menu was attached properly, we just need to call this handy method
            var f = FlyoutBase.GetAttachedFlyout(element);
            f.Hide();
        }

        private void OtherOptions_Tapped(object sender, TappedRoutedEventArgs e) {
            var selectedItem = sender as TextBlock;

            if (selectedItem != null) {
                if (selectedItem.Text.ToString() == "remove") {
                    var accountEmail = selectedItem.Tag.ToString();

                    Account user = DatabaseController.GetInstance().GetAccount(accountEmail);
                    DatabaseController.GetInstance().RemoveAccount(accountEmail);

                    Debug.WriteLine(DatabaseController.GetInstance().GetAccounts().Count);
                    if (DatabaseController.GetInstance().GetAccounts().Count != 0) {
                        if (Authentication.SelectedAccount == accountEmail) {
                            Account newAccount = DatabaseController.GetInstance().GetAccounts().First();
                            Authentication.SelectedAccount = newAccount.AccountEmail;
                            
                            Authentication.Token = newAccount.AuthenticatedToken;
                            Authentication.TokenSecret = newAccount.AuthenticationTokenSecret;

                            MainView.SwitchedAccount = true;
                        }
                    } else {
                        Grid_Tapped(null, null);
                        if (!Frame.Navigate(typeof(Login))) {
                            throw new Exception();
                        }
                    }
                    List.ItemsSource = null;
                    List.ItemsSource = DatabaseController.GetInstance().GetAccounts();
                }
                Grid_Tapped(null, null);
            }
        }

        private void Blog_Click(object sender, RoutedEventArgs e) {
            if (((Button)sender).Tag != null) {
                UserPreferences.CurrentBlog = ((Button)sender).Tag as Blog;
                MainView.SwitchedBlog = true;
                Frame.GoBack();
            }
        }

        private void Account_Click(object sender, RoutedEventArgs e) {
            if (Authentication.SelectedAccount != ((Button)sender).Tag.ToString()) {
                Authentication.SelectedAccount = ((Button)sender).Tag.ToString();

                Account user = DatabaseController.GetInstance().GetAccount(Authentication.SelectedAccount);
                Authentication.Token = user.AuthenticatedToken;
                Authentication.TokenSecret = user.AuthenticationTokenSecret;

                MainView.SwitchedAccount = true;
                if (!Frame.Navigate(typeof(MainView))) {
                    throw new Exception();
                }
            } else {
                if (!Frame.Navigate(typeof(MainView))) {
                    throw new Exception();
                }
            }
        }
    }
}
