using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using AIGS.Common;
using AIGS.Helper;
using HandyControl.Controls;
using Stylet;
using TIDALDL_UI.Else;

namespace TIDALDL_UI.Pages
{
    public class AboutViewModel : ModelBase
    {
        public string Type { get; set; } = "(BETA)";
        public Visibility ShowDonate { get; set; } = Visibility.Collapsed;
        public string Version { get; set; } = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string LastVersion { get; set; }

        public void Feedback() => NetHelper.OpenWeb(Global.URL_TIDAL_ISSUES);
        public void Telegram() => NetHelper.OpenWeb(Global.URL_TIDAL_GROUP);
        public void ClickPaypal() => NetHelper.OpenWeb(Global.URL_PAYPAL);
        public void ClickBuymeacoffee() => NetHelper.OpenWeb(Global.URL_PAYPAL);
        public void Donate()
        {
            if (ShowDonate == Visibility.Collapsed)
                ShowDonate = Visibility.Visible;
            else
                ShowDonate = Visibility.Collapsed;
        }

        public void WindowClose() => this.ViewVisibility = Visibility.Hidden;



        //Update
        public void StartUpdate(string sVersion)
        {
            string url = GithubHelper.getFileUrl(Global.NAME_GITHUB_AUTHOR, Global.NAME_GITHUB_PROJECT, sVersion, Global.NAME_GITHUB_FILE);

            if(PathHelper.Mkdirs(Global.PATH_UPDATE) == false)
            {
                Dialog.Show(new MessageView(MessageBoxImage.Error, "Creat folder falied!", false));
                return;
            }

            DownloadFileHepler.StartAsync(url, Global.PATH_UPDATE + Global.NAME_GITHUB_FILE, null, UpdateDownloadNotify, CompleteDownloadNotify, ErrDownloadNotify, 3);
        }

        public bool UpdateDownloadNotify(long lTotalSize, long lAlreadyDownloadSize, long lIncreSize, object data)
        {
            //int progress = (int)(lAlreadyDownloadSize * 100 / lTotalSize);
            //if (progress > ProgressValue)
            //    ProgressValue = progress;

            //float Size;
            //if (TotalSize.IsBlank())
            //{
            //    Size = (float)lTotalSize / 1048576;
            //    TotalSize = Size.ToString("#0.00");
            //}

            //Size = (float)lAlreadyDownloadSize / 1048576;
            //DownloadSize = Size.ToString("#0.00");
            return true;
        }

        public void CompleteDownloadNotify(long lTotalSize, object data)
        {
            //ProgressValue = 100;

            //if (UnzipRequire())
            //    Action((true, "Download success!"));
            //else
            //    ShowErr();
        }

        public void ErrDownloadNotify(long lTotalSize, long lAlreadyDownloadSize, string sErrMsg, object data)
        {
            //ShowErr();
        }
    }
}
