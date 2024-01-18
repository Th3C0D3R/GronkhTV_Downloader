using GronkhTV_DL.Properties;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GronkhTV_DL.classes
{
    public static class Globals
    {
        public static IWebDriver webDriver;
        public static IJavaScriptExecutor jsExecuter;

        public static bool StreamsLoaded = false;
        public static bool SingleStreamLoaded = false;

        private static readonly ObservableCollection<Streams> streamList = [];
        public static ObservableCollection<Streams> StreamList
        {
            get { return streamList; }
        }
        public static Streams SelectedStream = new();

        public static TimeSpan Elapsed 
        { 
            get {
                return DateTime.Now - Settings.Default.lastUpdate;
            } 
        }

        public static int SelVid = -1;

        public static TimeSpan GetUpdateInterval(TimeSpan updateInterval)
        {
            if(updateInterval.Minutes is > 0 and < 60) return TimeSpan.FromMinutes(1);
            else if(updateInterval.Hours is > 0 and < 24) return TimeSpan.FromHours(1);
            else if(updateInterval.Days is > 0 and < 365) return TimeSpan.FromDays(1);
            else return TimeSpan.FromSeconds(1);
        }
        public static string FormatElapsedTime(TimeSpan elapsed)
        {
            string[] labels = ["day", "hour", "minute", "second"];
            int[] values = [elapsed.Days, elapsed.Hours, elapsed.Minutes, elapsed.Seconds];

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > 0)
                {
                    return $"{values[i]} {labels[i]}{(values[i] > 1 ? "s" : "")}";
                }
            }

            return "0 seconds";
        }

    }

    public class ElapsedWrapper : INotifyPropertyChanged
    {
        private static TimeSpan _elapsed;

        public TimeSpan Elapsed
        {
            get
            {
                return _elapsed;
            }
            set
            {
                if (_elapsed != value)
                {
                    _elapsed = value;
                    OnPropertyChanged(nameof(Elapsed));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Singleton pattern
        private static ElapsedWrapper _instance;

        public static ElapsedWrapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ElapsedWrapper();
                }
                return _instance;
            }
        }
        public ElapsedWrapper()
        {
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(1); // Update every second
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Elapsed = DateTime.Now - Settings.Default.lastUpdate;
        }
    }

}
