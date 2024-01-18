using GronkhTV_DL.classes;
using static GronkhTV_DL.classes.Globals;
using System.Windows;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using Path = System.IO.Path;
using System;
using System.Reflection;
using GronkhTV_DL.dialog;
using GronkhTV_DL.dialog.classes;
using System.Windows.Threading;
using GronkhTV_DL.Properties;
using System.Runtime.InteropServices;

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
                sbiCurrentProcess.Text = "Loading Website...";
                pbProgress.Value = 0;
            });
            try
            {
                StreamsLoaded = false;

                webDriver.Navigate().GoToUrl("https://gronkh.tv/streams/");
                jsExecuter = (IJavaScriptExecutor)webDriver;

                string x = (string)jsExecuter.ExecuteAsyncScript(Properties.Resources.fetchStreams);

                Dispatcher.Invoke(() =>
                {
                    sbiCurrentProcess.Text = "Deserialize result...";
                    pbProgress.Value = 35;
                });

                Root? root = JsonConvert.DeserializeObject<Root>(x);
                if (root == null) return [];

                Dispatcher.Invoke(() =>
                {
                    sbiCurrentProcess.Text = "Ordering streamlist";
                    pbProgress.Value = 70;
                });

                List<Video>? videos = root.results?.videos?.OrderBy(v => v.episode).Reverse().ToList();
                if (videos == null) return [];

                StreamsLoaded = true;
                Dispatcher.Invoke(() =>
                {
                    sbiCurrentProcess.Text = "Finish loading streams!";
                    sbiCurrentProcess.Foreground = Brushes.Green;
                    pbProgress.Value = 100;
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
                sbiCurrentProcess.Text = "Generating detailed Streamlist...";
                pbProgress.Value = 0;
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
                    pbProgress.Value += Math.Ceiling((double)(100 / videos.Count));
                });
            }
            Dispatcher.Invoke(() =>
            {
                pbProgress.Value = 100;
            });
        }
        private void frmMain_Loaded(object sender, RoutedEventArgs e)
        {
            InitWebDriver();
            DataContext = this;
            sbiCurrentProcess.Text = "Waiting for action...";

            sbiLastUpdate.DataContext = new ElapsedWrapper();

            Task t = Task.Factory.StartNew(() =>
            {
                Dispatcher.Invoke(() => sbiCurrentProcess.Foreground = Brushes.Orange);
                Dispatcher.Invoke(() => sbiCurrentProcess.Text = "Loading streams...");

                var vids = CollectStreams();
                FillStreamList(vids);

                Settings.Default.lastUpdate = DateTime.Now;
                Settings.Default.Save();

                Dispatcher.Invoke(() =>
                {
                    sbiCurrentProcess.Foreground = Brushes.Black;
                    sbiCurrentProcess.Text = "Waiting for action...";
                    pbProgress.Value = 0;
                });

            });
        }
        private void frmMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Settings.Default.Width = (int)Width;
            Settings.Default.Height = (int)Height;
            Settings.Default.Save();
        }
        private void miReload_Click(object sender, RoutedEventArgs e)
        {

        }
        private void miExit_Click(object sender, RoutedEventArgs e)
        {
            DeInitWebDriver();
            Environment.Exit(0);
        }
        public void DownloadStream(object sender, RoutedEventArgs e)
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
                    outputDir = Path.Combine(outputDir, stream.episode.ToString() + ".mp4");
                    exeDir = Path.Combine(exeDir, "youtubedl", "youtube-dl.exe");
                    if (File.Exists(exeDir))
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo
                        {
                            Arguments = $"--hls-prefer-native -o \"{outputDir}\" --limit-rate 999G --http-chunk-size 10M " + url,
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

                            var splittedString = e.Data.Split([' ', '%']).ToList().FindAll(j => !string.IsNullOrWhiteSpace(j));
                            if (splittedString.Count <= 0 || splittedString.Count <= 4) return;

                            if (splittedString[1] == "100" && splittedString[4] == "in")
                            {
                                sbiCurrentProcess.Dispatcher.Invoke(() => sbiCurrentProcess.Text = $"Download finished! Duration: {splittedString[5]}");
                                pbProgress.Dispatcher.Invoke(() => pbProgress.Value = 0);
                            }
                            else if (splittedString[0] == "[download]" && splittedString[2] == "of")
                            {
                                if (double.TryParse(splittedString[1], out double progress))
                                {
                                    pbProgress.Dispatcher.Invoke(() => pbProgress.Value = progress);

                                }
                                if (splittedString.Count == 8)
                                {
                                    if (!string.IsNullOrWhiteSpace(splittedString[7]))
                                    {
                                        sbiCurrentProcess.Dispatcher.Invoke(() => sbiCurrentProcess.Text = $"Downloading stream... {splittedString[6]}: {splittedString[7]}");
                                    }
                                }
                            }
                        });
                        //process.ErrorDataReceived += ReceiveData;

                        process.Start();
                        process.BeginOutputReadLine();
                    }
                }

            }
        }
        private void Window_Closed(object sender, EventArgs e)
        {

        }


        private void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            DeInitWebDriver();
        }
        private void InitWebDriver()
        {
            sbiCurrentProcess.Text = "Initialize WebDriver...";

            try
            {
                miWebDriverStatus.Foreground = Brushes.Orange;
                miWebDriverStatus.Text = "WebDriver loading";
                var s = DriverFinder.FullPath(new ChromeOptions() { BrowserVersion = "120" });
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

    }
}