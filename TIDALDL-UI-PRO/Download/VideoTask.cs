using AIGS.Helper;
using TIDALDL_UI.Else;
using TidalLib;

namespace TIDALDL_UI.Download
{
    public class VideoTask : Task
    {
        public VideoTask(Video video, int index, TELL_PARENT_OVER tellparent, Album album = null, Playlist playlist = null)    
        : base(index, tellparent, album, playlist)
        {
            TidalVideo = video;
            Title = video.Title;
            Own = video.ArtistsName;

            Start();
        }

        public override void Download()
        {
            //GetStream
            Progress.StatusMsg = "GetStream...";
            VideoStream = Global.Client.GetVideStreamUrl(TidalVideo.ID, Global.Settings.VideoQuality).Result;
            Progress.StatusMsg = "GetStream success...";

            Codec = VideoStream.Resolution;

            //Get path
            string path = Paths.GetVideoPath(TidalVideo, TidalAlbum, TidalPlaylist);

            //Download
            string errmsg = "";
            Progress.StatusMsg = "Start...";
            string[] tsurls = M3u8Helper.GetTsUrls(VideoStream.M3u8Url);
            if (!(bool)M3u8Helper.Download(tsurls, path, UpdateDownloadNotify, ref errmsg, Proxy: Global.Client.proxy))
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
    }
}
