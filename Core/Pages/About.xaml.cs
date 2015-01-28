using APIWrapper.Content;
using Core.Shared.Common;
using System;
using Windows.ApplicationModel;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Core.Pages {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class About : Page {

        public About() {
            this.InitializeComponent();

            AppVersion.Text = string.Format("{0}.{1}.{2}.{3}",
                Package.Current.Id.Version.Major.ToString(),
                Package.Current.Id.Version.Minor.ToString(),
                Package.Current.Id.Version.Build.ToString(),
                Package.Current.Id.Version.Revision.ToString());
        }
    }
}
