using AIGS.Common;
using AIGS.Helper;
using Genius;
using HtmlAgilityPack;
using MediaToolkit;
using MediaToolkit.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TagLib;
using TidalLib;

namespace TIDALDL_UI.Else
{
    public class Tools
    {
        public static LoginKey GetKey(bool bVideo = false)
        {
            if (Global.AccessKey != null)
                return Global.AccessKey;

            if (bVideo && Global.VideoKey != null)
                return Global.VideoKey;

            return Global.CommonKey;
        }

        private static string FormatPath(string fileName, Settings settings, bool bIsDir = true)
        {
            string sRet = PathHelper.ReplaceLimitChar(fileName, "-");
            if(sRet.IsNotBlank())
            {
                if (bIsDir && sRet.Length > settings.MaxDirName)
                    sRet = sRet.Substring(0, settings.MaxDirName);
                if (!bIsDir && sRet.Length > settings.MaxFileName)
                    sRet = sRet.Substring(0, settings.MaxFileName);
            }
            sRet = sRet.Trim();
            return sRet;
        }

        private static string getExtension(string DlUrl)
        {
            if (DlUrl.IndexOf(".flac") >= 0)
                return ".flac";
            if (DlUrl.IndexOf(".mp4") >= 0)
                return ".mp4";
            return ".m4a";
        }

        public static string GetArtistPath(Artist artist, Settings settings)
        {
            string basepath = $"{settings.OutputDir}/Album/{FormatPath(artist.Name, settings)}/";
            return basepath;
        }

        //{ArtistName}/{Flag} [{AlbumID}] [{AlbumYear}] {AlbumTitle}
        public static string GetAlbumPath(Album album, Settings settings)
        {
            try
            {
                // outputdir/Album/
                string basepath = $"{settings.OutputDir}/Album/";
                // string basepath = $"{settings.OutputDir}/Album/{FormatPath(album.Artists[0].Name, settings)}";

                // album folder pre: [ME][ID]
                string flag = Client.GetFlag(album, eType.ALBUM, true, "");
                if (settings.AudioQuality != eAudioQuality.Master)
                    flag = flag.Replace("M", "");
                if (flag.IsNotBlank())
                    flag = $"[{flag}]";

                string artist = FormatPath(string.Join(", ", album.Artists.Select(an_artist => an_artist.Name)), settings);

                string name = settings.AlbumFolderFormat;
                if (name.IsBlank())
                    name = "{ArtistName}/{Flag} [{AlbumID}] [{AlbumYear}] {AlbumTitle}";
                name = name.Replace("{ArtistName}", artist);
                name = name.Replace("{AlbumID}", album.ID);
                name = name.Replace("{AlbumYear}", album.ReleaseDate != null ? album.ReleaseDate.Substring(0, 4) : "");
                name = name.Replace("{AlbumTitle}", FormatPath(album.Title, settings));
                name = name.Replace("{Flag}", flag);
                name = name.Trim();
                return $"{basepath}/{name}/";
            }
            catch
            {
                return null;
            }

        }
        public static string GetAlbumPath2(Album album, Settings settings)
        {
            // outputdir/Album/artist/
            string basepath = $"{settings.OutputDir}/Album/{FormatPath(album.Artists[0].Name, settings)}";

            // album folder pre: [ME][ID]
            string flag = Client.GetFlag(album, eType.ALBUM, true, "");
            if (settings.AudioQuality != eAudioQuality.Master)
                flag = flag.Replace("M", "");
            if (flag.IsNotBlank())
                flag = $"[{flag}]";

            string id = "";
            if(settings.AddAlbumIDBeforeFolder)
                id = $"[{album.ID}]";

            string pre = flag + id;

            // album and addyear
            string albumName = FormatPath(album.Title, settings);
            string addyear = album.ReleaseDate == null ? "" : $"[{album.ReleaseDate.Substring(0, 4)}]";

            if (settings.AddYear == ePositionYear.After)
                albumName = albumName + addyear;
            if (settings.AddYear == ePositionYear.Before)
                albumName = addyear + albumName;

            string path = $"{basepath}/{pre}{albumName}/";
            return path;
        }

        public static string GetPlaylistPath(Playlist plist, Settings settings)
        {
            string path = $"{settings.OutputDir}/Playlist/{FormatPath(plist.Title, settings)}/";
            return path;
        }

