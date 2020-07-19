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
    /// SearchView.xaml 的交互逻辑
    /// </summary>
    public partial class SearchView : UserControl
    {
        public SearchView()
        {
            InitializeComponent();

            IEnumerable<object> contentList = new[]
            {
                new Uri(@"pack://application:,,,/resource/cover/2.jpg"),
                new Uri(@"pack://application:,,,/resource/cover/3.jpg"),
                new Uri(@"pack://application:,,,/resource/cover/1.jpg"),
                new Uri(@"pack://application:,,,/resource/cover/4.jpg"),
                new Uri(@"pack://application:,,,/resource/cover/5.jpg"),
                new Uri(@"pack://application:,,,/resource/cover/6.jpg"),
            };
            ctrlCoverFlow1.AddRange(contentList);
            ctrlCoverFlow2.AddRange(contentList);
            ctrlCoverFlow1.JumpTo(2);
            ctrlCoverFlow2.JumpTo(2);
        }
    }
}
