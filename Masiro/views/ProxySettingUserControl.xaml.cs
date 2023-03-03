using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Masiro.views
{
    /// <summary>
    /// ProxySettingUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ProxySettingUserControl : UserControl
    {
        public ProxySettingUserControl()
        {
            InitializeComponent();
        }

        private void ProxyPortEdit_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            // 使用正则表达式检查文本是否符合要求
            var text    = ProxyPortEdit.Text;
            var pattern = @"^[0-9]+$";
            if (Regex.IsMatch(text, pattern)) return;
            ProxyPortEdit.Text           = Regex.Replace(text, "[^0-9]", "");
            ProxyPortEdit.SelectionStart = ProxyPortEdit.Text.Length;
        }
    }
}