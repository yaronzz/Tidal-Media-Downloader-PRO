using AIGS.Common;
using AIGS.Helper;
using Stylet;
using TidalLib;

namespace TIDALDL_UI.Else
{
    public class VideoTask : Screen
    {
        public int    Index { get; set; }
        public string Codec { get; set; }
        public string Title { get; set; }
        public string Own   { get; set; }
        public ProgressHelper Progress { get; set; }
        System.DateTime StartTime { get; set; }
        public string CurSizeString { get; set; }
        public string TotalSizeString { get; set; }
        public long CountIncreSize { get; set; } = 0;
        public string DownloadSpeedString { get; set; }

        Video     TidalVideo { get; set; }
        Album     TidalAlbum { get; set; }
        Playlist  TidalPlaylist { get; set; }
        Settings  Settings { get; set; }
        VideoStreamUrl Stream { get; set; }

        public delegate void TELL_PARENT_OVER();
        public TELL_PARENT_OVER TellParentOver;

        public VideoTask(Video video, int index, Settings settings, TELL_PARENT_OVER tellparent, Album album = null, Playlist playlist = null)
        {
            Index = index;
            Settings = settings;
            TidalVideo = video;
            TidalAlbum = album;
            TidalPlaylist = playlist;

            Title = video.Title;
            Own = video.ArtistsName;

            Progress = new ProgressHelper(false);
            TellParentOver = tellparent;

            Start();
        }

        #region Method
        public void Start()
        {
            //Add to threadpool
            ThreadTool.AddWork((object[] data) =>
            {
                if (Progress.GetStatus() != ProgressHelper.STATUS.WAIT)
                    return;

                Progress.SetStatus(ProgressHelper.STATUS.RUNNING);
                Download();
            });
        }

        public void Cancel()
        {
            if (Progress.GetStatus() != ProgressHelper.STATUS.COMPLETE)
                Progress.SetStatus(ProgressHelper.STATUS.CANCLE);
        }

        public void Restart()
        {
            ProgressHelper.STATUS status = Progress.GetStatus();
            if (status == ProgressHelper.STATUS.CANCLE || status == ProgressHelper.STATUS.ERROR)
            {
                Progress.Clear();
                Start();
            }
        }
        #endregion

        public void Download()
        {
            LoginKey key = Tools.GetKey(true);

            //GetStream
            Progress.StatusMsg = "GetStream...";
            (Progress.Errmsg, Stream) = Client.GetVideStreamUrl(key, TidalVideo.ID, Settings.VideoQuality).Result;
            if (Progress.Errmsg.IsNotBlank() || Stream == null)
                goto ERR_RETURN;

            Codec = Stream.Resolution;
            Progress.StatusMsg = "GetStream success...";

            //Get path
            string path = Tools.GetVideoPath(Settings, TidalVideo, TidalAlbum, TidalPlaylist, ".mp4");

            //Download
            string errmsg = "";
            Progress.StatusMsg = "Start...";
            string[] tsurls = M3u8Helper.GetTsUrls(Stream.M3u8Url);
            if (!(bool)M3u8Helper.Download(tsurls, path, UpdateDownloadNotify, ref errmsg, Proxy: key.Proxy))
            {
                Progress.Errmsg = "Download failed!" + errmsg;
                goto ERR_RETURN;
            }

            ////Convert
            //(Progress.Errmsg, path) = Tools.ConvertTsToMp4(path);
            //if (Progress.Errmsg.IsNotBlank() )
            //    goto ERR_RETURN;

            Progress.SetStatus(ProgressHelper.STATUS.COMPLETE);
            goto CALL_RETURN;

        ERR_RETURN:
            if (Progress.GetStatus() == ProgressHelper.STATUS.CANCLE)
                goto CALL_RETURN;

            Progress.SetStatus(ProgressHelper.STATUS.ERROR);

        CALL_RETURN:
            TellParentOver();

            DownloadSpeedString = "";
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
            return true;
        }

    }
}
