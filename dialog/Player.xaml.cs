using LibVLCSharp.Shared;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using Application = System.Windows.Application;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace GronkhTV_DL.dialog
{
    /// <summary>
    /// Interaktionslogik für _player.xaml
    /// </summary>
    public partial class Player : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public long VideoDuration
        {
            get { return _videoduration; }
            set
            {
                if (_videoduration != value)
                {
                    _videoduration = value;
                    OnPropertyChanged(nameof(VideoDuration));
                }
            }
        }
        public long TimeElapsed
        {
            get { return _timeelapsed; }
            set
            {
                if (_timeelapsed != value)
                {
                    _timeelapsed = value;
                    OnPropertyChanged(nameof(TimeElapsed));
                }
            }
        }
        public string ToolTipVideoTime
        {
            get
            {
                return $"{TimeSpan.FromSeconds(TimeElapsed):hh\\:mm\\:ss}/{TimeSpan.FromSeconds(VideoDuration):hh\\:mm\\:ss}";
            }
            private set{}
        }

        public Player()
        {
            InitializeComponent();
            this.KeyDown += Player_KeyDown;
            this.KeyUp += Player_KeyUp;
            this.Closing += Player_Closing;
        }

        private void Player_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (player.MediaPlayer == null) return;
            player.MediaPlayer.Stop();
        }

        private void Player_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (player.MediaPlayer == null) return;
            switch (e.Key)
            {
                case System.Windows.Input.Key.Space:
                    Button_Click(this, new());
                    break;
                default:
                    break;
            }
        }

        private void Player_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (player.MediaPlayer == null) return;
            switch (e.Key)
            {
                case System.Windows.Input.Key.Left:
                    player.MediaPlayer.Time -= 10 * 1000;
                    break;
                case System.Windows.Input.Key.Right:
                    player.MediaPlayer.Time += 10 * 1000;
                    break;
                default:
                    break;
            }
        }

        public void Play(string uri,int duration)
        {
            Core.Initialize();
            LibVLC libvlc = new();
            var media = new Media(libvlc, new Uri(uri));
            player.MediaPlayer = new MediaPlayer(media);
            player.MediaPlayer.Play();
            player.MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            VideoDuration = duration;

            emojiBox.Source = TryFindResource("ImgSourcePause") as DrawingImage;
        }

        private void MediaPlayer_TimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
        {
            TimeElapsed = e.Time/1000;
            Application.Current.Dispatcher.Invoke(() =>
            {
                timeSlider.Value = TimeElapsed;
                timeSlider.Maximum = VideoDuration;
                timeSlider.ToolTip = ToolTipVideoTime;
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (player.MediaPlayer == null) return;
            if (player.MediaPlayer.IsPlaying)
            {
                player.MediaPlayer.Pause();
                emojiBox.Source = TryFindResource("ImgSourcePlay") as DrawingImage;
            }
            else
            {
                player.MediaPlayer.Play();
                emojiBox.Source = TryFindResource("ImgSourcePause") as DrawingImage;
            }
        }


        private long _timeelapsed = 0;
        private long _videoduration = 0;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
    }
}
