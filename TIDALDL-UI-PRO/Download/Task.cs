using AIGS.Common;
using AIGS.Helper;
using Stylet;
using TIDALDL_UI.Else;
using TidalLib;

namespace TIDALDL_UI.Download
{
    public class Task : Screen
    {
        public int Index { get; set; }
        public string Codec { get; set; }
        public string Title { get; set; }
        public string Own { get; set; }
        public ProgressHelper Progress { get; set; }
        public System.DateTime StartTime { get; set; }
        public string CurSizeString { get; set; }
        public string TotalSizeString { get; set; }
        public long CountIncreSize { get; set; } = 0;
        public string DownloadSpeedString { get; set; }

        public Album TidalAlbum { get; set; }
        public Playlist TidalPlaylist { get; set; }

        public Track TidalTrack { get; set; }
        public StreamUrl TrackStream { get; set; }

        public Video TidalVideo { get; set; }
        public VideoStreamUrl VideoStream { get; set; }

        public delegate void TELL_PARENT_OVER();
        public TELL_PARENT_OVER TellParentOver;

        public Task(int index, TELL_PARENT_OVER tellparent, Album album = null, Playlist playlist = null)
        {
            Index = index;
            TidalAlbum = album;
            TidalPlaylist = playlist;

            Progress = new ProgressHelper(false);
            TellParentOver = tellparent;
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


        public virtual void Download()
        {
            
        }

        public void ErrDownloadNotify(long lTotalSize, long lAlreadyDownloadSize, string sErrMsg, object data)
        {
            Progress.Errmsg = sErrMsg;
            return;
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