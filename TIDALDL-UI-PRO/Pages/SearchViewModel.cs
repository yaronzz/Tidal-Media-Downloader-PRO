using AIGS.Common;
using AIGS.Helper;
using HandyControl.Controls;
using HandyControl.Tools.Extension;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TIDALDL_UI.Else;
using TidalLib;

namespace TIDALDL_UI.Pages
{
    public class SearchViewModel : ModelBase
    {
        //show view
        public bool ShowWait { get; set; } = false;
        public bool ShowDetail { get; set; } = false;
        public bool ShowList { get; set; } = false;

        //search parameter
        public SearchResult SearchResult { get; set; }
        public ObservableCollection<CoverCard> CoverCards { get; set; }

        //detail parameter
        public Detail Detail { get; set; }

        //quality and resolution
        public eAudioQuality QualityValue { get; set; } = Settings.Read().AudioQuality;
        public eVideoQuality ResolutionValue { get; set; } = Settings.Read().VideoQuality;
        public List<eAudioQuality> ComboxTrackQuality { get; set; } = AIGS.Common.Convert.ConverEnumToList<eAudioQuality>();
        public List<eVideoQuality> ComboxVideoResolution { get; set; } = AIGS.Common.Convert.ConverEnumToList<eVideoQuality>();

        protected override async void OnViewLoaded()
        {
            CoverCards = await CoverCard.GetList();
        }

        #region search \ get detail \ download

        public async void Search(string SearchStr)
        {
            if (SearchStr.IsBlank())
            {
                Growl.Error("Search string is empty!", Global.TOKEN_MAIN);
                return;
            }

            ShowWait = true;

            (string msg, eType type, object data) = await Client.Get(Global.CommonKey, SearchStr, eType.NONE, Global.Settings.SearchNum, Global.Settings.IncludeEP, false);
            if (msg.IsNotBlank() || data == null)
            {
                Growl.Error("Search Err!" + msg, Global.TOKEN_MAIN);
            }
            else if (type == eType.SEARCH)
            {
                SearchResult = (SearchResult)data;
                ShowDetail = false;
                ShowList = true;
            }
            else
            {
                Detail = Detail.Creat(data, type);
                ShowDetail = true;
                ShowList = false;
            }

            ShowWait = false;
        }

        public async void GetDetail()
        {
            if (SearchResult == null)
            {
                Growl.Error("Please search first!", Global.TOKEN_MAIN);
                return;
            }

            ShowWait = true;

            string id = null;
            eType type = eType.NONE;
            string selectHeader = ((System.Windows.Controls.TabItem)((SearchView)this.View).ctrSearchTab.SelectedItem).Header.ToString();
            if (selectHeader == "ALBUM")
            {
                if (((SearchView)this.View).ctrAlbumGrid.SelectedIndex < 0)
                    goto ERR_NO_SELECT;
                id = SearchResult.Albums[((SearchView)this.View).ctrAlbumGrid.SelectedIndex].ID.ToString();
                type = eType.ALBUM;
            }
            else if (selectHeader == "TRACK")
            {
                if (((SearchView)this.View).ctrTrackGrid.SelectedIndex < 0)
                    goto ERR_NO_SELECT;
                id = SearchResult.Tracks[((SearchView)this.View).ctrTrackGrid.SelectedIndex].ID.ToString();
                type = eType.TRACK;
            }
            else if (selectHeader == "VIDEO")
            {
                if (((SearchView)this.View).ctrVideoGrid.SelectedIndex < 0)
                    goto ERR_NO_SELECT;
                id = SearchResult.Videos[((SearchView)this.View).ctrVideoGrid.SelectedIndex].ID.ToString();
                type = eType.VIDEO;
            }
            else if (selectHeader == "ARTIST")
            {
                if (((SearchView)this.View).ctrArtistGrid.SelectedIndex < 0)
                    goto ERR_NO_SELECT;
                id = SearchResult.Artists[((SearchView)this.View).ctrArtistGrid.SelectedIndex].ID.ToString();
                type = eType.ARTIST;
            }
            else if (selectHeader == "PLAYLIST")
            {
                if (((SearchView)this.View).ctrPlaylistGrid.SelectedIndex < 0)
                    goto ERR_NO_SELECT;
                id = SearchResult.Playlists[((SearchView)this.View).ctrPlaylistGrid.SelectedIndex].UUID;
                type = eType.PLAYLIST;
            }

            (string msg, eType otype, object data) = await Client.Get(Global.CommonKey, id, type, Global.Settings.SearchNum, Global.Settings.IncludeEP, false);

            Detail = Detail.Creat(data, type);
            ShowDetail = true;
            ShowList = false;
            ShowWait = false;
            return;

            ERR_NO_SELECT:
            Growl.Error("Please select one item!", Global.TOKEN_MAIN);
            ShowWait = false;
            return;
        }

