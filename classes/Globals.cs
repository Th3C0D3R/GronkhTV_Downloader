using OpenQA.Selenium;
using System;
using System.Collections.Generic;
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

        public static List<Streams> StreamList = new();
        public static Streams SelectedStream = new();


        public static int SelVid = -1;

    }
}
