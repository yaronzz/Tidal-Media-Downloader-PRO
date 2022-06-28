using AIGS.Common;
using AIGS.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TagLib;
using TIDALDL_UI.Else;
using TidalLib;

namespace TIDALDL_UI.Download
{
    public class TrackTask : Task
    {
        public TrackTask(Track track, int index, TELL_PARENT_OVER tellparent, Album album = null, Playlist playlist = null)
        : base(index, tellparent, album, playlist)
        {
            TidalTrack = track;
            Title = track.Title;
            Own = track.Album.Title;

            Start();
        }

        private string GetLyrics(string path)
        {
            var lyrics = "";
            try
            {
                lyrics = Global.Client.GetTrackLyrics(TidalTrack.ID).Result.Subtitles;
                if (Global.Settings.SaveLyrics)
                {
                    var lrcPath = path + ".lrc";
                    AIGS.Helper.FileHelper.Write(lyrics, true, lrcPath);
                }
            }
            catch (Exception)
            {
            }
            return lyrics;
        }

        private string[] GetArtistsNamesList(ObservableCollection<Artist> artists)
        {
            if (artists == null)
                return null;

            var names = new List<string>();
            foreach (var item in artists)
            {
                names.Add(item.Name);
            }
            return names.ToArray();
        }

        public void SetMetaData(string filepath, Album TidalAlbum, Track TidalTrack, string lyrics = "")
        {
            try
            {
                var tfile = TagLib.File.Create(filepath);

                tfile.Tag.Album = TidalAlbum.Title;
                tfile.Tag.Track = (uint)TidalTrack.TrackNumber;
                tfile.Tag.TrackCount = (uint)TidalAlbum.NumberOfTracks;
                tfile.Tag.Title = TidalTrack.Title;
                tfile.Tag.Disc = (uint)TidalTrack.VolumeNumber;
                tfile.Tag.DiscCount = (uint)TidalAlbum.NumberOfVolumes;
                tfile.Tag.Copyright = TidalTrack.Copyright;
                tfile.Tag.AlbumArtists = GetArtistsNamesList(TidalAlbum.Artists);
                tfile.Tag.Performers = GetArtistsNamesList(TidalTrack.Artists);
                tfile.Tag.Lyrics = lyrics;
                //ReleaseDate
                if (TidalAlbum.ReleaseDate != null && TidalAlbum.ReleaseDate.IsNotBlank())
                    tfile.Tag.Year = (uint)AIGS.Common.Convert.ConverStringToInt(TidalAlbum.ReleaseDate.Split("-")[0]);

                //Cover
                var pictures = new Picture[1];
                pictures[0] = new Picture(NetHelper.DownloadData(TidalAlbum.CoverHighUrl));
                tfile.Tag.Pictures = pictures;
                tfile.Save();
            }
            catch (Exception e)
            {
                throw new Exception("Set metadata failed!");
            }
        }

        public override void Download()
        {
            try
            {
                //GetStream
                Progress.StatusMsg = "GetStream...";
                TrackStream = Global.Client.GetTrackStreamUrl(TidalTrack.ID, Global.Settings.AudioQuality).Result;
                Progress.StatusMsg = "GetStream success...";

                Codec = TrackStream.Codec;
                if (TidalAlbum == null && TidalTrack.Album != null)
                    TidalAlbum = Global.Client.GetAlbum(TidalTrack.Album.ID).Result;

                //Get path 
                string path = Paths.GetTrackPath(TidalTrack, TrackStream, TidalAlbum, TidalPlaylist);

                //Check if song downloaded already
                if (Global.Settings.CheckExist && System.IO.File.Exists(path))
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
                    if ((bool)DownloadFileHepler.Start(TrackStream.Url, path, Timeout: 5 * 1000, UpdateFunc: UpdateDownloadNotify, ErrFunc: ErrDownloadNotify, Proxy: Global.Client.proxy))
                    {
                        //Decrypt
                        Progress.StatusMsg = "Decrypt...";
                        if (!Decryption.DecryptTrackFile(TrackStream, path))
                        {
                            Progress.Errmsg = "Decrypt failed!";
                            goto ERR_RETURN;
                        }

                        //Get lyrics
                        Progress.StatusMsg = "Get lyrics...";
                        var lyrics = GetLyrics(path);
                                        
                        //SetMetaData
                        Progress.StatusMsg = "Set metaData...";
                        SetMetaData(path, TidalAlbum, TidalTrack, lyrics);

                        Progress.SetStatus(ProgressHelper.STATUS.COMPLETE);
                        goto CALL_RETURN;
                    }
                }
                Progress.Errmsg = "Download failed!";
                System.IO.File.Delete(path);
            }
            catch(Exception e)
            {
                Progress.Errmsg = e.Message;
            }

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
