using API.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Core.Utils.Controls {
    public sealed partial class AudioPlayer : UserControl {

        public readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source",
            typeof(string),
            typeof(PostView),
            new PropertyMetadata(string.Empty));

        public Post Source {
            get {
                return (Post)this.GetValue(SourceProperty);
            }
            set {
                this.SetValue(SourceProperty, value);
            }
        }

        public AudioPlayer() {
            this.InitializeComponent();
        }

        public void LoadControl() {
            if (Source != null) {
                //Player.Source = new Uri(Source.audio_url);
                Name.Text = Source.artist;
                Song.Text = Source.track_name;
            } else {
                Debug.WriteLine("not set");
            }
        }
    }
}
