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
using TIDALDL_UI.Else;

namespace TIDALDL_UI.Pages
{
    public class MainViewModel : ModelBase
    {
        private IWindowManager Manager;
        public LoginViewModel VMLogin { get; set; }
        public SearchViewModel VMSearch { get; private set; } = new SearchViewModel();
        public AboutViewModel VMAbout { get; private set; } = new AboutViewModel();
        public SettingsViewModel VMSettings { get; private set; } = new SettingsViewModel();
        public DownloadViewModel VMDownload { get; private set; } = new DownloadViewModel();

        public MainViewModel(IWindowManager manager)
        {
            Manager = manager;
        }

        protected override void OnViewLoaded()
        {
            Global.VMMain = this;
            Global.Settings = Settings.Read();

            //Show search
            ShowPage("search");

            //Settings change
            Settings.Change(Global.Settings);

            if(Global.Settings.Version != VMAbout.Version)
            {
                Global.Settings.Version = VMAbout.Version;
                Global.Settings.Save();
                ShowPage("about");
            }
        }

        #region Show page
        public void ShowSearch() => ShowPage("search");
        public void ShowDownload() => ShowPage("download");
        public void ShowSettings() => ShowPage("settings");
        public void ShowAbout() => ShowPage("about");
        private void ShowPage(string name)
        {
            if (name == "about")
            {
                VMAbout.ViewVisibility = Visibility.Visible;
                return;
            }

            VMSearch.ViewVisibility = Visibility.Hidden;
            VMDownload.ViewVisibility = Visibility.Hidden;
            VMSettings.ViewVisibility = Visibility.Hidden;
            VMAbout.ViewVisibility = Visibility.Hidden;

            
            if (name == "search")
                VMSearch.ViewVisibility = Visibility.Visible;
            if (name == "download")
                VMDownload.ViewVisibility = Visibility.Visible;
            if (name == "settings")
            {
                VMSettings.Load();
                VMSettings.ViewVisibility = Visibility.Visible;
            }
        }
        #endregion

        #region Windows

        public void WindowMove()=>((MainView)this.View).DragMove();
        public void WindowMin()=>((MainView)this.View).WindowState = WindowState.Minimized;
        public void WindowMax()=>AIGS.Helper.ScreenShotHelper.MaxWindow((MainView)this.View);

        public void WindowClose()
        {
            ThreadTool.Close();
            RequestClose();
        }

        public void Logout()
        {
            ThreadTool.Close();

            Manager.ShowWindow(VMLogin);
            RequestClose();
        }
        #endregion



    }
}
