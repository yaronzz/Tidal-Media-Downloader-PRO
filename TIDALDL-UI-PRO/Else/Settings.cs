using AIGS.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TidalLib;

namespace TIDALDL_UI.Else
{
    public class UserSettings : Stylet.Screen
    {
        [JsonProperty("Userid")]
        public string Userid { get; set; } = null;

        [JsonProperty("Countrycode")]
        public string Countrycode { get; set; } = null;

        [JsonProperty("Accesstoken")]
        public string Accesstoken { get; set; } = null;

        [JsonProperty("Refreshtoken")]
        public string Refreshtoken { get; set; } = null;

        [JsonProperty("ProxyEnable")]
        public bool ProxyEnable { get; set; } = false;

        [JsonProperty("ProxyHost")]
        public string ProxyHost { get; set; } = null;

        [JsonProperty("ProxyPort")]
        public int ProxyPort { get; set; } = 0;

        [JsonProperty("ProxyUser")]
        public string ProxyUser { get; set; } = null;

        [JsonProperty("ProxyPwd")]
        public string ProxyPwd { get; set; } = null;

        public bool Save()
        {
            string data = JsonHelper.ConverObjectToString<UserSettings>(this, true);
            return FileHelper.Write(EncryptHelper.Encode(data, Global.KEY_BASE), true, Paths.GetUserSettingsPath());
        }

        public static UserSettings Read()
        {
            string buf = FileHelper.Read(Paths.GetUserSettingsPath());
            string data = EncryptHelper.Decode(buf, Global.KEY_BASE);
            UserSettings ret = JsonHelper.ConverStringToObject<UserSettings>(data);
            if (ret == null)
                return new UserSettings();
            return ret;
        }
    }

    public class Settings : Stylet.Screen
    {
        [JsonProperty("ThemeType")]
        public Theme.Type ThemeType { get; set; } = Theme.Type.Default;
        
        [JsonProperty("LanguageType")]
        public Language.Type LanguageType { get; set; } = Language.Type.Default;

        [JsonProperty("OutputDir")]
        public string OutputDir { get; set; } = "./download/";

        [JsonProperty("ThreadNum")]
        public int ThreadNum { get; set; } = 1;

        [JsonProperty("SearchNum")]
        public int SearchNum { get; set; } = 30;

        [JsonProperty("Version")]
        public string Version { get; set; } = null;


        [JsonProperty("AudioQuality")]
        public eAudioQuality AudioQuality { get; set; } = eAudioQuality.HiFi;

        [JsonProperty("VideoQuality")]
        public eVideoQuality VideoQuality { get; set; } = eVideoQuality.P720;

        [JsonProperty("CheckExist")]
        public bool CheckExist { get; set; } = true;

        [JsonProperty("UsePlaylistFolder")]
        public bool UsePlaylistFolder { get; set; } = true;

        [JsonProperty("IncludeEP")]
        public bool IncludeEP { get; set; } = true;

        [JsonProperty("SaveCovers")]
        public bool SaveCovers { get; set; } = true;

        [JsonProperty("SaveLyrics")]
        public bool SaveLyrics { get; set; } = true;

        [JsonProperty("SaveAlbumInfo")]
        public bool SaveAlbumInfo { get; set; } = true;

        [JsonProperty("AlbumFolderFormat")]
        public string AlbumFolderFormat { get; set; } = "{ArtistName}/{Flag} {AlbumTitle} [{AlbumID}] [{AlbumYear}]";

        [JsonProperty("TrackFileFormat")]
        public string TrackFileFormat { get; set; } = "{TrackNumber} - {ArtistName} - {TrackTitle}{ExplicitFlag}";

        [JsonProperty("VideoFileFormat")]
        public string VideoFileFormat { get; set; } = "{ArtistName}/{TrackNumber} - {VideoTitle}{ExplicitFlag}";


        public static void Change(Settings newItem, Settings oldItem = null)
        {
            if(oldItem == null || oldItem.ThemeType != newItem.ThemeType)
                Theme.Change(newItem.ThemeType);
            if (oldItem == null || oldItem.LanguageType != newItem.LanguageType)
                Language.Change(newItem.LanguageType);
            if (oldItem == null || oldItem.ThreadNum != newItem.ThreadNum)
                ThreadTool.SetThreadNum(newItem.ThreadNum);
        }

        public bool Save()
        {
            string data = JsonHelper.ConverObjectToString<Settings>(this,true);
            return FileHelper.Write(data, true, Paths.GetSettingsPath());
        }

        public static Settings Read()
        {
            string data = FileHelper.Read(Paths.GetSettingsPath());
            Settings ret = JsonHelper.ConverStringToObject<Settings>(data);
            if (ret == null)
                return new Settings();
            return ret;
        }
    }
}

