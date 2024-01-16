
using GronkhTV_DL.classes;
using static GronkhTV_DL.classes.Globals;
using System.Windows;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;

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
            InitWebDriver();
            DataContext = this;
        }

        private void CurrentDomain_ProcessExit(object? sender, EventArgs e)
        {
            try
            {
                webDriver.Close();
                webDriver.Quit();
            }
            catch (Exception)
            {}
        }
        private void InitWebDriver()
        {
            try
            {
                var s = DriverFinder.FullPath(new ChromeOptions() { BrowserVersion = "stable" });
                var driverService = ChromeDriverService.CreateDefaultService(s);
                driverService.HideCommandPromptWindow = true;
                var options = new ChromeOptions();
                options.AddArguments("--headless", "--no-sandbox", "--disable-gpu", "disable-gpu");
                webDriver = new ChromeDriver(driverService,options);
            }
            catch (Exception)
            { }
        }
        private List<Video> CollectStreams()
        {
            try
            {
                StreamsLoaded = false;

                webDriver.Navigate().GoToUrl("https://gronkh.tv/streams/");
                jsExecuter = (IJavaScriptExecutor)webDriver;

                string x = (string)jsExecuter.ExecuteAsyncScript(Properties.Resources.fetchStreams);

                Root? root = JsonConvert.DeserializeObject<Root>(x);
                if (root == null) return [];

                List<Video>? videos = root.results?.videos?.OrderBy(v => v.episode).Reverse().ToList();
                if (videos == null) return [];

                StreamsLoaded = true;
                return videos;
            }
            catch (Exception)
            {
                return [];
            }
        }

        private void miReload_Click(object sender, RoutedEventArgs e)
        {

        }

        private void miExit_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}