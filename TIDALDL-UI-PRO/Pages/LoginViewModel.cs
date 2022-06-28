using AIGS.Common;
using AIGS.Helper;
using HandyControl.Controls;
using Stylet;
using System;
using System.Threading.Tasks;
using TIDALDL_UI.Else;
using TidalLib;

namespace TIDALDL_UI.Pages
{
    public class LoginViewModel : ModelBase
    {
        public bool BtnLoginEnable { get; set; } = true;
        public bool HaveInit { get; set; } = false;
        public TidalDeviceCode DeviceCode { get; set; }
        public UserSettings Settings { get; set; } = UserSettings.Read();
        private IWindowManager Manager;
        private MainViewModel VMMain;

        public LoginViewModel(IWindowManager manager, MainViewModel vmmain)
        {
            Manager = manager;
            VMMain  = vmmain;
            VMMain.VMLogin = this;
            return;
        }

        protected override async void OnViewLoaded()
        {
            if (HaveInit)
                return;
            HaveInit = true;

            BtnLoginEnable = false;

            SaveProxy();

            //Auto login by accessToken
            if (Settings.Accesstoken.IsNotBlank())
            {
                try
                {
                    var key = await Global.Client.Login(Settings.Accesstoken);
                    goto LOGIN_SUCCESS;
                }
                catch (Exception)
                {
                }
            }

            if (Settings.Refreshtoken.IsNotBlank())
            {
                try
                {
                    var key = await Global.Client.RefreshAccessToken(Settings.Refreshtoken);
                    Settings.Userid = key.UserID;
                    Settings.Accesstoken = key.AccessToken;
                    Settings.Save();
                    goto LOGIN_SUCCESS;
                }
                catch (Exception)
                {
                }
            }

            //get device code
            try
            {
                DeviceCode = await Global.Client.GetDeviceCode();
            }
            catch (Exception)
            {
                Growl.Error(Language.Get("strmsgGetDeviceCodeFailed"), Global.TOKEN_LOGIN);
                BtnLoginEnable = true;
                return;
            }


        LOGIN_SUCCESS:
            Manager.ShowWindow(VMMain);
            RequestClose();
            BtnLoginEnable = true;
            return;
        }

        public async void Login()
        {
            BtnLoginEnable = false;

            SaveProxy();

            try
            {
                DeviceCode = await Global.Client.GetDeviceCode();
            }
            catch (Exception)
            {
                Growl.Error(Language.Get("strmsgGetDeviceCodeFailed"), Global.TOKEN_LOGIN);
                BtnLoginEnable = true;
            }

            ThreadHelper.Start(CheckAuthThreadFunc);
            return;
        }

        public void SaveProxy()
        {
            if (Global.Client == null)
                Global.Client = new Client();
            Global.Client.proxy = null;
            if (Settings.ProxyEnable)
                Global.Client.proxy = new HttpHelper.ProxyInfo(Settings.ProxyHost, 
                                                                Settings.ProxyPort, 
                                                                Settings.ProxyUser, 
                                                                Settings.ProxyPwd);
            Settings.Save();
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


        public void CheckAuthThreadFunc(object[] datas)
        {
            NetHelper.OpenWeb($"https://{DeviceCode.VerificationUri}/{DeviceCode.UserCode}");
            
            SaveProxy();

            try
            {
                var key = Global.Client.CheckAuthStatus(DeviceCode).Result;
                Settings.Userid = key.UserID;
                Settings.Countrycode = key.CountryCode;
                Settings.Accesstoken = key.AccessToken;
                Settings.Refreshtoken = key.RefreshToken;
                Settings.Save();

                this.View.Dispatcher.Invoke(new Action(() => {
                    Manager.ShowWindow(VMMain);
                    RequestClose();
                }));
            }
            catch (Exception e)
            {
                Growl.Error(e.ToString(), Global.TOKEN_LOGIN);
            }

            BtnLoginEnable = true;
        }

    }



}
