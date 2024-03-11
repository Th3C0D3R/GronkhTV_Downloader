using GronkhTV_DL.classes;
using GronkhTV_DL.dialog;
using GronkhTV_DL.dialog.classes;
using GronkhTV_DL.Properties;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static GronkhTV_DL.classes.Globals;
using Brushes = System.Windows.Media.Brushes;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;


namespace GronkhTV_DL
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private DispatcherTimer timer;
		public MainWindow()
		{
			InitializeComponent();
			Width = Settings.Default.Width;
			Height = Settings.Default.Height;
			AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
		}
		private List<Video> CollectStreams()
		{
			Dispatcher.Invoke(() =>
			{
				CurrentAction = "Loading Website...";
				ProgressValue = 0;
			});
			try
			{
				StreamsLoaded = false;

				webDriver.Navigate().GoToUrl("https://gronkh.tv/streams/");
				jsExecuter = (IJavaScriptExecutor)webDriver;

				string x = (string)jsExecuter.ExecuteAsyncScript(Properties.Resources.fetchStreams);

				Dispatcher.Invoke(() =>
				{
					CurrentAction = "Deserialize result...";
					ProgressValue = 35;
				});

				Root? root = JsonConvert.DeserializeObject<Root>(x);
				if (root == null) return [];

				Dispatcher.Invoke(() =>
				{
					CurrentAction = "Ordering streamlist";
					ProgressValue = 70;
				});

				List<Video>? videos = root.results?.videos?.OrderBy(v => v.episode).Reverse().ToList();
				if (videos == null) return [];

				StreamsLoaded = true;
				Dispatcher.Invoke(() =>
				{
					CurrentAction = "Finish loading streams!";
					ProgressValue = 100;
				});
				return videos;
			}
			catch (Exception)
			{
				return [];
			}
		}
		private void FillStreamList(List<Video> videos)
		{
			Dispatcher.Invoke(() =>
			{
				sbiCurrentProcess.Foreground = Brushes.Orange;
				CurrentAction = "Generating detailed Streamlist...";
				ProgressValue = 0;
				StreamList.Clear();
			});
			foreach (Video vid in videos)
			{
				Streams strm = new()
				{
					title = vid.title,
					episode = vid.episode,
					video_length = vid.video_length,
					created_at = vid.created_at,
					preview_url = vid.preview_url
				};

				string x = (string)jsExecuter.ExecuteAsyncScript(Properties.Resources.fetchPlaylist, strm.episode);
				x = (string)jsExecuter.ExecuteAsyncScript(Properties.Resources.fetchPlaylistDetailed, x.Replace("\"", ""));

				var lines = x.Split('\n');
				var allQualities = Enumerable.Range(0, lines.Length).Where(i => lines[i].Contains("BANDWIDTH") && lines[i].Contains("NAME=")).ToArray();
				for (int i = 0; i < allQualities.Length; i++)
				{
					var line = lines[allQualities[i]];
					var quality = line.Substring(line.LastIndexOf("NAME=\"") + 6, 5).Replace("\"", "");

					if (lines.Length >= i + 1)
					{
						var url = lines[allQualities[i] + 1].Replace("\r", "");
						if (url.StartsWith("https") && url.EndsWith(".m3u8"))
						{
							strm.Qualities.StreamQualities.Add(new() { quality = quality, url = url });
						}
					}
				}

				Dispatcher.Invoke(() =>
				{
					StreamList.Add(strm);
					ProgressValue += Math.Ceiling((double)(100 / videos.Count));
				});
			}
			Dispatcher.Invoke(() =>
			{
				ProgressValue = 100;
			});
		}
		private void frmMain_Loaded(object sender, RoutedEventArgs e)
		{
			InitWebDriver();
			DataContext = this;
			CurrentAction = "Waiting for action...";

			sbiLastUpdate.DataContext = new ElapsedWrapper();

			miReload_Click(sender, e);
		}
		private void frmMain_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Settings.Default.Width = (int)Width;
			Settings.Default.Height = (int)Height;
			Settings.Default.Save();
		}
		private void miReload_Click(object sender, RoutedEventArgs e)
		{
			lvStreams.IsEnabled = false;
			Task t = Task.Factory.StartNew(() =>
			{
				Dispatcher.Invoke(() => sbiCurrentProcess.Foreground = Brushes.Orange);
				Dispatcher.Invoke(() => CurrentAction = "Loading streams...");

				var vids = CollectStreams();
				FillStreamList(vids);

				Settings.Default.lastUpdate = DateTime.Now;
				Settings.Default.Save();

				Dispatcher.Invoke(() =>
				{
					sbiCurrentProcess.Foreground = Brushes.Black;
					CurrentAction = "Waiting for action...";
					ProgressValue = 0;
					lvStreams.IsEnabled = true;
				});

			});
		}
		private void miExit_Click(object sender, RoutedEventArgs e)
		{
			DeInitWebDriver();
			Environment.Exit(0);
		}
		public async void DownloadStream(object sender, RoutedEventArgs e)
		{
			var item = e.Source as MenuItem;

			if (item?.DataContext is Streams stream)
			{
				if (stream.Qualities.StreamQualities.Count <= 0) return;
				SelectQualityDialog sqdialog = new(stream.Qualities.StreamQualities)
				{ Owner = this };

				if (sqdialog.ShowDialog() ?? false)
				{
					string url = QData.SelectedQuality?.url ?? "";
					if (string.IsNullOrWhiteSpace(url)) return;

					string exeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
					string outputDir = Path.Combine(exeDir, "output");
					Directory.CreateDirectory(outputDir);
					string outputFile = Path.Combine(outputDir, stream.episode.ToString() + ".mp4");
					exeDir = Path.Combine(exeDir, "youtubedl", "youtube-dl.exe");
					if (File.Exists(exeDir))
					{
						lvStreams.IsEnabled = false;
						ProcessStartInfo startInfo = new ProcessStartInfo
						{
							Arguments = $"--hls-prefer-native -o \"{outputFile}\" --limit-rate 999G --http-chunk-size 10M " + url,
							FileName = exeDir,
							RedirectStandardOutput = true,
							RedirectStandardError = false,
							UseShellExecute = false,
							CreateNoWindow = true
						};

						using Process process = new() { StartInfo = startInfo };

						process.OutputDataReceived += new((sender, e) =>
						{
							if (e.Data == null) return;

#if DEBUG
							Debug.WriteLine(e.Data);
#endif

							var splittedString = e.Data.Split([' ', '%']).ToList().FindAll(j => !string.IsNullOrWhiteSpace(j));
							if (splittedString.Count <= 0 || splittedString.Count <= 4) return;

							if (splittedString[1] == "100" && splittedString[4] == "in")
							{
								CurrentAction = $"Download finished! Duration: {splittedString[5]}";
								ProgressValue = 0.0;
							}
							else if (splittedString[0] == "[download]" && splittedString[2] == "of")
							{
								if (double.TryParse(splittedString[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double progress))
								{
									ProgressValue = progress;
								}
								if (splittedString.Count == 8)
								{
									if (!string.IsNullOrWhiteSpace(splittedString[7]))
									{
										CurrentAction = $"Downloading stream... {splittedString[3]} {splittedString[6]}: {splittedString[7]}";
									}
								}
							}
						});

						bool killed = false;
						bool didRun = false;
						CancelDownload.Visibility = Visibility.Visible;

						CancelDownload.Click += new((sender, e) =>
						{
							if (didRun) return;
							try
							{
								if (MessageBox.Show("Cancel Stream Download?", "Cancel?", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
								process.Kill();
								didRun = killed = true;

							}
							catch (Exception) { }
						});

						await Task.Run(() =>
						{
							process.Start();
							process.BeginOutputReadLine();
							process.WaitForExit();
						});

						if (killed)
						{
							CancelDownload.Visibility = Visibility.Hidden;
							sbiCurrentProcess.Foreground = Brushes.Black;
							CurrentAction = "Waiting for action...";
							ProgressValue = 0;
							lvStreams.IsEnabled = true;
							return;
						}
						if (MessageBox.Show($"{CurrentAction}\n\nFile downloaded: {outputFile}\n\nOpen Output Folder?", "Download finished!", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
						{
							try
							{
								Process.Start(Path.GetDirectoryName(outputDir));
							}
							catch (Exception) { }
						}
						CancelDownload.Visibility = Visibility.Hidden;

						miReload_Click(sender, e);
					}
				}

			}
			lvStreams.IsEnabled = true;
		}


		private void CurrentDomain_ProcessExit(object? sender, EventArgs e)
		{
			DeInitWebDriver();
		}
		private void InitWebDriver()
		{
			CurrentAction = "Initialize WebDriver...";

			try
			{
				miWebDriverStatus.Foreground = Brushes.Orange;
				miWebDriverStatus.Text = "WebDriver loading";
				var s = DriverFinder.FullPath(new ChromeOptions() { BrowserVersion = "stable" });
				var driverService = ChromeDriverService.CreateDefaultService(s);
				driverService.HideCommandPromptWindow = true;
				var options = new ChromeOptions();
				options.AddArguments("--headless", "--no-sandbox", "--disable-gpu", "--disable-default-apps", "--disable-extensions", "--no-first-run", "--no-default-browser-check");
				webDriver = new ChromeDriver(driverService, options);
			}
			catch (Exception)
			{ }

			miWebDriverStatus.Foreground = Brushes.Green;
			miWebDriverStatus.Text = "WebDriver loaded";
		}
		private void DeInitWebDriver()
		{
			try
			{
				if (webDriver != null)
				{
					webDriver.Close();
					webDriver.Quit();
				}
			}
			catch (Exception)
			{ }
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{

		}

		public void WatchStream(object sender, RoutedEventArgs e)
		{
			var item = e.Source as MenuItem;
			if (item?.DataContext is Streams stream)
			{
				if (stream.Qualities.StreamQualities.Count > 0)
				{
					SelectQualityDialog sqdialog = new(stream.Qualities.StreamQualities);
					sqdialog.btnSelectQuality.Content = "Play";
					if (sqdialog.ShowDialog() ?? false)
					{
						string url = QData.SelectedQuality?.url ?? "";
						if (!string.IsNullOrWhiteSpace(url))
						{
							Player p = new();
							p.Show();
							p.Play(url);

							//var exedir = AppDomain.CurrentDomain.BaseDirectory;
							//var css = File.ReadAllText($"{exedir.Replace("\\", "/")}classes/video-js.css");
							//var js1 = File.ReadAllText($"{exedir.Replace("\\", "/")}classes/video.js");
							//var js2 = File.ReadAllText($"{exedir.Replace("\\", "/")}classes/videohls.js");
							//string html = "<html><head>";
							//html += "<meta content='IE=edge' http-equiv='X-UA-Compatible'/>";
							//html += $"<style>{css}</style>";
							//html += $"<script>{js1}</script>";
							//html += $"<script>{js2}</script>";
							//html += "<video id='my_video' class='video-js vjs-fluid vjs-default-skin' controls preload='auto' data-setup='{}' autoplay>";
							//html += "<source src='" + url.Trim() + "' type='application/x-mpegURL'>";
							//html += "</video>";
							//html += "<script>";
							//html += "var player = videojs('my_video');";
							//html += "player.play();";
							//html += "</script>";
							//html += "</body></html>";

							//System.Windows.Forms.WebBrowser wb = new()
							//{
							//	Dock = DockStyle.Fill,
							//	DocumentText = html,
							//	AllowNavigation = false,
							//	ScriptErrorsSuppressed = false
							//};

							//Form player = new()
							//{
							//	Text = stream.title
							//};
							//player.Controls.Add(_player);
							//player.Show();

							//html = js1 = js2 = css = "";
						}
					}
				}
			}
			lvStreams.IsEnabled = true;
		}
	}
}