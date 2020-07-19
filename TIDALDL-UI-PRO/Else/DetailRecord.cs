using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace TIDALDL_UI.Else
{
    public class DetailRecord : Screen
    {
        public class Item : Screen
        {
            public string Title { get; set; }
            public string Type { get; set; }
            public string Flag { get; set; }
            public string Duration { get; set; }
            public string AlbumTitle { get; set; }
            //public bool Check { get; set; }
            public object Data { get; set; }

            public Item(string title, string duration, string albumtitle, object data = null, string type = "TRACK", string flag = "")
            {
                //Check = true;
                Flag = flag; //M-Master E-Explicit
                Title = title;
                Type = type;
                Duration = duration;
                AlbumTitle = albumtitle;
                Data = data;
            }
        }

        public object Data { get; set; }
        public string Header { get; set; }
        public string Title { get;  set; }
        public string Intro { get;  set; }
        public string ReleaseDate { get;  set; }
        public BitmapImage Cover { get;  set; }
        public string FlagDetail { get; set; } //Master Explicit
        public string Modes { get; set; }
        public ObservableCollection<Item> ItemList { get;  set; }
    }


}
