using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using AIGS.Common;
using AIGS.Helper;
using HandyControl.Controls;
using HandyControl.Themes;
using Stylet;
using Tidal;
using TIDALDL_UI.Else;

namespace TIDALDL_UI.Pages
{
    public class SettingsViewModel : Stylet.Screen
    {
        public MainViewModel MainView;

        public string OutputDir { get; set; }
        public int ThreadNum { get; set; }
        public int SearchNum { get; set; }
        public bool OnlyM4a { get; set; }
        public bool AddHyphen { get; set; }
        public bool UseTrackNumber { get; set; }
        public bool CheckExist { get; set; }
        public bool ArtistBeforeTitle { get; set; }
        public int MaxFileName { get; set; } = 60;
        public int MaxDirName { get; set; } = 60;

        public bool AddExplicitTag { get; set; }
        public bool IncludeEPSingle { get; set; }
        public bool AddAlbumIDBeforeFolder { get; set; }
        public int AddYearIndex { get; set; }
        public bool SaveCovers { get; set; }
        public int ThemeIndex { get; set; }
        public string AccessToken { get; set; }

        public SettingsViewModel()
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            OutputDir = Config.OutputDir();
            ThreadNum = AIGS.Common.Convert.ConverStringToInt(Config.ThreadNum());
            SearchNum = AIGS.Common.Convert.ConverStringToInt(Config.SearchNum());
            OnlyM4a = Config.OnlyM4a();
            AddHyphen = Config.AddHyphen();
            UseTrackNumber = Config.UseTrackNumber();

            AddExplicitTag = Config.AddExplicitTag();
            IncludeEPSingle = Config.IncludeEP();
            SaveCovers = Config.SaveCovers();
            MaxFileName = int.Parse(Config.MaxFileName());
            MaxDirName = int.Parse(Config.MaxDirName());
            CheckExist = Config.CheckExist();
            ArtistBeforeTitle = Config.ArtistBeforeTitle();
            AddYearIndex = Config.AddYear();
            AddAlbumIDBeforeFolder = Config.AddAlbumIDBeforeFolder();
            AccessToken = Config.Accesstoken();

            ThemeIndex = int.Parse(Config.ThemeIndex());
        }

        public void SetOutputDir()
        {
            FolderBrowserDialog openFileDialog = new FolderBrowserDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                OutputDir = openFileDialog.SelectedPath;
        }

        public void Cancel()
        {
            LoadSettings();
        }

        public void Confim()
        {
            Config.ThreadNum(ThreadNum.ToString());
            Config.SearchNum(SearchNum.ToString());
            Config.OnlyM4a(OnlyM4a.ToString());
            Config.AddExplicitTag(AddExplicitTag.ToString());
            Config.SaveCovers(SaveCovers.ToString());
            Config.IncludeEP(IncludeEPSingle.ToString());
            Config.CheckExist(CheckExist.ToString());
            Config.ArtistBeforeTitle(ArtistBeforeTitle.ToString());
            Config.AddHyphen(AddHyphen.ToString());
            Config.AddYear(AddYearIndex);
            Config.OutputDir(OutputDir);
            Config.UseTrackNumber(UseTrackNumber.ToString());
            Config.MaxFileName(MaxFileName.ToString());
            Config.MaxDirName(MaxDirName.ToString());
            Config.AddAlbumIDBeforeFolder(AddAlbumIDBeforeFolder.ToString());
            Config.ThemeIndex(ThemeIndex.ToString());

            TidalTool.SetSearchMaxNum(int.Parse(Config.SearchNum()));
            ThreadTool.SetThreadNum(ThreadNum);
            ChangeTheme(ThemeIndex);

            if (AccessToken.IsNotBlank())
            {
                if (TidalTool.loginByAccesstoken(AccessToken))
                    Config.Accesstoken(AccessToken);
                else
                {
                    Growl.Warning("Accesstoken is not valid!", "SettingsMsg");
                    return;
                }
            }
            Growl.Success("Refresh settings success!", "SettingsMsg");
        }

        public void Logout()
        {
            MainView.Logout();
        }


        public int CurThemeIndex = 0;
        public void ChangeTheme(int ThemeIndex)
        {
            if (ThemeIndex == CurThemeIndex)
                return;

            SharedResourceDictionary.SharedDictionaries.Clear();
            if (ThemeIndex == 0)
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml", UriKind.RelativeOrAbsolute) });
            else
                System.Windows.Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/HandyControl;component/Themes/SkinDark.xaml", UriKind.RelativeOrAbsolute) });

            System.Windows.Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary {Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml") });
            System.Windows.Application.Current.MainWindow?.OnApplyTemplate();
            CurThemeIndex = ThemeIndex;
        }
    }
}
