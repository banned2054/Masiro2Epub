using Masiro.lib;
using Masiro.model;
using System.Windows;
using System.Windows.Controls;

namespace Masiro.view
{
    public partial class SettingPageUserControl : UserControl
    {
        public SettingPageUserControl()
        {
            InitializeComponent();

            var jsonText    = FileUnitTool.ReadFile("data/setting.json");
            var settingJson = JsonUtility.FromJson<SettingJson>(jsonText) ?? new SettingJson();

            var flag1 = settingJson.UseProxy;
            ProxySettingUc.ProxyCheckBox.IsChecked = flag1;
            if (!flag1) return;
            ProxySettingUc.ProxyUrlEdit.Text  = settingJson.ProxyUrl;
            ProxySettingUc.ProxyPortEdit.Text = settingJson.ProxyPort.ToString();

            UnsaveUrlCheckBox.IsChecked = settingJson.UseUnsaveUrl;
        }

        private void SaveSettingButton_OnClick(object sender, RoutedEventArgs e)
        {
            var jsonText    = FileUnitTool.ReadFile("data/setting.json");
            var settingJson = JsonUtility.FromJson<SettingJson>(jsonText) ?? new SettingJson();

            if (ProxySettingUc.ProxyCheckBox.IsChecked == true)
            {
                if (ProxySettingUc.ProxyPortEdit.Text == "" || ProxySettingUc.ProxyUrlEdit.Text == "")
                {
                    MessageBox.Show("请输入代理地址和端口");
                    return;
                }

                settingJson.UseProxy  = true;
                settingJson.ProxyUrl  = ProxySettingUc.ProxyUrlEdit.Text;
                settingJson.ProxyPort = int.Parse(ProxySettingUc.ProxyPortEdit.Text);
            }
            else
            {
                settingJson.UseProxy = false;
            }

            settingJson.UseUnsaveUrl = UnsaveUrlCheckBox.IsChecked == true;
            MessageBox.Show("保存成功！");
        }
    }
}