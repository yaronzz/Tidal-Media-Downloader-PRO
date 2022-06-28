using AIGS.Common;
using TIDALDL_UI.Pages;

namespace TIDALDL_UI.Else
{
    public class Global : ViewMoudleBase
    {
        public static MainViewModel VMMain { get; set; }
        public static Settings Settings { get; set; }
        public static TidalLib.Client Client { get; set; }

        //url
        public static string URL_TIDAL_GROUP = "https://t.me/tidal_group";
        public static string URL_TIDAL_GITHUB = "https://github.com/yaronzz/Tidal-Media-Downloader-PRO";
        public static string URL_TIDAL_ISSUES = "https://github.com/yaronzz/Tidal-Media-Downloader-PRO/issues";
        public static string URL_PAYPAL = "https://www.paypal.com/paypalme/yaronzz";
        public static string URL_BUYMEACOFFEE = "https://www.buymeacoffee.com/yaronzz";

        //update
        public static string NAME_GITHUB_AUTHOR = "yaronzz";
        public static string NAME_GITHUB_PROJECT = "Tidal-Media-Downloader-PRO";
        public static string NAME_GITHUB_FILE = "tidal-gui.exe";
        public static string KEY_BASE = "fa^8jk@j9d";
            
        //Token
        public static string TOKEN_MAIN = "MainToken";
        public static string TOKEN_LOGIN = "LoginToken";


        //Global DynamicResource
        //public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
    }

}
