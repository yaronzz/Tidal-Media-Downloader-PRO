using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TIDALDL_UI.Else
{
    public class Language
    {
        public static string Get(string key)
        {
            object value = Application.Current.FindResource(key);
            if (value == null)
                return "NULL";
            return value.ToString();
        }

        public enum Type
        {
            Default,
            Chinese,
            Czech,			
            Dutch,
            English,
            German,
            PortugueseBR,
            PortuguesePT,			
            Russian,
            Spanish,
            Turkish,
        }

        private static ResourceDictionary GetResourceDictionaryByType(Type type = Type.Default)
        {
            string findstr = null;
            ResourceDictionary ret = null;

            //Load all resource
            List<ResourceDictionary> pList = new List<ResourceDictionary>();
            foreach (ResourceDictionary item in Application.Current.Resources.MergedDictionaries)
                pList.Add(item);

            //find resource file
            if (type == Type.Default)
                findstr = @"StringResource.English.xaml";
            else
            {
                string sub = AIGS.Common.Convert.ConverEnumToString((int)type, typeof(Type), 0);
                findstr = string.Format(@"StringResource.{0}.xaml", sub);
            }

            ret = pList.FirstOrDefault(x => x.Source != null && x.Source.OriginalString.Contains(findstr));
            if(ret == null)
                ret = pList.FirstOrDefault(x => x.Source != null && x.Source.OriginalString.Contains("StringResource.English.xaml"));
            return ret;
        }

        public static bool Change(Type type = Type.Default)
        {
            ResourceDictionary file = GetResourceDictionaryByType(type);
            if (file == null)
                return false;

            Application.Current.Resources.MergedDictionaries.Remove(file);
            Application.Current.Resources.MergedDictionaries.Add(file);
            return true;
        }
    }
}
