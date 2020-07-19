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
using Tidal;
using TIDALDL_UI.Else;

namespace TIDALDL_UI.Pages
{
    public class SearchViewModel : Screen
    {
        //wait dialog
        public Visibility WaitPageEnable { get; set; } = Visibility.Hidden;
        public Visibility ShowCoverFlow { get; set; } = Visibility.Visible;
        public MainViewModel MainView;

        //search parameter
        eObjectType SearchType = eObjectType.None;
        public SearchResult SearchResult { get; set; }

        //detail parameter
        public Visibility ShowDetailView { get; set; } = Visibility.Hidden;
        public bool InGetDetail { get; set; }
        public DetailRecord Detail { get; set; }
        public bool AllCheck { get; set; }

        //quality and resolution
        public int SelectedIndexTrackQuality { get; set; } = Config.QualityIndex();
        public int SelectedValueVideoResolution { get; set; } = Config.ResolutionValue();
        public static Dictionary<int, string> ComboxTrackQuality { get; set; } = AIGS.Common.Convert.ConverEnumToDictionary(typeof(Tidal.eSoundQuality), false);
        public static Dictionary<int, string> ComboxVideoResolution { get; set; } = AIGS.Common.Convert.ConverEnumToDictionary(typeof(Tidal.eResolution), false);

        #region CoverFolw
        public void SearchByCoverFlow(string key)
        {
            int iIndex = ((SearchView)this.View).ctrlCoverFlow1.PageIndex;
            if(key != "1")
                iIndex = ((SearchView)this.View).ctrlCoverFlow2.PageIndex;
            string[] sAlbumID = { "103579473", "27633968", "103578093", "118390427", "33826047", "58990484" };
            Search(sAlbumID[iIndex]);
        }

        #endregion

        #region search
        public async void Search(string SearchStr)
        {
            
            
            ShowCoverFlow = Visibility.Hidden;
            if (SearchStr.IsBlank())
            {
                Growl.Error("Search string is empty!", "SearchMsg");
                return;
            }

            //Search
            ShowWait();
            object SearchObj = await Task.Run(() => { return TidalTool.tryGet(SearchStr, out SearchType); });
            if (SearchType == eObjectType.None)
            {
                Growl.Error("Search Err!", "SearchMsg");
            }
            else if (SearchType == Tidal.eObjectType.SEARCH)
            {
                SearchResult = ((Tidal.SearchResult)SearchObj);
                ShowDetailView = Visibility.Hidden;
            }
            else
            {
                LoadDetail(SearchObj);
                ShowDetailView = Visibility.Visible;
            }
            ShowWait(false);
        }
        #endregion

        #region Left/Right/Show wait
        public void GoLeft()
        {
            ShowDetailView = Visibility.Hidden;
        }

        public void GoRight()
        {
            ShowDetailView = Visibility.Visible;
        }

        public void ShowWait(bool check = true)
        {
            if (check)
                WaitPageEnable = Visibility.Visible;
            else
                WaitPageEnable = Visibility.Hidden;
        }
        #endregion

        #region Quality change
        public void ChangeQuality()
        {
            string sValue = AIGS.Common.Convert.ConverEnumToString(SelectedIndexTrackQuality, typeof(Tidal.eSoundQuality), 0);
            Config.Quality(sValue);
        }

        public void ChangeResolution()
        {
            string sValue = AIGS.Common.Convert.ConverEnumToString(SelectedValueVideoResolution, typeof(Tidal.eResolution), 0);
            Config.Resolution(sValue);
        }
        #endregion

