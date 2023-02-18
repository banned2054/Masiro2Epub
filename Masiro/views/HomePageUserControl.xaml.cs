using Masiro.lib;
using Masiro.reference;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Masiro.views
{
    public partial class HomePageUserControl : UserControl
    {
        public HomePageUserControl()
        {
            InitializeComponent();

            // 订阅LoginUserControl中的LoginButtonClicked事件
            LoginUc.LoginButtonClicked   += LoginAndChangeToSearchPage;
            SelectUc.LogoutButtonClicked += LogoutAndChangeToLoginPage;
        }

        private void LoginAndChangeToSearchPage(object? sender, EventArgs e)
        {
            LoginUc.Visibility  = Visibility.Collapsed;
            SelectUc.Visibility = Visibility.Visible;


            var jsonString = FileUnitTool.ReadFile("data/user.json");
            var user       = JsonUtility.FromJson<UserInfoJson>(jsonString) ?? new UserInfoJson();
            SelectUc.UserNameLabel.Content = user.UserName;
        }

        private void LogoutAndChangeToLoginPage(object? sender, EventArgs e)
        {
            LoginUc.Visibility  = Visibility.Visible;
            SelectUc.Visibility = Visibility.Collapsed;


            var jsonString = FileUnitTool.ReadFile("data/user.json");
            var user       = JsonUtility.FromJson<UserInfoJson>(jsonString) ?? new UserInfoJson();
            SelectUc.UserNameLabel.Content = user.UserName;
        }
    }
}