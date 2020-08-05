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

        public List<ePositionYear> ComboxAddYear { get; set; } = AIGS.Common.Convert.ConverEnumToList<ePositionYear>();
        public List<Else.Theme.Type> ComboxTheme { get; set; } = AIGS.Common.Convert.ConverEnumToList<Else.Theme.Type>();

        public void Load()
        {
            Settings = Settings.Read();
            AccessToken = UserSettings.Read().Accesstoken;
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
                    Growl.Warning("Accesstoken is not valid! " + msg, Global.TOKEN_MAIN);
                else
                {
                    UserSettings user = UserSettings.Read();
                    user.Accesstoken = AccessToken;
                    user.Save();
                    Global.AccessKey = key;
                }
            }

            Settings.Change(Settings, Global.Settings);

            Global.Settings = Settings;
            Global.Settings.Save();

            Load();
            Growl.Success("Refresh settings success!", Global.TOKEN_MAIN);
        }

        public void Logout()
        {
            Global.VMMain.Logout();
        }
    }
}
