
using GronkhTV_DL.classes;
using static GronkhTV_DL.classes.Globals;
using System.Windows;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Newtonsoft.Json;
using System.Windows.Media;

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


		private void CurrentDomain_ProcessExit(object? sender, EventArgs e)
		{
			DeInitWebDriver();
		}
		private void InitWebDriver()
		{
			sbiCurrentProcess.Content = "Initialize WebDriver...";
			try
			{
				var s = DriverFinder.FullPath(new ChromeOptions() { BrowserVersion = "stable" });
				var driverService = ChromeDriverService.CreateDefaultService(s);
				driverService.HideCommandPromptWindow = true;
				var options = new ChromeOptions();
				options.AddArguments("--headless", "--no-sandbox", "--disable-gpu", "disable-gpu");
				webDriver = new ChromeDriver(driverService, options);
				miWebDriverStatus.Foreground = Brushes.Green;
				miWebDriverStatus.Text = "WebDriver loaded";
			}
			catch (Exception)
			{ }
		}
		private void DeInitWebDriver()
		{
			try
			{
				webDriver.Close();
				webDriver.Quit();
			}
			catch (Exception)
			{ }
		}
		private void miExit_Click(object sender, RoutedEventArgs e)
		{
			DeInitWebDriver();
			Environment.Exit(0);
		}

		private void frmMain_Loaded(object sender, RoutedEventArgs e)
		{
			InitWebDriver();
			DataContext = this;
			sbiCurrentProcess.Content = "Waiting for action...";
		}
	}
}