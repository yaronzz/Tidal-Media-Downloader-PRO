using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AIGS.Common;
using AIGS.Helper;
using HandyControl.Controls;
using Stylet;
using Tidal;
using TIDALDL_UI.Else;

namespace TIDALDL_UI.Pages
{
    public class MainViewModel : Screen
    {
        public bool ShowViewSearch { get; set; } = true;
        public bool ShowViewDownload { get; set; }
        public bool ShowViewSettings { get; set; }

        private IWindowManager Manager;
        public LoginViewModel VMLogin { get; set; }
        public SearchViewModel VMSearch { get; private set; }
        public AboutViewModel VMAbout { get; private set; }
        public SettingsViewModel VMSettings { get; private set; }
        public DownloadViewModel VMDownload { get; private set; }

        public MainViewModel(IWindowManager manager, SearchViewModel search, AboutViewModel about, SettingsViewModel settings, DownloadViewModel download)
        {
            Manager = manager;
            VMSearch = search;
            VMAbout = about;
            VMSettings = settings;
            VMDownload = download;

            VMSearch.MainView = this;
            VMSettings.MainView = this;

            VMSettings.ChangeTheme(int.Parse(Config.ThemeIndex()));
        }

        #region Show page
        public void ShowSearch() => ShowPage("search");
        public void ShowDownload() => ShowPage("download");
        public void ShowSettings() => ShowPage("settings");
        public void ShowAbout() => Manager.ShowDialog(VMAbout);
        private void ShowPage(string name)
        {
            ShowViewSearch = false;
            ShowViewDownload = false;
            ShowViewSettings = false;
            if (name == "search")
                ShowViewSearch = true;
           
            if (name == "download")
                ShowViewDownload = true;
            if (name == "settings")
                ShowViewSettings = true;
        }
        #endregion

        #region Windows
        public void WindowMove()
        {
            ((MainView)this.View).DragMove();
        }

        public void WindowMin()
        {
            ((MainView)this.View).WindowState = WindowState.Minimized;
        }

        public void WindowMax()
        {
            AIGS.Helper.ScreenShotHelper.MaxWindow((MainView)this.View);
        }

        public void WindowClose()
        {
            ThreadTool.Close();
            RequestClose();
        }

        public void Logout()
        {
            TidalTool.logout();
            ThreadTool.Close();
            Manager.ShowWindow(VMLogin);
            RequestClose();
        }
        #endregion



    }
}
