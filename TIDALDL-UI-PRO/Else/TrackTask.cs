using AIGS.Common;
using AIGS.Helper;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tidal;
namespace TIDALDL_UI.Else
{
    public class TrackTask : Screen
    {
        public int Index { get; set; }
        public string Title { get; set; }

        public string Own { get; set; }
        public string Codec { get; set; }
        public string Errmsg { get; set; }
        public ProgressHelper Progress { get; set; }

        Track TidalTrack;
        Album TidalAlbum;
        Playlist TidalPlaylist;
        public int AddYear;
        public bool CheckExist;
        public bool ArtistBeforeTitle;
        public bool AddExplict;
        public bool OnlyM4a;
        public bool AddHyphen;
        public bool UseTrackNumber;
        eSoundQuality Quality;
        public string FilePath { get; set; }
        public string OutputDir { get; set; }

        public delegate void TELL_PARENT_OVER();
        public TELL_PARENT_OVER TellParentOver;


        public TrackTask(Track data, int index, TELL_PARENT_OVER tellparent, Album album = null, Playlist playlist = null)
        {
            Index = index;
            Title = data.Title;
            Own = album == null ? null : album.Title;
            Progress = new ProgressHelper(false);
            TellParentOver = tellparent;

            TidalTrack = data;
            TidalAlbum = album;
            TidalPlaylist = playlist;
            Quality = TidalTool.getQuality(Config.Quality());
            OnlyM4a = Config.OnlyM4a();
            AddHyphen = Config.AddHyphen();
            UseTrackNumber = Config.UseTrackNumber();
            CheckExist = Config.CheckExist();
            ArtistBeforeTitle = Config.ArtistBeforeTitle();
            AddExplict = Config.AddExplicitTag();
            AddYear    = Config.AddYear();
            OutputDir  = Config.OutputDir();
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
            string Errlabel = "";

            //GetStream
            Progress.StatusMsg = "GetStream...";
            StreamUrl TidalStream = TidalTool.getStreamUrl(TidalTrack.ID.ToString(), Quality, out Errlabel);
            if (Errlabel.IsNotBlank() || TidalStream == null)
                goto ERR_RETURN;
            if (TidalStream.Codec != "ac4" && TidalStream.Codec != "mha1")
            {
                if (Quality >= eSoundQuality.LOSSLESS)
                {
                    if (TidalStream.Url.Contains(".m4a") || TidalStream.Url.Contains(".mp4"))
                    {
                        StreamUrl TidalStream2 = TidalTool.getStreamUrl2(TidalTrack.ID.ToString(), Quality, out Errlabel);
                        if (Errlabel.IsBlank() && TidalStream2 != null)
                            TidalStream = TidalStream2;
                        Errlabel = "";
                    }
                }
            }
            Codec = TidalStream.Codec;
            Progress.StatusMsg = "GetStream success...";

            //Get path 
            FilePath = TidalTool.getTrackPath(OutputDir, TidalAlbum, TidalTrack, TidalStream.Url,
                AddHyphen, TidalPlaylist, artistBeforeTitle: ArtistBeforeTitle, addexplicit: AddExplict,
                addYear: AddYear, useTrackNumber: UseTrackNumber);


            //Check if song is downloaded already
            string CheckName = OnlyM4a ? FilePath.Replace(".mp4", ".m4a") : FilePath;
            if (CheckExist && System.IO.File.Exists(CheckName))
            {
                Progress.UpdateInt(100, 100);
                Progress.SetStatus(ProgressHelper.STATUS.COMPLETE);
                goto CALL_RETURN;
            }

            //Get contributors
            Progress.StatusMsg = "GetContributors...";
            ObservableCollection<Contributor> pContributors = TidalTool.getTrackContributors(TidalTrack.ID.ToString(), out Errlabel);

            //Download
            Progress.StatusMsg = "Start...";
            for (int i = 0; i < 100 && Progress.GetStatus() != ProgressHelper.STATUS.CANCLE; i++)
            {
                if ((bool)DownloadFileHepler.Start(TidalStream.Url, FilePath, Timeout: 5 * 1000, UpdateFunc: UpdateDownloadNotify, ErrFunc: ErrDownloadNotify, Proxy: TidalTool.PROXY))
                {
                    //Decrypt
                    if (!TidalTool.DecryptTrackFile(TidalStream, FilePath))
                    {
                        Errlabel = "Decrypt failed!";
                        goto ERR_RETURN;
                    }

                    if (OnlyM4a && Path.GetExtension(FilePath).ToLower().IndexOf("mp4") >= 0)
                    {
                        if (TidalStream.Codec != "ac4" && TidalStream.Codec != "mha1")
                        {
                            string sNewName;
                            if (!Config.IsFFmpegExist())
                            {
                                Errlabel = "Convert mp4 to m4a failed!(FFmpeg is not exist!)";
                                goto ERR_RETURN;
                            }
                            if (!TidalTool.ConvertMp4ToM4a(FilePath, out sNewName))
                            {
                                Errlabel = "Convert mp4 to m4a failed!(No reason, Please feedback!)";
                                goto ERR_RETURN;
                            }
                            else
                                FilePath = sNewName;
                        }
                    }

                    //SetMetaData 
                    if (TidalAlbum == null && TidalTrack.Album != null)
                    {
                        string sErrcode = null;
                        TidalAlbum = TidalTool.getAlbum(TidalTrack.Album.ID.ToString(), out sErrcode);
                    }
                    string sLabel = TidalTool.SetMetaData(FilePath, TidalAlbum, TidalTrack, TidalTool.getAlbumCoverPath(OutputDir, TidalAlbum, AddYear), pContributors);
                    if (sLabel.IsNotBlank())
                    {
                        Errlabel = "Set metadata failed!" + sLabel;
                        goto ERR_RETURN;
                    }

                    Progress.SetStatus(ProgressHelper.STATUS.COMPLETE);
                    goto CALL_RETURN;
                }
            }
            Errlabel = "Download failed!";

        ERR_RETURN:
            if (Progress.GetStatus() == ProgressHelper.STATUS.CANCLE)
                goto CALL_RETURN;
            Progress.SetStatus(ProgressHelper.STATUS.ERROR);
            Progress.Errmsg = Errlabel;

        CALL_RETURN:
            TellParentOver();
        }

        public void ErrDownloadNotify(long lTotalSize, long lAlreadyDownloadSize, string sErrMsg, object data)
        {
            return;
        }

        public bool UpdateDownloadNotify(long lTotalSize, long lAlreadyDownloadSize, long lIncreSize, object data)
        {
            Progress.UpdateInt(lAlreadyDownloadSize, lTotalSize);
            if (Progress.GetStatus() != ProgressHelper.STATUS.RUNNING)
                return false;
            return true;
        }
    }
}
