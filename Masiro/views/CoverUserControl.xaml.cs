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
using Masiro.lib;

namespace Masiro.views
{
    /// <summary>
    /// CoverUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class CoverUserControl : UserControl
    {
        public CoverUserControl()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var filePath = FileUnitTool.SelectFile();
            this.BookCoverEdit.Text = filePath;
        }
    }
}