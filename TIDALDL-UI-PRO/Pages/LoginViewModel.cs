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

            //Login
            (string msg, LoginKey key) = await Client.Login(Settings.Username, Settings.Password, null, PROXY);
            (string msg2, LoginKey key2) = await Client.Login(Settings.Accesstoken, PROXY);
            (string msg3, LoginKey key3) = await Client.Login(Settings.Username, Settings.Password, "_DSTon1kC8pABnTw", PROXY);
            if (msg.IsNotBlank() || key == null)
            {
                Growl.Error("Login Err! " + msg, Global.TOKEN_LOGIN);
                goto RETURN_POINT;
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
    }



}
