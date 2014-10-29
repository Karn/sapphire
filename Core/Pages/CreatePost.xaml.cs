using API.Authentication;
using API.Content;
using API.Utils;
using Core.Common;
using Core.Utils.Misc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
﻿

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Core.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreatePost : Page {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        Image PhotoView;

        public CreatePost() {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

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
            string type = e.NavigationParameter.ToString();
            Debug.WriteLine(type);
            var x = new List<string>();
            x.Add(type);
            Type.ItemsSource = x;
            PageTitle.Text = PageTitle.Text.ToString() + ": " + type;
            MainPage.NPD.Visibility = Visibility.Collapsed;
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

        private void SelectBlogButton_Tapped(object sender, TappedRoutedEventArgs e) {
            UserData.CurrentBlog = ((Button)sender).Tag as API.Content.Blog;
            MainPage.SwitchedBlog = true;
            Frame.GoBack();
        }

        private void TagBox_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == Windows.System.VirtualKey.Space) {
                var x = ((TextBox)sender).Text.Split(' ');
                var tags = string.Empty;
                var tags2 = string.Empty;
                foreach (var tag in x) {
                    if (!tag.StartsWith("#"))
                        tags += "#" + tag + " ";
                    else {
                        tags += tag + " ";
                    }
                }
                ((TextBox)sender).Text = tags;
                x = tags.Split(' ');
            } else if (e.Key == Windows.System.VirtualKey.Back) {
                var x = ((TextBox)sender).Text.Split(' ');
                var tags = string.Empty;
                for (int i = 0; i < x.Count() - 1; i++) {
                    tags += x.ElementAt(i) + " ";
                }

                ((TextBox)sender).Text = tags;
            }
            ((TextBox)sender).Select(((TextBox)sender).Text.Length, 0);
        }

        private void Post_Text(object sender, RoutedEventArgs e) {
            var title = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Text_Title")).Text;
            var body = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Text_Body")).Text;
            var tags = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Text_Tags")).Text;
            if (!string.IsNullOrEmpty(tags)) {
                tags = tags.Replace(" #", ", ");
                tags = AuthenticationManager.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length - 1) : tags.Substring(0, tags.Length - 1)));
            }
            try {
                API.Content.CreatePost.Text(AuthenticationManager.Utils.UrlEncode(title), AuthenticationManager.Utils.UrlEncode(body), tags);
                Frame.GoBack();
            } catch (Exception ex) {
                MainPage.ErrorFlyout.DisplayMessage("Failed to create text post");
                DebugHandler.Log("Failed to create text post.");
            }
        }

        private void Post_Photo(object sender, RoutedEventArgs e) {
            //var title = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Text_Title")).Text;
            //var body = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Text_Body")).Text;

            //APIContent.Content.CreatePost.Text(title, body);
            //Frame.GoBack();
        }

        private void Post_Quote(object sender, RoutedEventArgs e) {
            var quote = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Quote_Quote")).Text;
            var source = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Quote_Source")).Text;

            var tags = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Text_Tags")).Text;
            if (!string.IsNullOrEmpty(tags)) {
                tags = tags.Replace(" #", ", ");
                tags = AuthenticationManager.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length - 1) : tags.Substring(0, tags.Length - 1)));
            }
            try {
                API.Content.CreatePost.Quote(AuthenticationManager.Utils.UrlEncode(quote), AuthenticationManager.Utils.UrlEncode(source), tags);
                Frame.GoBack();
            } catch (Exception ex) {
                MainPage.ErrorFlyout.DisplayMessage("Failed to create text post");
                DebugHandler.Log("Failed to create text post.");
            }
        }

        private void Post_Link(object sender, RoutedEventArgs e) {
            var title = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Link_Title")).Text;
            var url = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Link_Url")).Text;
            var description = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Link_Description")).Text;

            var tags = ((TextBox)((StackPanel)((Button)sender).Parent).FindName("Text_Tags")).Text;
            if (!string.IsNullOrEmpty(tags)) {
                tags = tags.Replace(" #", ", ");
                tags = AuthenticationManager.Utils.UrlEncode((tags.StartsWith(" ") ? tags.Substring(1, tags.Length - 1) : tags.Substring(0, tags.Length - 1)));
            }
            try {
                API.Content.CreatePost.Link(AuthenticationManager.Utils.UrlEncode(title), AuthenticationManager.Utils.UrlEncode(url), AuthenticationManager.Utils.UrlEncode(description), tags);
                Frame.GoBack();
            } catch (Exception ex) {
                MainPage.ErrorFlyout.DisplayMessage("Failed to create text post");
                DebugHandler.Log("Failed to create text post.");
            }
        }

        private async void Photo_Image_Tapped(object sender, TappedRoutedEventArgs e) {
            //PhotoView = (Image)((Grid)(((StackPanel)((Grid)sender).Parent).FindName("Photo_Grid"))).FindName("Photo_Image");

            var filePicker = new FileOpenPicker();
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            filePicker.SettingsIdentifier = "picker1";
            filePicker.CommitButtonText = "Open File to Process";

            filePicker.PickSingleFileAndContinue();

            //FileOpenPicker openPicker = new FileOpenPicker();
            //openPicker.ViewMode = PickerViewMode.Thumbnail;
            //openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            //openPicker.FileTypeFilter.Add(".jpg");
            //openPicker.FileTypeFilter.Add(".jpeg");
            //openPicker.FileTypeFilter.Add(".png");

            //openPicker.PickSingleFileAndContinue();
            //if (file != null) {
            //    // Application now has read/write access to the picked file
            //    //OutputTextBlock.Text = "Picked photo: " + file.Name;
            //    using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read)) {
            //        // Set the image source to the selected bitmap 
            //        photo = new BitmapImage();
            //        photo.DecodePixelHeight = 200;
            //        photo.DecodePixelWidth = 200;

            //        await photo.SetSourceAsync(fileStream);

            //        var PhotoView = ((Image)(((StackPanel)((Button)sender).Parent).FindName("Photo_Image")));
            //        PhotoView.Source = photo;
            //    } 

            //} else {
            //    //  OutputTextBlock.Text = "Operation cancelled.";
            //}
        }
        public async void ContinueFileOpenPicker(FileOpenPickerContinuationEventArgs args) {
            if (args.Files.Count > 0) {
                StorageFile file = args.Files[0];

                Debug.WriteLine(file.DisplayName);

            } else {

            }
        }

    }
}
