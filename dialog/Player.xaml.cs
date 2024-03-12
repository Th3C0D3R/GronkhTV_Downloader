using LibVLCSharp.Shared;
using System.Windows;
using Application = System.Windows.Application;
using MediaPlayer = LibVLCSharp.Shared.MediaPlayer;

namespace GronkhTV_DL.dialog
{
	/// <summary>
	/// Interaktionslogik für _player.xaml
	/// </summary>
	public partial class Player : Window
	{

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

		public void Play(string uri)
		{
			Core.Initialize();
			LibVLC libvlc = new();
			var media = new Media(libvlc, new Uri(uri));
			player.MediaPlayer = new MediaPlayer(media) { EnableHardwareDecoding = true, EnableKeyInput = true, EnableMouseInput = true};
			player.MediaPlayer.Play();

            emojiBox.Text = Application.Current.Resources["PauseButtonText"] as string;
        }

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (player.MediaPlayer == null) return;
			if (player.MediaPlayer.IsPlaying)
			{
				player.MediaPlayer.Pause();
				emojiBox.Text = Application.Current.Resources["PlayButtonText"] as string;
			}
			else
			{
				player.MediaPlayer.Play();
                emojiBox.Text = Application.Current.Resources["PauseButtonText"] as string;
			}
		}

    }
}
