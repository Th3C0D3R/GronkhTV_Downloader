using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        public static int SelVid = -1;

    }

}
