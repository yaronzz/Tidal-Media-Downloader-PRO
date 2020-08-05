﻿using AIGS.Common;
using AIGS.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TIDALDL_UI.Pages;
using TidalLib;

namespace TIDALDL_UI.Else
{
    public class Global : ViewMoudleBase
    {
        public static MainViewModel VMMain { get; set; }
        public static Settings Settings { get; set; }

        public static LoginKey CommonKey { get; set; }
        public static LoginKey VideoKey { get; set; }
        public static LoginKey AccessKey { get; set; }

        //Path
        public static string PATH_BASE = SystemHelper.GetUserFolders().PersonalPath + "\\Tidal-gui\\data\\";
        public static string PATH_SETTINGS = PATH_BASE + "settings.json";
        public static string PATH_USERSETTINGS = PATH_BASE + "usersettings.json";

        //url
        public static string URL_TIDAL_GROUP = "https://t.me/tidal_group";
        public static string URL_TIDAL_GITHUB = "https://github.com/yaronzz/Tidal-Media-Downloader-PRO";
        public static string URL_TIDAL_ISSUES = "https://github.com/yaronzz/Tidal-Media-Downloader-PRO/issues";
        public static string URL_PAYPAL = "https://www.paypal.com/paypalme/yaronzz";
        public static string URL_BUYMEACOFFEE = "https://www.buymeacoffee.com/yaronzz";

        //Token
        public static string TOKEN_MAIN = "MainToken";
        public static string TOKEN_LOGIN = "LoginToken";


        //Global DynamicResource
        //public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;

    }

}
