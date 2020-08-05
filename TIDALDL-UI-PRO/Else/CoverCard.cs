using AIGS.Common;
using AIGS.Helper;
using Stylet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static AIGS.Helper.HttpHelper;

namespace TIDALDL_UI.Else
{
    public class CoverCard 
    {
        public string ImgUrl { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Url { get; set; }

        public static async Task<ObservableCollection<CoverCard>> GetList()
        {
            try
            {
                Result result = await HttpHelper.GetOrPostAsync("https://cdn.jsdelivr.net/gh/yaronzz/CDN/app/tidal/todaycards.json");
                if(result.sData.IsNotBlank())
                {
                    ObservableCollection<CoverCard> pList = JsonHelper.ConverStringToObject<ObservableCollection<CoverCard>>(result.sData);
                    return pList;
                }
            }
            catch { }
            return GetDefaultList();
        }

        private static ObservableCollection<CoverCard> GetDefaultList()
        {
            CoverCard card1 = new CoverCard()
            {
                ImgUrl = "https://resources.tidal.com/images/19e4f65c/97f5/43b3/b0ff/7855f0646f95/320x320.jpg",
                Title = "1989 (Deluxe Edition)",
                SubTitle = "Taylor Swift",
                Url = "https://listen.tidal.com/album/121444594",
            };
            CoverCard card2 = new CoverCard()
            {
                ImgUrl = "https://resources.tidal.com/images/5ea6c02a/938d/4641/8fd9/8de90df7d087/320x320.jpg",
                Title = "Happy End",
                SubTitle = "Back Number",
                Url = "https://listen.tidal.com/album/103578093",
            };
            CoverCard card3 = new CoverCard()
            {
                ImgUrl = "https://resources.tidal.com/images/bf3796e4/52f2/4d4b/896c/e39ce41a17df/320x320.jpg",
                Title = "Doo-Wops & Hooligans",
                SubTitle = "Bruno Mars",
                Url = "https://listen.tidal.com/album/4935184",
            };
            ObservableCollection<CoverCard> pCards = new ObservableCollection<CoverCard>();
            pCards.Add(card1);
            pCards.Add(card2);
            pCards.Add(card3);

            string sjson = JsonHelper.ConverObjectToString<ObservableCollection<CoverCard>>(pCards);
            FileHelper.Write(sjson, true, "./covercards.json");

            return pCards;
        }
    }
}
