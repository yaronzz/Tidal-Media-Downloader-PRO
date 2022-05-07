using AIGS.Common;
using AIGS.Helper;
using Stylet;
using System;
using TidalLib;

namespace TIDALDL_UI.Else
{
    public class TrackTask : Screen
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

        Track TidalTrack { get; set; }
        Album TidalAlbum { get; set; }
        Playlist TidalPlaylist { get; set; }
        StreamUrl Stream { get; set; }
        Settings Settings { get; set; }

        public delegate void TELL_PARENT_OVER();
        public TELL_PARENT_OVER TellParentOver;

        public TrackTask(Track track, int index, Settings settings, TELL_PARENT_OVER tellparent, Album album = null, Playlist playlist = null)
        {
            Index = index;
            Settings = settings;
            TidalTrack = track;
            TidalAlbum = album;
            TidalPlaylist = playlist;

            Title = track.Title;
            Own = track.Album.Title;

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
            try
            {
                LoginKey key = Tools.GetKey();

                //GetStream
                Progress.StatusMsg = "GetStream...";
                (Progress.Errmsg, Stream) = Client.GetTrackStreamUrl(key, TidalTrack.ID, Settings.AudioQuality).Result;
                if (Progress.Errmsg.IsNotBlank() || Stream == null)
                    goto ERR_RETURN;

                Codec = Stream.Codec;
                Progress.StatusMsg = "GetStream success...";

                if (TidalAlbum == null && TidalTrack.Album != null)
                {
                    string tmpmsg;
                    (tmpmsg, TidalAlbum) = Client.GetAlbum(key, TidalTrack.Album.ID, false).Result;
                }

                //Get path 
                string path = Tools.GetTrackPath(Settings, TidalTrack, Stream, TidalAlbum, TidalPlaylist);

                //Check if song downloaded already
                string checkpath = Settings.OnlyM4a ? path.Replace(".mp4", ".m4a") : path;
                if (Settings.CheckExist && System.IO.File.Exists(checkpath))
                {
                    Progress.UpdateInt(100, 100);
                    Progress.SetStatus(ProgressHelper.STATUS.COMPLETE);
                    goto CALL_RETURN;
                }

                //Download
                Progress.StatusMsg = "Start...";
                for (int i = 0; i < 50 && Progress.GetStatus() != ProgressHelper.STATUS.CANCLE; i++)
                {
                    StartTime = TimeHelper.GetCurrentTime();
                    if ((bool)DownloadFileHepler.Start(Stream.Url, path, Timeout: 5 * 1000, UpdateFunc: UpdateDownloadNotify, ErrFunc: ErrDownloadNotify, Proxy: key.Proxy))
                    {
                        //Decrypt
                        Progress.StatusMsg = "Decrypt...";
                        if (!Tools.DecryptTrackFile(Stream, path))
                        {
                            Progress.Errmsg = "Decrypt failed!";
                            goto ERR_RETURN;
                        }

                        if (Settings.OnlyM4a)
                        {
                            (Progress.Errmsg, path) = Tools.ConvertMp4ToM4a(path, Stream);
                            if (Progress.Errmsg.IsNotBlank())
                                goto ERR_RETURN;
                        }

                        //Get lyrics
                        Progress.StatusMsg = "Get lyrics...";
                        string lyrics = Client.GetLyrics(key, TidalTrack.Title, TidalTrack.Artist == null ? "" : TidalTrack.Artist.Name);

                        //SetMetaData
                        Progress.StatusMsg = "Set metaData...";
                        if (TidalAlbum == null)
                            (Progress.Errmsg, TidalAlbum) = Client.GetAlbum(key, TidalTrack.Album.ID, false).Result;
                        Progress.Errmsg = Tools.SetMetaData(path, TidalAlbum, TidalTrack, lyrics);
                        if (Progress.Errmsg.IsNotBlank())
                        {
                            Progress.Errmsg = "Set metadata failed!" + Progress.Errmsg;
                            goto ERR_RETURN;
                        }

                        Progress.SetStatus(ProgressHelper.STATUS.COMPLETE);
                        goto CALL_RETURN;
                    }
                }
                Progress.Errmsg = "Download failed!";
                System.IO.File.Delete(path);
            }
            catch(Exception e)
            {
                Progress.Errmsg = "Download failed!" + e.Message;
            }

        ERR_RETURN:
            if (Progress.GetStatus() == ProgressHelper.STATUS.CANCLE)
                goto CALL_RETURN;
            Progress.SetStatus(ProgressHelper.STATUS.ERROR);

        CALL_RETURN:
            TellParentOver();

            DownloadSpeedString = "";
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
