using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using AIGS.Common;
using AIGS.Helper;
using Stylet;
using TIDALDL_UI.Else;

namespace TIDALDL_UI.Pages
{
    public class AboutViewModel : ModelBase
    {
        public string Type { get; set; } = "(BETA)";
        public Visibility ShowDonate { get; set; } = Visibility.Collapsed;
        public string Version { get; set; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();


        public void Feedback() => NetHelper.OpenWeb(Global.URL_TIDAL_ISSUES);
        public void Telegram() => NetHelper.OpenWeb(Global.URL_TIDAL_GROUP);
        public void ClickPaypal() => NetHelper.OpenWeb(Global.URL_PAYPAL);
        public void ClickBuymeacoffee() => NetHelper.OpenWeb(Global.URL_PAYPAL);
        public void Donate()
        {
            if (ShowDonate == Visibility.Collapsed)
                ShowDonate = Visibility.Visible;
            else
                ShowDonate = Visibility.Collapsed;
        }

        public void WindowClose() => this.ViewVisibility = Visibility.Hidden;
    }
}
