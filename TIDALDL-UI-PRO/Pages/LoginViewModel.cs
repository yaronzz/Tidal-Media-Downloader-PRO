using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using AIGS.Common;
using HandyControl.Tools;
using AIGS.Helper;
using Stylet;
using TIDALDL_UI.Else;
using HandyControl.Controls;
using TidalLib;

namespace TIDALDL_UI.Pages
{
    public class LoginViewModel : ModelBase
    {
        public bool BtnLoginEnable { get; set; } = true;
        public UserSettings Settings { get; set; } = UserSettings.Read();
        private IWindowManager Manager;
        private MainViewModel VMMain;

        public LoginViewModel(IWindowManager manager, MainViewModel vmmain)
        {
            Manager = manager;
            VMMain  = vmmain;
            VMMain.VMLogin = this;

            //If AutoLogin
            if (Settings.AutoLogin && Settings.Username.IsNotBlank() && Settings.Password.IsNotBlank())
                Login();
            return;
        }

        public async void Login()
        {
            BtnLoginEnable = false;

            if (Settings.Username.IsBlank() || Settings.Password.IsBlank())
            {
                Growl.Error("Username or password is err!", Global.TOKEN_LOGIN);
                goto RETURN_POINT;
            }

            //Proxy
            HttpHelper.ProxyInfo PROXY = Settings.ProxyEnable ? new HttpHelper.ProxyInfo(Settings.ProxyHost, Settings.ProxyPort, Settings.ProxyUser, Settings.ProxyPwd) : null;

            //token
            (string token1, string token2) = await GetToken();

            //Login (lossless key \ video key)
            (string msg, LoginKey key)   = await Client.Login(Settings.Username, Settings.Password, token1, PROXY);
            (string msg3, LoginKey key3) = await Client.Login(Settings.Username, Settings.Password, token2, PROXY);
            if (msg.IsNotBlank() || key == null)
            {
                Growl.Error("Login Err! " + msg, Global.TOKEN_LOGIN);
                goto RETURN_POINT;
            }

            //Auto get accesstoken(master key)
            string printSuccess = null;
            string printWarning = null;
            (string msg2, LoginKey key2) = Client.GetAccessTokenFromTidalDesktop(key.UserID);
            if (key2 != null && msg2.IsBlank() && key2.AccessToken != Settings.Accesstoken)
            {
                (msg2, key2) = await Client.Login(key2.AccessToken, PROXY);
                if (msg2.IsBlank() && key2 != null)
                {
                    printSuccess = "Auto get accesstoken success!";
                    Settings.Accesstoken = key2.AccessToken;
                }
            }
            else
                key2 = null;

            if (key2 == null && Settings.Accesstoken.IsNotBlank())
            {
                (msg2, key2) = await Client.Login(Settings.Accesstoken, PROXY);
                if (msg2.IsNotBlank() || key2 == null)
                    printWarning = "Accesstoken is not valid! " + msg;
            }
            
            if (!Settings.Remember)
                Settings.Password = null;
            Settings.Userid      = key.UserID;
            Settings.Sessionid1  = key.SessionID;
            Settings.Accesstoken = Settings.Accesstoken;
            Settings.Save();
            Global.CommonKey = key;
            Global.VideoKey = key3;
            Global.AccessKey = key2;

            Manager.ShowWindow(VMMain);
            if (printSuccess.IsNotBlank())
                Growl.Success(printSuccess, Global.TOKEN_MAIN);
            else if (printWarning.IsNotBlank())
                Growl.Warning(printWarning, Global.TOKEN_MAIN);

            RequestClose();

        RETURN_POINT:
            BtnLoginEnable = true;
            return;
        }

        public void WindowMove()
        {
            ((LoginView)this.View).DragMove();
        }

        public void WindowClose()
        {
            ThreadTool.Close();
            RequestClose();
        }


        public async Task<(string, string)> GetToken()
        {
            try
            {
                HttpHelper.Result result = await HttpHelper.GetOrPostAsync("https://cdn.jsdelivr.net/gh/yaronzz/CDN@latest/app/tidal/token.json");
                if (result.sData.IsNotBlank())
                {
                    string token = JsonHelper.GetValue(result.sData, "token");
                    string token2 = JsonHelper.GetValue(result.sData, "token2");
                    return (token, token2);
                }
            }
            catch { }
            return Client.GetDefaultToken();
        }
    }



}