        public async void Download()
        {
            if (Detail == null)
            {
                Growl.Error("Nothing to downlond!", Global.TOKEN_MAIN);
                return;
            }

            ShowWait = true;

            //Get data
            if(Detail.Data.GetType() == typeof(Track))
            {
                Track track = (Track)Detail.Data;
                (string msg, Album album) = await Client.GetAlbum(Global.CommonKey, track.Album.ID, false);
                if(msg.IsNotBlank() || album == null)
                {
                    Growl.Error("Get track's album information failed!", Global.TOKEN_MAIN);
                    goto RETURN_POINT;
                }
                track.Album = album;
            }
            if(Detail.Data.GetType() == typeof(Artist))
            {
                foreach (var item in Detail.Items)
                {
                    string msg = null;
                    Album album = (Album)item.Data;
                    (msg, album.Tracks, album.Videos) = await Client.GetItems(Global.CommonKey, album.ID, eType.ALBUM);
                    if (msg.IsNotBlank())
                    {
                        Growl.Error("Get artist's album information failed!", Global.TOKEN_MAIN);
                        goto RETURN_POINT;
                    }
                }
            }

            //add task
            Global.VMMain.VMDownload.AddTask(Detail);

            //go to download page
            Global.VMMain.ShowDownload();

        RETURN_POINT:
            ShowWait = false;
        }
        #endregion



        #region Left/Right/AllCheck

        public void GoLeft()
        {
            if (ShowDetail == false && ShowList == false)
                return;

            if (ShowDetail == true)
            {
                ShowDetail = false;
                ShowList = SearchResult == null ? false : true;
            }
            else if (ShowList == true)
            {
                ShowList = false;
                ShowDetail = false;
            }
        }

        public void GoRight()
        {
            if (ShowDetail)
                return;

            if (ShowList == false)
            {
                if(SearchResult != null)
                {
                    ShowList = true;
                    return;
                }
            }

            if(Detail != null)
                ShowDetail = true;
        }

        public void CheckChange(object sender, RoutedEventArgs e)
        {
            if (Detail == null || Detail.Items == null)
                return;
            foreach (var item in Detail.Items)
                item.Check = (bool)((CheckBox)sender).IsChecked;
        }

        #endregion


        #region Quality change

        public void ChangeQuality()
        {
            if (Global.Settings != null)
            {
                Global.Settings.AudioQuality = QualityValue;
                Global.Settings.Save();
            }
        }

        public void ChangeResolution()
        {
            if (Global.Settings != null)
            {
                Global.Settings.VideoQuality = ResolutionValue;
                Global.Settings.Save();
            }
        }

        #endregion
    }


    public class Detail : Screen
    {
        public object Data { get; set; }
        public string Title { get; set; }
        public string Intro { get; set; }
        public string ReleaseDate { get; set; }
        public string CoverUrl { get; set; }
        public string Flag { get; set; }       
        public ObservableCollection<Item> Items { get; set; } = new ObservableCollection<Item>();

        public class Item : Screen
        {
            public bool   Check { get; set; } = true;
            public string Title { get; set; }
            public string Type { get; set; }
            public string Flag { get; set; }
            public string Duration { get; set; }
            public string Album { get; set; }
            public object Data { get; set; }
        }

        public static Detail Creat(object data, eType type)
        {
            if (type == eType.ALBUM)
                return new Detail((Album)data);
            if (type == eType.ARTIST)
                return new Detail((Artist)data);
            if (type == eType.PLAYLIST)
                return new Detail((Playlist)data);
            if (type == eType.VIDEO)
                return new Detail((Video)data);
            if (type == eType.TRACK)
                return new Detail((Track)data);
            return null;
        }

