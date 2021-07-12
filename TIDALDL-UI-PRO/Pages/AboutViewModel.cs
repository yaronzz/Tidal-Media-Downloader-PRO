using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using AIGS.Common;
using AIGS.Helper;
using HandyControl.Controls;
using Stylet;
using TIDALDL_UI.Else;
using TidalLib;

namespace TIDALDL_UI.Pages
{
    public class AboutViewModel : ModelBase
    {
        public MainViewModel MainVM;
        public string Type { get; set; } = "(BETA)";
        public string Version { get; set; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string LastVersion { get; set; }
        public Visibility ShowDonate { get; set; } = Visibility.Collapsed;
        public Visibility ShowProgress { get; set; } = Visibility.Collapsed;
        public Visibility EnableUpdate { get; set; } = Visibility.Collapsed;
        public ProgressHelper Progress { get; set; } = new ProgressHelper(false);
        public string DownloadStatusInfo { get; set; }
        System.DateTime StartTime { get; set; }
        public string CurSizeString { get; set; }
        public string TotalSizeString { get; set; }
        public long CountIncreSize { get; set; } = 0;
        public string DownloadSpeedString { get; set; }
        public void Feedback() => NetHelper.OpenWeb(Global.URL_TIDAL_ISSUES);
        public void Telegram() => NetHelper.OpenWeb(Global.URL_TIDAL_GROUP);
        public void ClickPaypal() => NetHelper.OpenWeb(Global.URL_PAYPAL);
        public void ClickBuymeacoffee() => NetHelper.OpenWeb(Global.URL_BUYMEACOFFEE);

        public void Donate()
        {
            if (ShowDonate == Visibility.Collapsed)
                ShowDonate = Visibility.Visible;
            else
                ShowDonate = Visibility.Collapsed;
        }

        public void WindowClose() => this.ViewVisibility = Visibility.Hidden;


        public void EndUpdate()
        {
            ShowProgress = Visibility.Collapsed;
        }
        //Update
        public void StartUpdate()
        {
            Progress.ValueInt = 0;
            CountIncreSize = 0;
            ShowProgress = Visibility.Visible;
            DownloadStatusInfo = Language.Get("strmsgGetNewVersionUrl");

            string url = GithubHelper.getFileUrl(Global.NAME_GITHUB_AUTHOR, Global.NAME_GITHUB_PROJECT, LastVersion, Global.NAME_GITHUB_FILE);
            if (PathHelper.Mkdirs(Global.PATH_UPDATE) == false)
            {
                DownloadStatusInfo = Language.Get("strmsgCreatUpdateFolderFailed");
                EndUpdate();
                return;
            }

            DownloadStatusInfo = Language.Get("strmsgStartUpdate");
            Progress.SetStatus(ProgressHelper.STATUS.RUNNING);
            StartTime = TimeHelper.GetCurrentTime();
            LoginKey key = Tools.GetKey();
            DownloadFileHepler.StartAsync(url, Global.PATH_UPDATE + Global.NAME_GITHUB_FILE, null, UpdateDownloadNotify, CompleteDownloadNotify, ErrDownloadNotify, 3, Proxy: key.Proxy);
        }

        public bool UpdateDownloadNotify(long lTotalSize, long lAlreadyDownloadSize, long lIncreSize, object data)
        {
            Progress.UpdateInt(lAlreadyDownloadSize, lTotalSize);
            if (Progress.GetStatus() != ProgressHelper.STATUS.RUNNING)
                return false;

            CountIncreSize += lIncreSize;
            long consumeTime = TimeHelper.CalcConsumeTime(StartTime);

            if (consumeTime >= 1000)
            {
                DownloadSpeedString = AIGS.Common.Convert.ConverStorageUintToString(CountIncreSize, AIGS.Common.Convert.UnitType.BYTE) + "/S";
                CountIncreSize = 0;
                StartTime = TimeHelper.GetCurrentTime();
            }

            CurSizeString = AIGS.Common.Convert.ConverStorageUintToString(lAlreadyDownloadSize, AIGS.Common.Convert.UnitType.BYTE);
            if (TotalSizeString.IsBlank())
                TotalSizeString = AIGS.Common.Convert.ConverStorageUintToString(lTotalSize, AIGS.Common.Convert.UnitType.BYTE);

            DownloadStatusInfo = CurSizeString + " / " + TotalSizeString + "  " + DownloadSpeedString;
            return true;
        }

        public void CompleteDownloadNotify(long lTotalSize, object data)
        {
            Progress.ValueInt = 100;
            Progress.SetStatus(ProgressHelper.STATUS.COMPLETE);
            DownloadStatusInfo = Language.Get("strmsgDownloadCompleteStartUpdate");

            string sBat = "ping -n 5 127.0.0.1\n";
            sBat += string.Format("move {0} {1}\\tidal-gui.exe\n", Global.PATH_UPDATE + Global.NAME_GITHUB_FILE, Path.GetFullPath(".\\"));
            sBat += string.Format("start {0}\\tidal-gui.exe\n", Path.GetFullPath(".\\"));
            FileHelper.Write(sBat, true, Global.PATH_UPDATEBAT);

            Application.Current.Dispatcher.BeginInvoke((Action)delegate ()
            {
                CmdHelper.StartExe(Global.PATH_UPDATEBAT, null, IsShowWindow: false);
                MainVM.WindowClose();
            });
        }

        public void ErrDownloadNotify(long lTotalSize, long lAlreadyDownloadSize, string sErrMsg, object data)
        {
            DownloadStatusInfo = sErrMsg;
        }
    }
}
