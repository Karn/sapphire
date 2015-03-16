using Core.AuthenticationManager;
using Core.Content;
using Core.Content.Model;
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

			BlogList.ItemsSource = UserPreferences.UserBlogs;
			List.ItemsSource = Authentication.AuthenticatedTokens.Keys;
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

		private void SelectAccountButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if (Authentication.SelectedAccount != ((Button)sender).Tag.ToString()) {
				Authentication.SelectedAccount = ((Button)sender).Tag.ToString();

				string token = "";
				Authentication.AuthenticatedTokens.TryGetValue(Authentication.SelectedAccount, out token);
				Authentication.Token = token;

				string secrettoken = "";
				Authentication.AuthenticatedSecretTokens.TryGetValue(Authentication.SelectedAccount, out secrettoken);
				Authentication.TokenSecret = secrettoken;

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

		private void AddAccount_Click(object sender, RoutedEventArgs e) {
			if (!Frame.Navigate(typeof(Login))) {
				throw new Exception();
			}
			List.ItemsSource = Authentication.AuthenticatedTokens.Keys;
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
					var x = selectedItem.Tag.ToString();

					Authentication.AuthenticatedTokens.Remove(x);
					Authentication.AuthenticatedSecretTokens.Remove(x);
					Debug.WriteLine(Authentication.AuthenticatedTokens.Count);
					if (Authentication.AuthenticatedTokens.Count != 0) {
						if (Authentication.SelectedAccount == x) {
							Authentication.SelectedAccount = Authentication.AuthenticatedTokens.Keys.First();

							string token = "";
							Authentication.AuthenticatedTokens.TryGetValue(Authentication.SelectedAccount, out token);
							Authentication.Token = token;

							string secrettoken = "";
							Authentication.AuthenticatedSecretTokens.TryGetValue(Authentication.SelectedAccount, out secrettoken);
							Authentication.TokenSecret = secrettoken;

							MainView.SwitchedAccount = true;
						}
						Authentication.SetAuthenticatedTokens();
					} else {
						Authentication.SetAuthenticatedTokens();
						Grid_Tapped(null, null);
						if (!Frame.Navigate(typeof(Login))) {
							throw new Exception();
						}
					}
					List.ItemsSource = null;
					List.ItemsSource = Authentication.AuthenticatedTokens.Keys;
				}
				Grid_Tapped(null, null);
			}
		}

		private void SelectBlogButton_Tapped(object sender, TappedRoutedEventArgs e) {
			if (((Button)sender).Tag != null) {
				UserPreferences.CurrentBlog = ((Button)sender).Tag as Blog;
				MainView.SwitchedBlog = true;
				Frame.GoBack();
			}
		}
	}
}
