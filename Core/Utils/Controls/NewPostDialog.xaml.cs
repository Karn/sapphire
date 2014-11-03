using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Core.Utils.Controls
{
    public sealed partial class NewPostDialog : UserControl
    {
        public NewPostDialog()
        {
            this.InitializeComponent();
        }

        public void AnimateIn() {
            //Storyboard1.Begin();
        }

        private void PostType_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((((Image)(e.OriginalSource)).Name).ToString().ToLower() == "photo") {
                MainPage.ErrorFlyout.DisplayMessage("Sorry, photo posts aren't quite ready yet!");
            } else {
                var frame = Window.Current.Content as Frame;
                if (!frame.Navigate(typeof(Pages.CreatePost), (((Image)(e.OriginalSource)).Name).ToString()))
                    throw new Exception("Navigation Failed");
            }
        }
    }
}
