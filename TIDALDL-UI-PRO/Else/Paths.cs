using AIGS.Helper;
using AIGS.Common;
using TidalLib;

namespace TIDALDL_UI.Else
{
    public class Paths 
    {
        public static string GetBasePath()
        {
            return $"{SystemHelper.GetUserFolders().PersonalPath}/Tidal-gui/";
        }

        public static string GetSettingsPath()
        {
            return $"{GetBasePath()}/data/settings.json";
        }

        public static string GetUserSettingsPath()
        {
            return $"{GetBasePath()}/data/usersettings.json";
        }

        public static string GetUpdatePath()
        {
            return $"{GetBasePath()}/update/";
        }

        public static string GetUpdateShellPath()
        {
            return $"{GetUpdatePath()}/update.bat";
        }

        private static string FormatPath(string fileName)
        {
            string sRet = PathHelper.ReplaceLimitChar(fileName, "-").Trim();
            return sRet;
        }

        private static string FormatYear(string releaseDate)
        {
            return releaseDate != null ? releaseDate.Substring(0, 4) : "";
        }

        private static string FormatExtension(StreamUrl stream)
        {
            if (stream.Url.Contains(".flac"))
                return ".flac";
            if (stream.Url.Contains(".mp4"))
            {
                if (stream.Codec.Contains("ac4") || stream.Codec.Contains("mha1"))
                    return ".mp4";
            }
            return ".m4a";
        }

        public static string GetAlbumPath(Album album)
        {
            var artistName = FormatPath(Client.GetArtistsName(album.Artists));
            var albumArtistName = "";
            if (album.Artist != null)
                albumArtistName = FormatPath(album.Artist.Name);

            // album folder pre: [ME]
            string flag = Client.GetFlag(album, eType.ALBUM, true, "");
            if (Global.Settings.AudioQuality != eAudioQuality.Master)
                flag = flag.Replace("M", "");
            if (flag.IsNotBlank())
                flag = $"[{flag}]";

            // album and addyear
            var albumName = FormatPath(album.Title);
            var year = FormatYear(album.ReleaseDate);

            var retpath = Global.Settings.AlbumFolderFormat;
            if (retpath.IsBlank())
                retpath = "{ArtistName}/{Flag} [{AlbumID}] [{AlbumYear}] {AlbumTitle}";

            retpath = retpath.Replace("{ArtistName}", artistName);
            retpath = retpath.Replace("{AlbumArtistName}", albumArtistName);
            retpath = retpath.Replace("{Flag}", flag);
            retpath = retpath.Replace("{AlbumID}", album.ID);
            retpath = retpath.Replace("{AlbumYear}", year);
            retpath = retpath.Replace("{AlbumTitle}", albumName);
            retpath = retpath.Replace("{AudioQuality}", album.AudioQuality);
            retpath = retpath.Replace("{DurationSeconds}", album.Duration.ToString());
            retpath = retpath.Replace("{Duration}", album.DurationStr);
            retpath = retpath.Replace("{NumberOfTracks}", album.NumberOfTracks.ToString());
            retpath = retpath.Replace("{NumberOfVideos}", album.NumberOfVideos.ToString());
            retpath = retpath.Replace("{NumberOfVolumes}", album.NumberOfVolumes.ToString());
            retpath = retpath.Replace("{ReleaseDate}", album.ReleaseDate ?? "");
            retpath = retpath.Replace("{RecordType}", album.Type ?? "");
            retpath = retpath.Replace("{None}", "");
            retpath = retpath.Trim();

            return $"{Global.Settings.OutputDir}/{retpath}";
        }

        public static string GetPlaylistPath(Playlist playlist)
        {
            var name = FormatPath(playlist.Title);
            return $"{Global.Settings.OutputDir}/Playlist/{name}";
        }

