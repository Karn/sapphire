using APIWrapper.Content.Model;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
                SongName.Text = Source.artist;
                Song.Text = Source.track_name;
            } else {
                Debug.WriteLine("not set");
            }
        }
    }
}
