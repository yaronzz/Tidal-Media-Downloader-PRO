using HandyControl.Data;
using HandyControl.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TIDALDL_UI.Else
{
    public class Theme
    {
        public enum Type
        {
            Default,
            Dark,
            Violet,
        }

        public static void Change(Type type = Type.Default)
        {
            SharedResourceDictionary.SharedDictionaries.Clear();
            var skins0 = Application.Current.Resources.MergedDictionaries[1];
            skins0.MergedDictionaries.Clear();
            skins0.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri($"pack://application:,,,/HandyControl;component/Themes/Skin{type.ToString()}.xaml") });	
            
            var skins1 = Application.Current.Resources.MergedDictionaries[2];	
            skins1.MergedDictionaries.Clear();	
            skins1.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml") });	
            Application.Current.MainWindow?.OnApplyTemplate();
        }

    }
}
