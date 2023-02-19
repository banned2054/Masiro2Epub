using Masiro.lib;
using Masiro.reference;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Masiro.views
{
    public partial class SelectPageUserControl : UserControl
    {
        public event EventHandler            LogoutButtonClicked = null!;
        public event EventHandler            ExportButtonClicked = null!;
        public ObservableCollection<Chapter> ChapterList { get; set; }

        public SelectPageUserControl()
        {
            InitializeComponent();
            ChapterList             = new ObservableCollection<Chapter>();
            ChapterGrid.DataContext = this;
        }

        private void LogoutButton_OnClick(object sender, RoutedEventArgs e)
        {
            LogoutButtonClicked?.Invoke(this, EventArgs.Empty);
        }

        private async void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            var url = BookUrlEdit.Text;
            if (url.StartsWith("https://masiro.me/admin/novelView?novel_id="))
            {
                var originText = FileUnitTool.ReadFile("data/user.json");
                var user       = JsonUtility.FromJson<UserInfoJson>(originText);
                if (user == null) return;
                var subUrl     = url.Split("https://masiro.me")[1];
                var originHtml = await NetworkUnitTool.MasiroHtml(user.Cookie, subUrl);
                if (originHtml.MyToken.StartsWith("fail"))
                {
                    MessageBox.Show($"error:{originHtml.MyToken[5..]}");
                    return;
                }

                var chapterList = StringUnitTool.GetChapterList(originHtml.MyToken);
                ChapterList.Clear();
                foreach (var chapter in chapterList)
                {
                    ChapterList.Add(new Chapter(chapter));
                }

                MessageBox.Show($"找到{ChapterList.Count}个章节");
            }
            else
            {
                MessageBox.Show("请注意链接是否正确");
            }
        }

        private void ExportButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (ChapterGrid.SelectedItem is not Chapter selectedChapter)
            {
                MessageBox.Show("请选择要导出的卷");
            }

            else
            {
                ExportButtonClicked?.Invoke(this, new ArgumentConvey.ChapterHandleArgs(selectedChapter));
            }
        }
    }
}