using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using AIGS.Common;
using AIGS.Helper;
using HandyControl.Controls;
using HandyControl.Data;
using Stylet;
using TIDALDL_UI.Else;
using TidalLib;
using MessageBox = HandyControl.Controls.MessageBox;

namespace TIDALDL_UI.Pages
{
    public class TaskViewModel : ModelBase
    {
        public string Title { get; set; }
        public string Cover { get; set; }
        public string Desc { get; set; }
        public string BasePath { get; set; }
        public Settings Settings { get; set; }
        public Visibility ShowItems { get; set; } = Visibility.Visible;

        public ObservableCollection<object> Items { get; set; } = new ObservableCollection<object>();
        public DownloadViewModel VMParent { get; set; }

        public TaskViewModel(Detail detail, DownloadViewModel parent)
        {
            Title = detail.Title;
            Cover = detail.CoverUrl;
            Desc = detail.Intro;
            VMParent = parent;
            Settings = Settings.Read();

            if (detail.Data.GetType() == typeof(Album))
                SetAlbum(detail, (Album)detail.Data);
            if (detail.Data.GetType() == typeof(Track))
                SetTrack(detail, (Track)detail.Data);
            if (detail.Data.GetType() == typeof(Video))
                SetVideo(detail, (Video)detail.Data);
            if (detail.Data.GetType() == typeof(Artist))
                SetArtist(detail, (Artist)detail.Data);
            if (detail.Data.GetType() == typeof(Playlist))
                SetPlaylist(detail, (Playlist)detail.Data);
        }

        public void SetAlbum(Detail detail, Album album)
        {
            BasePath = Tools.GetAlbumPath(album, Settings);

            if(Settings.SaveCovers)
                NetHelper.DownloadFile(album.CoverHighUrl, BasePath + "/Cover.jpg");

            foreach (var item in detail.Items)
            {
                if (item.Check == false)
                    continue;
                if(item.Data.GetType() == typeof(Track))
                    Items.Add(new TrackTask((Track)item.Data, Items.Count + 1, Settings, RecieveDownloadOver, album));
                else
                    Items.Add(new VideoTask((Video)item.Data, Items.Count + 1, Settings, RecieveDownloadOver, album));
            }
        }

        public void SetTrack(Detail detail, Track track)
        {
            BasePath = Tools.GetAlbumPath(track.Album, Settings);
            Items.Add(new TrackTask((Track)detail.Data, Items.Count + 1, Settings, RecieveDownloadOver, track.Album));
        }

        public void SetVideo(Detail detail, Video video)
        {
            BasePath = Tools.GetVideoPath(Settings, video, null);
            BasePath = Path.GetDirectoryName(BasePath);
            Items.Add(new VideoTask((Video)detail.Data, Items.Count + 1, Settings, RecieveDownloadOver, video.Album));
        }

        public void SetArtist(Detail detail, Artist artist)
        {
            BasePath = Tools.GetArtistPath(artist, Settings);
            foreach (var item in detail.Items)
            {
                if (item.Check == false)
                    continue;
                Album album = (Album)item.Data;

                if (Settings.SaveCovers)
                    NetHelper.DownloadFile(album.CoverHighUrl, Tools.GetAlbumPath(album, Settings) + "/Cover.jpg");

                foreach (var track in album.Tracks)
                    Items.Add(new TrackTask(track, Items.Count + 1, Settings, RecieveDownloadOver, album));
                foreach (var video in album.Videos)
                    Items.Add(new VideoTask(video, Items.Count + 1, Settings, RecieveDownloadOver, album));
            }
        }

        public void SetPlaylist(Detail detail, Playlist playlist)
        {
            BasePath = Tools.GetPlaylistPath(playlist, Settings);

            foreach (var item in detail.Items)
            {
                if (item.Check == false)
                    continue;
                if (item.Data.GetType() == typeof(Track))
                    Items.Add(new TrackTask((Track)item.Data, Items.Count + 1, Settings, RecieveDownloadOver, null, playlist));
                else
                    Items.Add(new VideoTask((Video)item.Data, Items.Count + 1, Settings, RecieveDownloadOver, null, playlist));
            }
        }

        public void RecieveDownloadOver()
        {
            lock (this)
            {
                bool bError = false;
                bool bAllOver = true;
                ProgressHelper.STATUS Status = ProgressHelper.STATUS.RUNNING;
                foreach (var item in Items)
                {
                    if (item.GetType() == typeof(TrackTask))
                        Status = ((TrackTask)item).Progress.GetStatus();
                    else if (item.GetType() == typeof(VideoTask))
                        Status = ((VideoTask)item).Progress.GetStatus();

                    if (Status == ProgressHelper.STATUS.RUNNING || Status == ProgressHelper.STATUS.WAIT)
                    {
                        bAllOver = false;
                        break;
                    }
                    else if (Status == ProgressHelper.STATUS.ERROR || Status == ProgressHelper.STATUS.CANCLE)
                        bError = true;
                }

                if (bAllOver)
                {
                    if(bError)
                        VMParent.TaskError(this);
                    else
                        VMParent.TaskComplete(this);
                }
            }
        }

        #region Button
        public void ChangeExpand()
        {
            if (ShowItems == Visibility.Collapsed)
                ShowItems = Visibility.Visible;
            else
                ShowItems = Visibility.Collapsed;
        }

        public void Delete()
        {
            //if (MessageBox.Show("Remove task?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            //    return;

            Dialog.Show(new MessageView(MessageBoxImage.Information, "Remove task?", true, (x) =>
            {
                foreach (var item in Items)
                {
                    if (item.GetType() == typeof(TrackTask))
                        ((TrackTask)item).Cancel();
                    else if (item.GetType() == typeof(VideoTask))
                        ((VideoTask)item).Cancel();
                }
                VMParent.DeleteTask(this);
            }));
        }

        public void Retry()
        {
            
        }

        public void OpenFolder()
        {
            string path = Path.GetFullPath(BasePath);
            try
            {
                Process.Start(path);
            }
            catch
            {
                Growl.Error("Open folder failed!" + path, Global.TOKEN_MAIN);
            }
        }
        #endregion
    }

}