        // "{TrackNumber} - {ArtistName} - {TrackTitle}{ExplicitFlag}";
        public static string GetTrackPath(Settings settings, Track track, StreamUrl stream, Album album, Playlist playlist = null)
        {
            string number = track.TrackNumber.ToString().PadLeft(2, '0');
            if (playlist != null)
                number = (playlist.Tracks.IndexOf(track) + 1).ToString().PadLeft(2, '0');

            string artist = FormatPath(string.Join(", ", track.Artists.Select(an_artist => an_artist.Name)), settings, false);

            //get explicit
            string sexplicit = "";
            if (track.Explicit)
                sexplicit = "(Explicit)";

            //get version
            string version = "";
            if (track.Version.IsNotBlank())
                version = " (" + track.Version + ")";

            //get title
            string title = FormatPath(track.Title + version, settings, false);

            //get extension
            string extension = getExtension(stream.Url);

            //base path
            string basepath = null;
            if (playlist == null)
            {
                basepath = GetAlbumPath(album, settings);
                if (album.NumberOfVolumes > 1)
                    basepath += $"CD{track.VolumeNumber}/";
            }
            else
                basepath = GetPlaylistPath(playlist, settings);

            string name = settings.TrackFileFormat;
            if (name.IsBlank())
                name = "{TrackNumber} - {ArtistName} - {TrackTitle}{ExplicitFlag}";
            name = name.Replace("{TrackNumber}", number);
            name = name.Replace("{ArtistName}", artist);
            name = name.Replace("{TrackTitle}", title);
            name = name.Replace("{ExplicitFlag}", sexplicit);

            if (album != null)
            {
                name = name.Replace("{AlbumID}", album.ID);
                name = name.Replace("{AlbumYear}", album.ReleaseDate == null ? "" : album.ReleaseDate.Substring(0, 4));
                name = name.Replace("{AlbumTitle}", FormatPath(album.Title, settings));
            }
            return $"{basepath}{name}{extension}";
        }

        // number - artist - title(version)(Explicit).flac
        public static string GetTrackPath2(Settings settings, Track track, StreamUrl stream, Album album, Playlist playlist = null)
        {
            //hyphen
            string hyphen = " ";
            if (settings.AddHyphen)
                hyphen = " - ";

            //get number
            string number = "";
            if (settings.UseTrackNumber)
            {
                number = track.TrackNumber.ToString().PadLeft(2, '0') + hyphen;
                if(playlist != null)
                    number = (playlist.Tracks.IndexOf(track) + 1).ToString().PadLeft(2, '0') + hyphen;
            }

            //get artist
            string artist = "";
            if (settings.ArtistBeforeTitle)
                artist = FormatPath(track.Artists[0].Name, settings, false) + hyphen;

            //get explicit
            string sexplicit = "";
            if(settings.AddExplicitTag && track.Explicit)
                sexplicit = "(Explicit)";

            //get version
            string version = "";
            if (track.Version.IsNotBlank())
                version = "(" + track.Version + ")";

            //get title
            string title = FormatPath(track.Title, settings, false);

            //get extension
            string extension = getExtension(stream.Url);

            //base path
            string basepath = null;
            if (playlist == null)
            {
                basepath = GetAlbumPath(album, settings);
                if (album.NumberOfVolumes > 1)
                    basepath += $"CD{track.VolumeNumber}/";
            }
            else
                basepath = GetPlaylistPath(playlist, settings);

            string path = $"{basepath}{number}{artist}{title}{version}{sexplicit}{extension}";
            return path;
        }

        // {ArtistName}/{TrackNumber} - {VideoTitle}.mp4
        public static string GetVideoPath(Settings settings, Video video, Album album, Playlist playlist = null, string ext = ".mp4")
        {
            //get number
            string number = video.TrackNumber.ToString().PadLeft(2, '0');
            if (playlist != null)
                number = (playlist.Videos.IndexOf(video) + 1).ToString().PadLeft(2, '0');

            //get artist
            string artist = FormatPath(string.Join(", ", video.Artists.Select(an_artist => an_artist.Name)), settings, false);

            //get explicit
            string sexplicit = "";
            if (video.Explicit)
                sexplicit = "(Explicit)";

            //get title
            string title = FormatPath(video.Title, settings, false);

            //base path
            string basepath = null;
            if (album != null)
                basepath = GetAlbumPath(album, settings);
            else if (playlist != null)
                basepath = GetPlaylistPath(playlist, settings);

            if (basepath == null)
                basepath = $"{settings.OutputDir}/Video/";

            string name = settings.VideoFileFormat;
            if (name.IsBlank())
                name = "{ArtistName}/{TrackNumber} - {VideoTitle}";
            name = name.Replace("{TrackNumber}", number);
            name = name.Replace("{ArtistName}", artist);
            name = name.Replace("{VideoTitle}", title);
            name = name.Replace("{ExplicitFlag}", sexplicit);

            return $"{basepath}{name}{ext}";
        }

