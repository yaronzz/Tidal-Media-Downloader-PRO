using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using AIGS.Common;
using AIGS.Helper;
using Stylet;
using Tidal;
using TIDALDL_UI.Else;

namespace TIDALDL_UI.Pages
{
    public class AboutViewModel : Screen
    {
        public string Version { get; set; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public void GotoProject()
        {
            NetHelper.OpenWeb("https://github.com/yaronzz/Tidal-Media-Downloader");
        }
    }
}
