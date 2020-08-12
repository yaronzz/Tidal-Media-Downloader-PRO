using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TIDALDL_UI.Pages
{
    /// <summary>
    /// MessageView.xaml 的交互逻辑
    /// </summary>
    public partial class MessageView : UserControl
    {
        Action<string> Action { get; set; }
        public bool Result { get; set; } = false;

        public MessageView(MessageBoxImage type = MessageBoxImage.Information, string message = null, bool cancel = true, Action<string> action = null)
        {
            InitializeComponent();

            if(type == MessageBoxImage.Warning)
            {
                CtlIcon.Fill = Brushes.Red;
                CtlIcon.Data = (Geometry)GetResource<Geometry>("WarningGeometry");
            }
            else
            {
                CtlIcon.Fill = Brushes.Green;
                CtlIcon.Data = (Geometry)GetResource<Geometry>("InfoGeometry");
            }

            if (cancel)
            {
                PanelOKCancel.Visibility = Visibility.Visible;
                PanelOK.Visibility = Visibility.Hidden;
            }
            else
            {
                PanelOKCancel.Visibility = Visibility.Hidden;
                PanelOK.Visibility = Visibility.Visible;
            }

            CtlMsg.Text = message;
            Action = action;
        }

        private void Confim(object sender, RoutedEventArgs e)
        {
            if (Action != null)
                Action(null);

            Result = true;
            BtnClose.Command.Execute(null);
        }

        public static T GetResource<T>(string key)
        {
            if (Application.Current.TryFindResource(key) is T resource)
            {
                return resource;
            }

            return default;
        }
    }
}
