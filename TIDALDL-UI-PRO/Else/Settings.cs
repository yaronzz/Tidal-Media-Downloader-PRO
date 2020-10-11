using AIGS.Helper;
using HandyControl.Properties.Langs;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TidalLib;

namespace TIDALDL_UI.Else
{
    public class UserSettings : Stylet.Screen
    {
        //User information

        [JsonProperty("Userid")]
        public string Userid { get; set; } = null;

        [JsonProperty("Username")]
        public string Username { get; set; } = null;

        [JsonProperty("Password")]
        public string Password { get; set; } = null;

        [JsonProperty("Sessionid1")]
        public string Sessionid1 { get; set; } = null;

        [JsonProperty("Sessionid2")]
        public string Sessionid2 { get; set; } = null;

        [JsonProperty("Countrycode")]
        public string Countrycode { get; set; } = null;

        [JsonProperty("Accesstoken")]
        public string Accesstoken { get; set; } = null;

        [JsonProperty("Remember")]
        public bool Remember { get; set; } = true;

        [JsonProperty("AutoLogin")]
        public bool AutoLogin { get; set; } = true;

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
            return FileHelper.Write(EncryptHelper.Encode(data, Global.KEY_BASE), true, Global.PATH_USERSETTINGS);
        }

        public static UserSettings Read()
        {
            string buf = FileHelper.Read(Global.PATH_USERSETTINGS);
            string data = EncryptHelper.Decode(buf, Global.KEY_BASE);
            UserSettings ret = JsonHelper.ConverStringToObject<UserSettings>(data);
            if (ret == null)
                return new UserSettings();
            return ret;
        }
    }

    public class Settings : Stylet.Screen
    {
        //Common

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

        //Track 

        [JsonProperty("OnlyM4a")]
        public bool OnlyM4a { get; set; } = true;

        [JsonProperty("AddExplicitTag")]
        public bool AddExplicitTag { get; set; } = true;

        [JsonProperty("AddHyphen")]
        public bool AddHyphen { get; set; } = true;

        [JsonProperty("UseTrackNumber")]
        public bool UseTrackNumber { get; set; } = true;

        [JsonProperty("AudioQuality")]
        public eAudioQuality AudioQuality { get; set; } = eAudioQuality.HiFi;

        [JsonProperty("MaxFileName")]
        public int MaxFileName { get; set; } = 50;

        [JsonProperty("MaxDirName")]
        public int MaxDirName { get; set; } = 50;

        [JsonProperty("CheckExist")]
        public bool CheckExist { get; set; } = true;

        [JsonProperty("ArtistBeforeTitle")]
        public bool ArtistBeforeTitle { get; set; } = true;

        //Video

        [JsonProperty("VideoQuality")]
        public eVideoQuality VideoQuality { get; set; } = eVideoQuality.P720;

        //Album

        [JsonProperty("IncludeEP")]
        public bool IncludeEP { get; set; } = true;

        [JsonProperty("AddAlbumIDBeforeFolder")]
        public bool AddAlbumIDBeforeFolder { get; set; } = false;

        [JsonProperty("SaveCovers")]
        public bool SaveCovers { get; set; } = true;

        [JsonProperty("AddYear")]
        public ePositionYear AddYear { get; set; } = ePositionYear.None;

        [JsonProperty("AlbumFolderFormat")]
        public string AlbumFolderFormat { get; set; } = "{Flag} {AlbumTitle} [{AlbumID}] [{AlbumYear}]";

        [JsonProperty("TrackFileFormat")]
        public string TrackFileFormat { get; set; } = "{TrackNumber} - {ArtistName} - {TrackTitle}{ExplicitFlag}";


        public static void Change(Settings newItem, Settings oldItem = null)
        {
            if(oldItem == null || oldItem.ThemeType != newItem.ThemeType)
                Theme.Change(newItem.ThemeType);
            //if (oldItem == null || oldItem.LanguageType != newItem.LanguageType)
            //    Language.Change(newItem.LanguageType);
            if (oldItem == null || oldItem.ThreadNum != newItem.ThreadNum)
                ThreadTool.SetThreadNum(newItem.ThreadNum);
        }

        public bool Save()
        {
            string data = JsonHelper.ConverObjectToString<Settings>(this,true);
            return FileHelper.Write(data, true, Global.PATH_SETTINGS);
        }

        public static Settings Read()
        {
            string data = FileHelper.Read(Global.PATH_SETTINGS);
            Settings ret = JsonHelper.ConverStringToObject<Settings>(data);
            if (ret == null)
                return new Settings();
            return ret;
        }
    }

    public enum ePositionYear
    {
        None,
        Before,
        After,
    }
}

