using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            English,
            简体中文,
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
                findstr = string.Format(@"StringResource.{0}.xaml", CultureInfo.CurrentCulture.Name);
            else if(type == Type.简体中文)
                findstr = string.Format(@"StringResource.{0}.xaml", "zh-CN");
            else
                findstr = @"StringResource.xaml";

            ret = pList.FirstOrDefault(x => x.Source != null && x.Source.OriginalString.Contains(findstr));
            if(ret == null)
                ret = pList.FirstOrDefault(x => x.Source != null && x.Source.OriginalString.Contains("StringResource.xaml"));
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
