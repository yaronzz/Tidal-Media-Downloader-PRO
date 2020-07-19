using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using AIGS.Common;
using HandyControl.Tools;
using AIGS.Helper;
using Stylet;
using Tidal;
using TIDALDL_UI.Else;

namespace TIDALDL_UI.Pages
{
    public class LoginViewModel : Screen
    {
        public string Errlabel { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Remember { get; set; }
        public bool AutoLogin { get; set; }
        public bool BtnLoginEnable { get; set; } = true;

        //Key
        public Visibility ShowKeyView { get; set; } = Visibility.Visible;
        public string Key { get; set; }
        public string[] KeyArray { get; set; }

        /// <summary>
        /// Proxy
        /// </summary>
        public bool ProxyEnable { get; set; } = Config.ProxyEnable();
        public string ProxyHost { get; set; } = Config.ProxyHost();
        public int ProxyPort { get; set; } = Config.ProxyPort();
        public string ProxyUser { get; set; } = Config.ProxyUser();
        public string ProxyPwd { get; set; } = Config.ProxyPwd();

        /// <summary>
        /// View
        /// </summary>
        private IWindowManager Manager;
        private MainViewModel VMMain;

        public LoginViewModel(IWindowManager manager, MainViewModel vmmain)
        {
            Manager = manager;
            VMMain = vmmain;
            Key = Config.Key();
            Remember = Config.Remember();
            AutoLogin = Config.AutoLogin();
            Username = Config.Username();
            Password = Config.Password();
            HandyControl.Tools.ConfigHelper.Instance.SetLang("en");
            
            //Check key
            updateKey();
            CheckKey();

            //If AutoLogin
            if (AutoLogin && Username.IsNotBlank() && Password.IsNotBlank())
                Login();
            return;
        }

        public void CheckKey()
        {
            Errlabel = "";
            if (Key.IsBlank())
                return;
            foreach (var item in KeyArray)
            {
                if(Key.Trim().ToLower() == item.Trim().ToLower())
                {
                    ShowKeyView = Visibility.Hidden;
                    Config.Key(Key);
                    return;
                }
            }
            Errlabel = "Key is err : " + Key;
        }

        public async void Login()
        {
            if (ShowKeyView == Visibility.Visible)
                return;

            Errlabel = "";
            if (Username.IsBlank() || Password.IsBlank())
            {
                Errlabel = "Username or password is err!";
                return;
            }
            BtnLoginEnable = false;

            //Proxy
            TidalTool.PROXY = ProxyEnable ? new HttpHelper.ProxyInfo(ProxyHost, ProxyPort, ProxyUser, ProxyPwd) : null;
            Config.ProxyEnable(ProxyEnable.ToString());
            Config.ProxyHost(ProxyHost);
            Config.ProxyPort(ProxyPort.ToString());
            Config.ProxyUser(ProxyUser);
            Config.ProxyPwd(ProxyPwd);

            //Login
            bool bRet = await Task.Run(() => { return TidalTool.login(Username, Password); });
            if (!bRet)
            {
                Errlabel = "Login Err! " + TidalTool.loginErrlabel;
                BtnLoginEnable = true;
                return;
            }

            if (Remember)
            {
                Config.Username(Username);
                Config.Password(Password);
            }
            BtnLoginEnable = true;

            VMMain.VMLogin = this;
            Manager.ShowWindow(VMMain);
            RequestClose();
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


        private void updateKey()
        {
            string sUrl = "https://onedrive.gimhoy.com/1drv/aHR0cHM6Ly8xZHJ2Lm1zL3QvcyFBc3h5VUd1Q0w4SGFncUpKck5xOEpNemFUanh5YlE/ZT1Ualk2MzQ=";
            try
            {
                string sErrmsg;
                string sReturn = (string)HttpHelper.GetOrPost(sUrl, out sErrmsg, IsErrResponse: true, Timeout: 10 * 1000);
                if (sReturn.IsNotBlank())
                {
                    KeyArray = sReturn.Split("\r\n");
                    Config.KeyArray(sReturn);
                    return;
                }
            }
            catch { }
            
            string sRet = Config.KeyArray();
            if (sRet != null)
                KeyArray = sRet.Split("\r\n");
            else
                KeyArray = null;
        }
    }



}