        #region Detail
        public async void GetDetail()
        {
            if (SearchResult == null)
            {
                Growl.Error("Please search first!", "SearchMsg");
                return;
            }

            ShowWait();
            string sID = null;
            object oObject = null;
            eObjectType oType = eObjectType.None;
            eObjectType inType = eObjectType.None;

            string selectHeader = ((System.Windows.Controls.TabItem)((SearchView)this.View).ctrSearchTab.SelectedItem).Header.ToString();
            if (selectHeader == "ALBUM")
            {
                if (((SearchView)this.View).ctrAlbumGrid.SelectedIndex < 0)
                    goto ERR_NO_SELECT;
                sID = SearchResult.Albums[((SearchView)this.View).ctrAlbumGrid.SelectedIndex].ID.ToString();
                inType = eObjectType.ALBUM;
            }
            else if(selectHeader == "TRACK")
            {
                if (((SearchView)this.View).ctrTrackGrid.SelectedIndex < 0)
                    goto ERR_NO_SELECT;
                sID = SearchResult.Tracks[((SearchView)this.View).ctrTrackGrid.SelectedIndex].ID.ToString();
                inType = eObjectType.TRACK;
            }
            else if (selectHeader == "VIDEO")
            {
                if (((SearchView)this.View).ctrVideoGrid.SelectedIndex < 0)
                    goto ERR_NO_SELECT;
                sID = SearchResult.Videos[((SearchView)this.View).ctrVideoGrid.SelectedIndex].ID.ToString();
                inType = eObjectType.VIDEO;
            }
            else if (selectHeader == "ARTIST")
            {
                if (((SearchView)this.View).ctrArtistGrid.SelectedIndex < 0)
                    goto ERR_NO_SELECT;
                sID = SearchResult.Artists[((SearchView)this.View).ctrArtistGrid.SelectedIndex].ID.ToString();
                inType = eObjectType.ARTIST;
            }
            else if (selectHeader == "PLAYLIST")
            {
                if (((SearchView)this.View).ctrPlaylistGrid.SelectedIndex < 0)
                    goto ERR_NO_SELECT;
                sID = SearchResult.Playlists[((SearchView)this.View).ctrPlaylistGrid.SelectedIndex].UUID;
                inType = eObjectType.PLAYLIST;
            }

            oObject = await Task.Run(() => { return TidalTool.tryGet(sID, out oType, inType); });
            LoadDetail(oObject);
            ShowDetailView = Visibility.Visible;

            ShowWait(false);
            return;

        ERR_NO_SELECT:
            Growl.Error("Please select one item!", "SearchMsg");
            ShowWait(false);
            return;
        }

        public void CheckChange()
        {
            if (Detail == null || Detail.ItemList == null)
                return;
            foreach (var item in Detail.ItemList)
            {
                if (item.Data.GetType() == typeof(Album))
                    ((Album)item.Data).WaitDownload = AllCheck;
                else if (item.Data.GetType() == typeof(Track))
                    ((Track)item.Data).WaitDownload = AllCheck;
                else if (item.Data.GetType() == typeof(Video))
                    ((Video)item.Data).WaitDownload = AllCheck;
            }
            return;
        }

        public void LoadDetail(object data)
        {
            if (data == null)
                return;

            Detail = new DetailRecord();
            Detail.Data = data;
            if (data.GetType() == typeof(Album))
            {
                Album album = (Album)data;
                Detail.Header = "ALBUMINFO";
                Detail.Title = album.Title;
                Detail.Intro = string.Format("by {0}-{1} Tracks-{2} Videos-{3}", album.Artist.Name, TimeHelper.ConverIntToString(album.Duration), album.NumberOfTracks, album.NumberOfVideos);
                Detail.Cover = AIGS.Common.Convert.ConverByteArrayToBitmapImage(album.CoverData);
                Detail.ReleaseDate = "Release date " + album.ReleaseDate;
                Detail.ItemList = new ObservableCollection<DetailRecord.Item>();
                if (album.Tracks != null)
                {
                    foreach (Track item in album.Tracks)
                    {
                        item.WaitDownload = true;
                        Detail.ItemList.Add(new DetailRecord.Item(item.Title, TimeHelper.ConverIntToString(item.Duration), item.Album.Title, item, flag: TidalTool.getFlag(item)));
                    }
                }
                if (album.Videos != null)
                {
                    foreach (Video item in album.Videos)
                    {
                        item.WaitDownload = true;
                        Detail.ItemList.Add(new DetailRecord.Item(item.Title, TimeHelper.ConverIntToString(item.Duration), item.Album.Title, item, "VIDEO", flag: TidalTool.getFlag(item)));
                    }
                }

                //FlagDetail
                if (album.AudioQuality == "HI_RES")
                    Detail.FlagDetail += "Master ";
                if (album.Explicit)
                    Detail.FlagDetail += "Explicit";

                //Modes
                if (album.AudioModes != null && album.AudioModes.Length > 0)
                {
                    foreach (var item in album.AudioModes)
                        Detail.Modes += item + " ";
                }
            }
            else if (data.GetType() == typeof(Video))
            {
                Video video = (Video)data;
                video.WaitDownload = true;
                Detail.Header = "VIDEOINFO";
                Detail.Title = video.Title;
                Detail.Cover = AIGS.Common.Convert.ConverByteArrayToBitmapImage(video.CoverData);
                Detail.Intro = string.Format("by {0}-{1}", video.Artist.Name, TimeHelper.ConverIntToString(video.Duration));
                Detail.ReleaseDate = "Release date " + video.ReleaseDate;
                Detail.ItemList = new ObservableCollection<DetailRecord.Item>();
                Detail.ItemList.Add(new DetailRecord.Item(video.Title, TimeHelper.ConverIntToString(video.Duration), video.Album == null ? "" : video.Album.Title, data, "VIDEO"));

                if (video.Explicit)
                    Detail.FlagDetail += "Explicit";
                //Detail.Modes += video.Quality;
            }
            else if (data.GetType() == typeof(Artist))
            {
                Artist artist = (Artist)data;
                Detail.Header = "ARTISTINFO";
                Detail.Title = artist.Name;
                Detail.Intro = string.Format("by {0} Albums-{1}", artist.Name, artist.Albums.Count);
                Detail.Cover = AIGS.Common.Convert.ConverByteArrayToBitmapImage(artist.CoverData);
                Detail.ReleaseDate = "";
                Detail.ItemList = new ObservableCollection<DetailRecord.Item>();
                if (artist.Albums != null)
                {
                    foreach (Album item in artist.Albums)
                    {
                        item.WaitDownload = true;
                        Detail.ItemList.Add(new DetailRecord.Item(item.Title, TimeHelper.ConverIntToString(item.Duration), item.Title, item, "ALBUM", flag: TidalTool.getFlag(item)));
                    }
                }
            }
            else if (data.GetType() == typeof(Playlist))
            {
                Playlist plist = (Playlist)data;
                Detail.Header = "PLAYLISTINFO";
                Detail.Title = plist.Title;
                Detail.Intro = string.Format("{0} Tracks-{1} Videos-{2}", TimeHelper.ConverIntToString(plist.Duration), plist.NumberOfTracks, plist.NumberOfVideos);
                Detail.Cover = AIGS.Common.Convert.ConverByteArrayToBitmapImage(plist.CoverData);
                Detail.ReleaseDate = "";
                Detail.ItemList = new ObservableCollection<DetailRecord.Item>();
                if (plist.Tracks != null)
                {
                    foreach (Track item in plist.Tracks)
                    {
                        item.WaitDownload = true;
                        Detail.ItemList.Add(new DetailRecord.Item(item.Title, TimeHelper.ConverIntToString(item.Duration), item.Album.Title, item, flag: TidalTool.getFlag(item)));
                    }
                }
                if (plist.Videos != null)
                {
                    foreach (Video item in plist.Videos)
                    {
                        item.WaitDownload = true;
                        Detail.ItemList.Add(new DetailRecord.Item(item.Title, TimeHelper.ConverIntToString(item.Duration), item.Title, item, "VIDEO"));
                    }
                }
            }
        }
        #endregion

