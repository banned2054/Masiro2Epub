using Masiro.Models.Chapter;
using Masiro.Models.Common;
using Masiro.Models.User;
using Masiro.Services;
using Masiro.Utils;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Masiro.Views;

public partial class SelectPageUserControl : UserControl
{
    public event EventHandler            LogoutButtonClicked = null!;
    public event EventHandler            ExportButtonClicked = null!;
    public ObservableCollection<Chapter> ChapterList { get; set; }

    public SelectPageUserControl()
    {
        InitializeComponent();
        ChapterList             = [];
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
            var originText = FileUtils.ReadFile("data/user.json");
            var user       = JsonUtils.FromJson<UserInfo>(originText);
            if (user == null) return;
            var subUrl     = url.Split("https://masiro.me")[1];

            var service = new MasiroService(user.Cookie);
            var result = await service.GetNovelHtmlWithBypassAsync(subUrl, Window.GetWindow(this));
            if (!result.Success)
            {
                MessageBox.Show($"error:{result.ErrorMessage}");
                return;
            }

            var chapterList = StringUtils.GetChapterList(result.Html);
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
            ExportButtonClicked?.Invoke(this, new ChapterHandleArgs(selectedChapter));
        }
    }
}
