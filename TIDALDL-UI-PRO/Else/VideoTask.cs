using AIGS.Common;
using AIGS.Helper;
using Stylet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tidal;
namespace TIDALDL_UI.Else
{
    public class VideoTask : Screen
    {
        public int Index { get; set; }
        public string Title { get; set; }

        public string Own { get; set; }
        public string Codec { get; set; }
        public string Errmsg { get; set; }
        public ProgressHelper Progress { get; set; }

        Video TidalVideo;
        Album TidalAlbum;
        Playlist TidalPlaylist;
        public int AddYear;
        public bool ArtistBeforeTitle;
        public bool AddHyphen;
        public eResolution Resolution { get; set; }

        public string FilePath { get; set; }
        public string OutputDir { get; set; }

        public delegate void TELL_PARENT_OVER();
        public TELL_PARENT_OVER TellParentOver;

        public VideoTask(Video data, int index, TELL_PARENT_OVER tellparent, Album album = null, Playlist playlist = null)
        {
            Index = index;
            Title = data.Title;
            Own = data.Artist.Name;
            Progress = new ProgressHelper(false);
            TellParentOver = tellparent;

            TidalVideo = data;
            TidalAlbum = album;
            TidalPlaylist = playlist;
            Resolution = TidalTool.getResolution(Config.Resolution());
            AddHyphen = Config.AddHyphen();
            ArtistBeforeTitle = Config.ArtistBeforeTitle();
            AddYear = Config.AddYear();
            OutputDir = Config.OutputDir();
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
            //GetStream
            Progress.StatusMsg = "GetStream...";
            string Errlabel = "";
            string RetResolution;
            string[] TidalVideoUrls = TidalTool.getVideoDLUrls(TidalVideo.ID.ToString(), Resolution, out Errlabel, out RetResolution);
            if (Errlabel.IsNotBlank())
                goto ERR_RETURN;
            string TsFilePath = TidalTool.getVideoPath(OutputDir, TidalVideo, TidalAlbum, ".ts", hyphen: AddHyphen, plist: TidalPlaylist, artistBeforeTitle: ArtistBeforeTitle, addYear: AddYear);
            Codec = RetResolution;

            //Download
            Progress.StatusMsg = "Start...";
            if (!(bool)M3u8Helper.Download(TidalVideoUrls, TsFilePath, ProgressNotify, Proxy: TidalTool.PROXY))
            {
                Errlabel = "Download failed!";
                goto ERR_RETURN;
            }

            //Convert
            FilePath = TidalTool.getVideoPath(OutputDir, TidalVideo, TidalAlbum, hyphen: AddHyphen, plist: TidalPlaylist, artistBeforeTitle: ArtistBeforeTitle, addYear: AddYear);
            if (!Config.IsFFmpegExist())
            {
                Errlabel = "FFmpeg is not exist!";
                goto ERR_RETURN;
            }
            if (!FFmpegHelper.Convert(TsFilePath, FilePath))
            {
                Errlabel = "Convert failed!";
                goto ERR_RETURN;
            }
            System.IO.File.Delete(TsFilePath);

            //SetMetaData 
            string sLabel = TidalTool.SetMetaData(FilePath, null, null, null, null, TidalVideo);
            if (sLabel.IsNotBlank())
            {
                Errlabel = "Set metadata failed!";
                goto ERR_RETURN;
            }

            Progress.SetStatus(ProgressHelper.STATUS.COMPLETE);
            goto CALL_RETURN;

            ERR_RETURN:
            if (Progress.GetStatus() == ProgressHelper.STATUS.CANCLE)
                goto CALL_RETURN;

            Progress.SetStatus(ProgressHelper.STATUS.ERROR);
            Progress.Errmsg = Errlabel;

        CALL_RETURN:
            TellParentOver();
        }

        public bool ProgressNotify(long lCurSize, long lAllSize)
        {
            Progress.UpdateInt(lCurSize, lAllSize);
            if (Progress.GetStatus() != ProgressHelper.STATUS.RUNNING)
                return false;
            return true;
        }
    }
}