        public static string GetTrackPath(Track track, StreamUrl stream, Album album = null, Playlist playlist = null)
        {
            string basepath = $"{Global.Settings.OutputDir}/";
            string number = track.TrackNumber.ToString().PadLeft(2, '0');
            if (album != null)
            {
                basepath = GetAlbumPath(album);
                if (album.NumberOfVolumes > 1)
                    basepath += $"CD{track.TrackNumber}";
            }

            if (playlist != null && Global.Settings.UsePlaylistFolder)
            {
                basepath = GetPlaylistPath(playlist);
                number = (playlist.Tracks.IndexOf(track) + 1).ToString().PadLeft(2, '0');
            }

            //# artist
            var artists = FormatPath(Client.GetArtistsName(track.Artists));
            var artist = "";
            if (track.Artist != null)
                artist = FormatPath(track.Artist.Name);

            //# title
            var title = FormatPath(track.Title);
            if (track.Version != null)
                title += $" ({FormatPath(track.Version)})";

            //# explicit
            var sexplicit = track.Explicit ? "(Explicit)" : "";

            //# album and addyear
            var albumName = FormatPath(album.Title);
            var year = FormatYear(album.ReleaseDate);

            //# extension
            var extension = FormatExtension(stream);

            var retpath = Global.Settings.TrackFileFormat;
            if (retpath.IsBlank())
                retpath = "{TrackNumber} - {ArtistName} - {TrackTitle}{ExplicitFlag}";

            retpath = retpath.Replace("{TrackNumber}", number);
            retpath = retpath.Replace("{ArtistName}", artist);
            retpath = retpath.Replace("{ArtistsName}", artists);
            retpath = retpath.Replace("{TrackTitle}", title);
            retpath = retpath.Replace("{ExplicitFlag}", sexplicit);
            retpath = retpath.Replace("{AlbumYear}", year);
            retpath = retpath.Replace("{AlbumTitle}", albumName);
            retpath = retpath.Replace("{AudioQuality}", track.AudioQuality);
            retpath = retpath.Replace("{DurationSeconds}", track.Duration.ToString());
            retpath = retpath.Replace("{Duration}", track.DurationStr);
            retpath = retpath.Replace("{TrackID}", track.ID.ToString());
            retpath = retpath.Trim();
            return $"{basepath}/{retpath}{extension}";
        }

        public static string GetVideoPath(Video video, Album album = null, Playlist playlist = null)
        {
            string basepath = $"{Global.Settings.OutputDir}/Video/";
            if (album != null)
                basepath = GetAlbumPath(album);
            else if (playlist != null)
                basepath = GetPlaylistPath(playlist);

            //# get number
            var number = video.TrackNumber.ToString().PadLeft(2, '0');

            //# get artist
            var artists = FormatPath(Client.GetArtistsName(video.Artists));
            var artist = "";
            if (video.Artist != null)
                artist = FormatPath(video.Artist.Name);

            //# explicit
            var sexplicit = video.Explicit ? "(Explicit)" : "";

            //# title
            var title = FormatPath(video.Title);
            if (video.Version != null)
                title += $" ({FormatPath(video.Version)})";

            //# album and addyear
            var year = FormatYear(video.ReleaseDate);
            var extension = ".mp4";

            var retpath = Global.Settings.VideoFileFormat;
            if (retpath.IsBlank())
                retpath = "{ArtistName}/{TrackNumber} - {VideoTitle}{ExplicitFlag}";

            retpath = retpath.Replace("{VideoNumber}", number);
            retpath = retpath.Replace("{ArtistName}", artist);
            retpath = retpath.Replace("{ArtistsName}", artists);
            retpath = retpath.Replace("{VideoTitle}", title);
            retpath = retpath.Replace("{ExplicitFlag}", sexplicit);
            retpath = retpath.Replace("{VideoYear}", year);
            retpath = retpath.Replace("{VideoID}", video.ID);
            retpath = retpath.Trim();
            return $"{basepath}/{retpath}{extension}";
        }
    }
}