        public void Download()
        {
            if(Detail == null)
            {
                Growl.Error("Nothing to downlond!", "SearchMsg");
                return;
            }
            //remove uncheck item
            if(RemoveItems() != true)
            {
                Growl.Error("Nothing to downlond!", "SearchMsg");
                return;
            }
            //add task
            MainView.VMDownload.AddTask(Detail.Data);
            //go to download page
            MainView.ShowDownload();
        }

        private bool RemoveItems()
        {
            //for (int i = 0; i < Detail.ItemList.Count; i++)
            //{


            //    if (Detail.ItemList[i].Check)
            //        continue;

            //    if (Detail.Data.GetType() == typeof(Video) || Detail.Data.GetType() == typeof(Track))
            //        return false;
            //    if (Detail.Data.GetType() == typeof(Album))
            //    {
            //        Album album = (Album)Detail.Data;
            //        if (Detail.ItemList[i].Type == "TRACK")
            //            album.Tracks.Remove((Track)Detail.ItemList[i].Data);
            //        else
            //            album.Videos.Remove((Video)Detail.ItemList[i].Data);
            //    }
            //    if (Detail.Data.GetType() == typeof(Playlist))
            //    {
            //        Playlist plist = (Playlist)Detail.Data;
            //        if (Detail.ItemList[i].Type == "TRACK")
            //            plist.Tracks.Remove((Track)Detail.ItemList[i].Data);
            //        else
            //            plist.Videos.Remove((Video)Detail.ItemList[i].Data);
            //    }
            //    if (Detail.Data.GetType() == typeof(Artist))
            //    {
            //        Artist artist = (Artist)Detail.Data;
            //        artist.Albums.Remove((Album)Detail.ItemList[i].Data);
            //    }
            //}

            //if (Detail.Data.GetType() == typeof(Album))
            //{
            //    Album album = (Album)Detail.Data;
            //    if (album.Tracks.Count <= 0 && album.Videos.Count <= 0)
            //        return false;
            //}
            //if (Detail.Data.GetType() == typeof(Artist))
            //{
            //    Artist artist = (Artist)Detail.Data;
            //    if (artist.Albums.Count <= 0)
            //        return false;
            //}
            //if (Detail.Data.GetType() == typeof(Playlist))
            //{
            //    Playlist plist = (Playlist)Detail.Data;
            //    if (plist.Tracks.Count <= 0 && plist.Videos.Count <= 0)
            //        return false;
            //}
            return true;
        }
    }
}