        public Detail(Album album)
        {
            Data = album;
            Title = album.Title;
            Intro = $"by {album.ArtistsName}-{TimeHelper.ConverIntToString(album.Duration)} Tracks-{album.NumberOfTracks} Videos-{album.NumberOfVideos}";
            CoverUrl = album.CoverUrl;
            ReleaseDate = $"Release date {album.ReleaseDate}";
            Flag = album.Flag;

            for (int i = 0; i < album.NumberOfTracks; i++)
            {
                Items.Add(new Item()
                {
                    Title = album.Tracks[i].DisplayTitle,
                    Type = "Track",
                    Flag = album.Tracks[i].FlagShort,
                    Duration = album.Tracks[i].DurationStr,
                    Album = album.Tracks[i].Album.Title,
                    Data = album.Tracks[i]
                });
            }
            for (int i = 0; i < album.NumberOfVideos; i++)
            {
                Items.Add(new Item()
                {
                    Title = album.Videos[i].Title,
                    Type = "Video",
                    Flag = album.Videos[i].FlagShort,
                    Duration = TimeHelper.ConverIntToString(album.Videos[i].Duration),
                    Album = album.Videos[i].Album == null ? null : album.Videos[i].Album.Title,
                    Data = album.Videos[i]
                });
            }
        }

        public Detail(Video video)
        {
            Data = video;
            Title = video.Title;
            Intro = $"by {video.ArtistsName}-{TimeHelper.ConverIntToString(video.Duration)}";
            CoverUrl = video.CoverUrl;
            ReleaseDate = $"Release date {video.ReleaseDate}";
            Flag = video.Flag;
            Items.Add(new Item()
            {
                Title = video.Title,
                Type = "Video",
                Flag = video.FlagShort,
                Duration = TimeHelper.ConverIntToString(video.Duration),
                Album = video.Album == null ? null : video.Album.Title,
                Data = video
            });
        }

        public Detail(Track track)
        {
            Data = track;
            Title = track.Title;
            Intro = $"by {track.ArtistsName}-{TimeHelper.ConverIntToString(track.Duration)}";
            CoverUrl = track.Album.CoverUrl;
            ReleaseDate = $"Release date {track.Album.ReleaseDate}";
            Flag = track.Flag;
            Items.Add(new Item()
            {
                Title = track.DisplayTitle,
                Type = "Track",
                Flag = track.Flag,
                Duration = track.DurationStr,
                Album = track.Album == null ? null : track.Album.Title,
                Data = track
            });
        }

        public Detail(Playlist playlist)
        {
            Data = playlist;
            Title = playlist.Title;
            Intro = $"by {TimeHelper.ConverIntToString(playlist.Duration)} Tracks-{playlist.NumberOfTracks} Videos-{playlist.NumberOfVideos}";
            CoverUrl = playlist.CoverUrl;
            ReleaseDate = $"Description {playlist.Description}";

            for (int i = 0; i < playlist.NumberOfTracks; i++)
            {
                Items.Add(new Item()
                {
                    Title = playlist.Tracks[i].Title,
                    Type = "Track",
                    Flag = playlist.Tracks[i].FlagShort,
                    Duration = playlist.Tracks[i].DurationStr,
                    Album = playlist.Tracks[i].Album.Title,
                    Data = playlist.Tracks[i]
                });
            }
            for (int i = 0; i < playlist.NumberOfVideos; i++)
            {
                Items.Add(new Item()
                {
                    Title = playlist.Videos[i].Title,
                    Type = "Video",
                    Flag = playlist.Videos[i].FlagShort,
                    Duration = playlist.Videos[i].DurationStr,
                    Album = playlist.Videos[i].Album == null ? null : playlist.Videos[i].Album.Title,
                    Data = playlist.Videos[i]
                });
            }
        }

        public Detail(Artist artist)
        {
            Data = artist;
            Title = artist.Name;
            Intro = $"by {artist.Name} Albums-{artist.Albums.Count}";
            CoverUrl = artist.CoverUrl;
            ReleaseDate = $"Artist types: {string.Join(" / ", artist.ArtistTypes)}";

            for (int i = 0; i < artist.Albums.Count; i++)
            {
                Items.Add(new Item()
                {
                    Title = artist.Albums[i].Title,
                    Type = "Album",
                    Flag = artist.Albums[i].FlagShort,
                    Duration = TimeHelper.ConverIntToString(artist.Albums[i].Duration),
                    Album = artist.Albums[i].Title,
                    Data = artist.Albums[i]
                });
            }
        }
    }
}
