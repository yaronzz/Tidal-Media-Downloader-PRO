using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using AIGS.Common;
using AIGS.Helper;
using HandyControl.Data;
using Stylet;
using Tidal;
using TIDALDL_UI.Else;
using MessageBox = HandyControl.Controls.MessageBox;

namespace TIDALDL_UI.Pages
{
    public class TaskViewModel : Screen
    {
        public BitmapImage Cover { get; set; }
        public string Title { get; set; }
        public string Desc { get; set; }
        public string BasePath { get; set; }
        public int MaxHeight { get; set; } = 1000;
        public ObservableCollection<object> ItemList { get; set; } = new ObservableCollection<object>();
        public DownloadViewModel VMParent { get; set; }


        public TaskViewModel(object data, DownloadViewModel parent)
        {
            VMParent = parent;
            if (data.GetType() == typeof(Album))
            {
                Album album = (Album)data;
                Title = album.Title;
                BasePath = TidalTool.getAlbumFolder(Config.OutputDir(), album, Config.AddYear());
                Desc = string.Format("by {0}-{1} Tracks-{2} Videos-{3}", album.Artist.Name, TimeHelper.ConverIntToString(album.Duration), album.NumberOfTracks, album.NumberOfVideos);
                Cover = AIGS.Common.Convert.ConverByteArrayToBitmapImage(album.CoverData);

                if (Config.SaveCovers())
                {
                    string CoverPath = TidalTool.getAlbumCoverPath(Config.OutputDir(), album, Config.AddYear());
                    FileHelper.Write(album.CoverData, true, CoverPath);
                }

                for (int i = 0; album.Tracks != null && i < album.Tracks.Count; i++)
                    if(album.Tracks[i].WaitDownload)
                        ItemList.Add(new TrackTask(album.Tracks[i], ItemList.Count + 1, RecieveDownloadOver, album));
                for (int i = 0; album.Videos != null && i < album.Videos.Count; i++)
                    if(album.Videos[i].WaitDownload)
                        ItemList.Add(new VideoTask(album.Videos[i], ItemList.Count + 1, RecieveDownloadOver, album));
            }
            else if (data.GetType() == typeof(Video))
            {
                Video video = (Video)data;
                Title = video.Title;
                BasePath = TidalTool.getVideoFolder(Config.OutputDir());
                Desc = string.Format("by {0}-{1}", video.Artist.Name, TimeHelper.ConverIntToString(video.Duration));
                Cover = AIGS.Common.Convert.ConverByteArrayToBitmapImage(video.CoverData);

                if(video.WaitDownload)
                    ItemList.Add(new VideoTask(video, 1, RecieveDownloadOver));
            }
            else if (data.GetType() == typeof(Artist))
            {
                Artist artist = (Artist)data;
                Title = artist.Name;
                BasePath = TidalTool.getArtistFolder(Config.OutputDir(), artist);
                Desc = string.Format("by {0} Albums-{1}", artist.Name, artist.Albums.Count);
                Cover = AIGS.Common.Convert.ConverByteArrayToBitmapImage(artist.CoverData);

                foreach (var item in artist.Albums)
                {
                    if (!item.WaitDownload)
                        continue;
                    for (int i = 0; item.Tracks != null && i < item.Tracks.Count; i++)
                        ItemList.Add(new TrackTask(item.Tracks[i], ItemList.Count + 1, RecieveDownloadOver, item));
                    for (int i = 0; item.Videos != null && i < item.Videos.Count; i++)
                        ItemList.Add(new VideoTask(item.Videos[i], ItemList.Count + 1, RecieveDownloadOver, item));
                }
            }
            else if (data.GetType() == typeof(Playlist))
            {
                Playlist plist = (Playlist)data;
                Title = plist.Title;
                BasePath = TidalTool.getPlaylistFolder(Config.OutputDir(), plist);
                Desc = string.Format("{0} Tracks-{1} Videos-{2}", TimeHelper.ConverIntToString(plist.Duration), plist.NumberOfTracks, plist.NumberOfVideos);
                Cover = AIGS.Common.Convert.ConverByteArrayToBitmapImage(plist.CoverData);

                for (int i = 0; plist.Tracks != null && i < plist.Tracks.Count; i++)
                    if(plist.Tracks[i].WaitDownload)
                        ItemList.Add(new TrackTask(plist.Tracks[i], ItemList.Count + 1, RecieveDownloadOver, null, plist));
                for(int i = 0; plist.Videos != null && i < plist.Videos.Count; i++)
                    if(plist.Videos[i].WaitDownload)
                        ItemList.Add(new VideoTask(plist.Videos[i], ItemList.Count + 1, RecieveDownloadOver, null, plist));
            }
        }

        public void RecieveDownloadOver()
        {
            lock (this)
            {
                bool bError = false;
                bool bAllOver = true;
                ProgressHelper.STATUS Status = ProgressHelper.STATUS.RUNNING;
                foreach (var item in ItemList)
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
            if (MaxHeight <= 0)
                MaxHeight = 1000;
            else
                MaxHeight = 0;
        }

        public void Delete()
        {
            if (MessageBox.Show("Remove task?", "Info", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;

            foreach (var item in ItemList)
            {
                if (item.GetType() == typeof(TrackTask))
                    ((TrackTask)item).Cancel();
                else if (item.GetType() == typeof(VideoTask))
                    ((VideoTask)item).Cancel();
            }
            VMParent.DeleteTask(this);
        }

        public void Retry()
        {

        }

        public void OpenFolder() => Process.Start(BasePath);
        #endregion
    }

}
