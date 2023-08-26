using Masiro.lib;
using System.Windows;
using System.Windows.Controls;

namespace Masiro.view
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
            BookCoverEdit.Text = filePath;
        }
    }
}