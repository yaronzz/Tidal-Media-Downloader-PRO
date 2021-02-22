using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using AIGS.Common;
using AIGS.Helper;
using HandyControl.Controls;
using HandyControl.Themes;
using Stylet;
using TIDALDL_UI.Else;
using TidalLib;

namespace TIDALDL_UI.Pages
{
    public class SettingsViewModel : ModelBase
    {
        public Settings Settings { get; set; }
        public string AccessToken { get; set; } 
        public string UserID { get; set; }

        public List<ePositionYear> ComboxAddYear { get; set; } = AIGS.Common.Convert.ConverEnumToList<ePositionYear>();
        public List<Else.Theme.Type> ComboxTheme { get; set; } = AIGS.Common.Convert.ConverEnumToList<Else.Theme.Type>();
        public List<Else.Language.Type> ComboxLanguage { get; set; } = AIGS.Common.Convert.ConverEnumToList<Else.Language.Type>();


        public void Load()
        {
            Settings = Settings.Read();

            UserSettings user = UserSettings.Read();
            AccessToken = user.Accesstoken;
            UserID = user.Userid;
        }

        public void SetOutputDir()
        {
            FolderBrowserDialog openFileDialog = new FolderBrowserDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                Settings.OutputDir = openFileDialog.SelectedPath;
        }

        public async void Confim()
        {
            if (AccessToken.IsNotBlank())
            {
                (string msg, LoginKey key) = await Client.Login(AccessToken);
                if (msg.IsNotBlank() || key == null)
                    Growl.Warning(Language.Get("strmsgAccessTokenIsNotValid") + msg, Global.TOKEN_MAIN);
                else if(key.UserID != Global.CommonKey.UserID)
                    Growl.Warning(Language.Get("strmsgUserMismatch"), Global.TOKEN_MAIN);
                else
                {
                    UserSettings user = UserSettings.Read();
                    user.Accesstoken = AccessToken;
                    user.Save();
                    Global.AccessKey = key;
                }
            }
            else
            {
                UserSettings user = UserSettings.Read();
                user.Accesstoken = null;
                user.Save();
                Global.AccessKey = null;
            }

            Settings.Change(Settings, Global.Settings);

            Global.Settings = Settings;
            Global.Settings.Save();

            Load();
            Growl.Success(Language.Get("strmsgRefreshSettingsSuccess"), Global.TOKEN_MAIN);
        }

        public void Logout()
        {
            Global.VMMain.Logout();
        }

        public void GetAccessToken()
        {
            
            (string msg, LoginKey key) = TidalLib.Client.GetAccessTokenFromTidalDesktop(UserID);
            if (msg.IsNotBlank())
            {
                Growl.Warning(msg, Global.TOKEN_MAIN);
                return;
            }

            Growl.Success(Language.Get("strmsgGetAccessTokenSuccess"), Global.TOKEN_MAIN);
            AccessToken = key.AccessToken;
        }
    }
}
