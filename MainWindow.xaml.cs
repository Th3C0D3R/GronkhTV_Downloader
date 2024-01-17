using GronkhTV_DL.classes;
using static GronkhTV_DL.classes.Globals;
using System.Windows;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace GronkhTV_DL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
        }
        private List<Video> CollectStreams()
        {
            Dispatcher.Invoke(() =>
            {
                sbiCurrentProcess.Content = "Loading Website...";
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
                    sbiCurrentProcess.Content = "Deserialize result...";
                    pbProgress.Value = 35;
                });

                Root? root = JsonConvert.DeserializeObject<Root>(x);
                if (root == null) return [];

                Dispatcher.Invoke(() =>
                {
                    sbiCurrentProcess.Content = "Ordering streamlist";
                    pbProgress.Value = 70;
                });

                List<Video>? videos = root.results?.videos?.OrderBy(v => v.episode).Reverse().ToList();
                if (videos == null) return [];

                StreamsLoaded = true;
                Dispatcher.Invoke(() =>
                {
                    sbiCurrentProcess.Content = "Finish loading streams!";
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
                sbiCurrentProcess.Content = "Generating detailed Streamlist...";
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
                    progressText.Text = $"{pbProgress.Value}%";
                });
            }
            Dispatcher.Invoke(() =>
            {
                pbProgress.Value = 100;
                progressText.Text = $"{pbProgress.Value}%";
            });
        }
        private void frmMain_Loaded(object sender, RoutedEventArgs e)
        {
            InitWebDriver();
            DataContext = this;
            sbiCurrentProcess.Content = "Waiting for action...";
            Task t = Task.Factory.StartNew(() =>
            {
                Dispatcher.Invoke(() => sbiCurrentProcess.Foreground = Brushes.Orange);
                Dispatcher.Invoke(() => sbiCurrentProcess.Content = "Loading streams...");

                var vids = CollectStreams();
                FillStreamList(vids);

                Dispatcher.Invoke(() => sbiCurrentProcess.Foreground = Brushes.Black);
                Dispatcher.Invoke(() => sbiCurrentProcess.Content = "Waiting for action...");
                Dispatcher.Invoke(() => AutoResizeColumns(lvStreams)); 
                Dispatcher.Invoke(() =>
                {
                    AutoResizeColumns(lvStreams);
                    sbiCurrentProcess.Foreground = Brushes.Black;
                    sbiCurrentProcess.Content = "Waiting for action...";
                    pbProgress.Value = 0;
                    progressText.Text = $"{pbProgress.Value}%";
                });

            });
        }
        private void miReload_Click(object sender, RoutedEventArgs e)
        {

        }
        private void miExit_Click(object sender, RoutedEventArgs e)
        {
            DeInitWebDriver();
            Environment.Exit(0);
        }

        private void AutoResizeColumns(ListView listView)
        {
            GridView gridView = listView.View as GridView;

            if (gridView != null)
            {
                foreach (var column in gridView.Columns)
                {
                    // Set column width to NaN and then measure it to auto-adjust
                    column.Width = double.NaN;
                    column.Width = column.ActualWidth;
                }
            }
        }
        private void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            DeInitWebDriver();
        }
        private void InitWebDriver()
        {
            sbiCurrentProcess.Content = "Initialize WebDriver...";

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