        // number - artist - title(Explicit).mp4
        public static string GetVideoPath2(Settings settings, Video video, Album album, Playlist playlist = null, string ext = ".mp4")
        {
            //hyphen
            string hyphen = " ";
            if (settings.AddHyphen)
                hyphen = " - ";

            //get number
            string number = "";
            if (settings.UseTrackNumber)
            {
                number = video.TrackNumber.ToString().PadLeft(2, '0') + hyphen;
                if (playlist != null)
                    number = (playlist.Videos.IndexOf(video) + 1).ToString().PadLeft(2, '0') + hyphen;
            }

            //get artist
            string artist = "";
            if (settings.ArtistBeforeTitle)
                artist = FormatPath(video.Artists[0].Name, settings, false) + hyphen;

            //get explicit
            string sexplicit = "";
            if (settings.AddExplicitTag && video.Explicit)
                sexplicit = "(Explicit)";

            //get title
            string title = FormatPath(video.Title, settings, false);

            //base path
            string basepath = null;
            if (album != null)
                basepath = GetAlbumPath(album, settings);
            else if (playlist != null)
                basepath = GetPlaylistPath(playlist, settings);
            else
                basepath = $"{settings.OutputDir}/Video/";

            string path = $"{basepath}{number}{artist}{title}{sexplicit}{ext}";
            return path;
        }

        public static (string, string) ConvertMp4ToM4a(string path, StreamUrl stream)
        {
            if (path.ToLower().Contains(".mp4") == false)
                return (null,path);

            if (stream.Codec == "ac4" || stream.Codec == "mha1")
                return (null, path);

            try
            {
                string newpath = path.Replace(".mp4", ".m4a");
                var inputFile = new MediaFile { Filename = path };
                var outputFile = new MediaFile { Filename = newpath };
                using (var engine = new Engine())
                {
                    engine.Convert(inputFile, outputFile);
                }

                System.IO.File.Delete(path);
                return (null, newpath);
            }
            catch(Exception e)
            {
                return (e.Message, path);
            }
        }

        public static (string, string) ConvertTsToMp4(string path)
        {
            if (path.ToLower().Contains(".ts") == false)
                return (null, path);

            try
            {
                string newpath = path.Replace(".ts", ".mp4");
                var inputFile = new MediaFile { Filename = path };
                var outputFile = new MediaFile { Filename = newpath };
                using (var engine = new Engine())
                {
                    engine.Convert(inputFile, outputFile);
                }

                System.IO.File.Delete(path);
                return (null, newpath);
            }
            catch (Exception e)
            {
                return (e.Message, path);
            }
        }




        public static string SetMetaData(string filepath, Album TidalAlbum, Track TidalTrack, string lyrics = "")
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
                tfile.Tag.AlbumArtists = Client.GetArtistsList(TidalAlbum.Artists);
                tfile.Tag.Performers = Client.GetArtistsList(TidalTrack.Artists);
                tfile.Tag.Lyrics = lyrics;

                //ReleaseDate
                if (TidalAlbum.ReleaseDate != null && TidalAlbum.ReleaseDate.IsNotBlank())
                    tfile.Tag.Year = (uint)AIGS.Common.Convert.ConverStringToInt(TidalAlbum.ReleaseDate.Split("-")[0]);

                //Cover
                var pictures = new Picture[1];
                pictures[0] = new Picture(NetHelper.DownloadData(TidalAlbum.CoverHighUrl));
                tfile.Tag.Pictures = pictures;
                tfile.Save();
                return null;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }



        #region Decrypt File
        static byte[] MASTER_KEY = System.Convert.FromBase64String("UIlTTEMmmLfGowo/UC60x2H45W6MdGgTRfo/umg4754=");

        private static byte[] ReadFile(string filepath)
        {
            FileStream fs = new FileStream(filepath, FileMode.Open);
            byte[] array = new byte[fs.Length];
            fs.Read(array, 0, array.Length);
            fs.Close();
            return array;
        }

        private static bool WriteFile(string filepath, byte[] txt)
        {
            try
            {
                FileStream fs = new FileStream(filepath, FileMode.Create);
                fs.Write(txt, 0, txt.Length);
                fs.Close();
                return true;
            }
            catch { return false; }
        }

        public static bool DecryptTrackFile(StreamUrl stream, string filepath)
        {
            try
            {
                if (!System.IO.File.Exists(filepath))
                    return false;
                if (stream.EncryptionKey.IsBlank())
                    return true;

                byte[] security_token = System.Convert.FromBase64String(stream.EncryptionKey);

                byte[] iv = security_token.Skip(0).Take(16).ToArray();
                byte[] str = security_token.Skip(16).ToArray();
                byte[] dec = AESHelper.Decrypt(str, MASTER_KEY, iv);

                byte[] key = dec.Skip(0).Take(16).ToArray();
                byte[] nonce = dec.Skip(16).Take(8).ToArray();
                byte[] nonce2 = new byte[16];
                nonce.CopyTo(nonce2, 0);

                byte[] txt = ReadFile(filepath);
                AES_CTR tool = new AES_CTR(key, nonce2);
                byte[] newt = tool.DecryptBytes(txt);
                bool bfalg = WriteFile(filepath, newt);
                return bfalg;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
