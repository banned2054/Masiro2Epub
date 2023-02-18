using Masiro.lib;
using Masiro.reference;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Masiro.views
{
    public partial class LoginUserControl : UserControl
    {
        public event EventHandler LoginButtonClicked = null!;

        public LoginUserControl()
        {
            InitializeComponent();

            if (!FileUnitTool.JudgeFileExist("data")) FileUnitTool.MakeDir("data");
            if (!FileUnitTool.JudgeFileExist("data/user.json")) FileUnitTool.MakeFile("data/user.json");

            var jsonString = FileUnitTool.ReadFile("data/user.json");
            var user       = JsonUtility.FromJson<UserInfoJson>(jsonString) ?? new UserInfoJson();
            UserNameEdit.Text     = user.UserName;
            PasswordEdit.Password = user.Password;
        }

        private async void LoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (UserNameEdit.Text == "" || PasswordEdit.Password == "")
            {
                MessageBox.Show("请输入账号密码");
                return;
            }


            var jsonString = FileUnitTool.ReadFile("data/user.json");
            var user       = JsonUtility.FromJson<UserInfoJson>(jsonString) ?? new UserInfoJson();
            user.UserName = UserNameEdit.Text;
            user.Password = PasswordEdit.Password;

            var token = await NetworkUnitTool.GetToken();
            user.Cookie = token.MyCookie;
            var loginToken = await NetworkUnitTool.LoginMasiro(token, user.UserName, user.Password);
            if (loginToken.MyToken != "success")
            {
                var errorMessage = loginToken.MyToken[5..];
                MessageBox.Show($"登录失败:{errorMessage}");
                return;
            }

            user.Cookie = loginToken.MyCookie;
            jsonString  = JsonUtility.ToJson(user);
            FileUnitTool.WriteFile("data/user.json", jsonString);
            MessageBox.Show("登录成功！");
            // 触发LoginButtonClicked事件
            LoginButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}