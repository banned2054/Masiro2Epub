using Masiro.reference;
using System;
using System.Windows;

namespace Masiro
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            HomePageUc.SelectUc.ExportButtonClicked += HandleCustomEvent;
        }

        private void HandleCustomEvent(object? sender, EventArgs e)
        {
            if (e is not ArgumentConvey.ChapterHandleArgs arg) return;
            var chapter = arg.NowChapter;
            MainWindowTab.SelectedIndex                 = 1; // 切换到第二个选项卡
            ExportPageUc.BookTitleUc.BookTitleEdit.Text = chapter.BookTitle;
            ExportPageUc.SectionGridUc.EpisodeList.Clear();
            foreach (var episode in chapter.EpisodeList)
            {
                ExportPageUc.SectionGridUc.EpisodeList.Add(new Episode(episode));
            }
        }
    }
}