using Masiro.Models.Export;
using Masiro.Models.User;
using Masiro.Services;
using Masiro.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Masiro.Views;

public partial class ExportPageUserControl : UserControl
{
    private bool   _isExporting;
    private double _maxProgress;
    private int    _i;
    private string _finalName = "";

    public double NowProgress
    {
        get
        {
            double value = 0;
            Dispatcher.Invoke(() => { value = (double)GetValue(NowValueProperty); });
            return value;
        }
        set { Dispatcher.Invoke(() => { SetValue(NowValueProperty, value); }); }
    }


    public static readonly DependencyProperty NowValueProperty = DependencyProperty.Register(
     "NowValue", typeof(double), typeof(ExportPageUserControl), new PropertyMetadata(0.0));

    public ExportPageUserControl()
    {
        InitializeComponent();
        var binding = new Binding("NowValue")
        {
            Source = this
        };
        ExportProgressBar.SetBinding(RangeBase.ValueProperty, binding);
    }

    private void SelectCoverButtonClick(object sender, RoutedEventArgs e)
    {
        var filePath = FileUtils.SelectFile();
        BookCoverEdit.Text = filePath;
    }

    private async void ExportButtonClick(object sender, RoutedEventArgs e)
    {
        if (_isExporting) return;

        if (BookTitleEdit.Text == "")
        {
            MessageBox.Show("请输入标题");
            return;
        }

        var coverPath = BookCoverEdit.Text;
        if (coverPath == "")
        {
            MessageBox.Show("请选择封面");
            return;
        }

        if (coverPath.StartsWith("http"))
        {
            var flag = await MasiroService.IsImageUrlAsync(coverPath);

            if (!flag)
            {
                MessageBox.Show("封面链接错误");
                return;
            }
        }

        else if (!FileUtils.JudgeFileExist(coverPath))
        {
            MessageBox.Show("封面图片不存在");
            return;
        }

        var invalidEpisodeFound = false;

        foreach (var episode in SectionGridUc.EpisodeList)
        {
            if (episode.Title != "" && episode.SubUrl != "" &&
                (episode.SubUrl.StartsWith("https://masiro.me/admin/novelReading?cid=") ||
                 episode.SubUrl.StartsWith("/admin/novelReading?cid="))) continue;
            invalidEpisodeFound = true;
            break;
        }

        if (invalidEpisodeFound)
        {
            MessageBox.Show("请检查章节名和链接");
            return;
        }


        var jsonText = FileUtils.ReadFile("data/user.json");
        var user     = JsonUtils.FromJson<UserInfo>(jsonText);
        if (user == null)
        {
            MessageBox.Show("请登录账号");
            return;
        }

        var epubPath = $"result/{BookTitleEdit.Text}.epub";
        if (FileUtils.JudgeFileExist(epubPath))
        {
            if (MessageBox.Show("文件已存在，是否删除覆盖？", "Confirm Message", MessageBoxButton.OKCancel) !=
                MessageBoxResult.OK) return;
            FileUtils.DeleteFile(epubPath);
            ExportWorks();
        }
        else
        {
            ExportWorks();
        }
    }

    //后台更新进度条的函数，调用一次即可
    private void UpdateProcessBar()
    {
        var thread = new Thread(() =>
        {
            //只要在导出，就一直关注更新进度条
            while (_isExporting)
            {
                UpdateValue();
                if (Math.Abs(NowProgress - 100) > 0.000001) continue;
                _isExporting = false;
                System.Diagnostics.Process.Start("Explorer.exe",
                                                 _finalName);
            }
        })
        {
            IsBackground = true
        };
        thread.Start();
    }

    private void UpdateValue()
    {
        while (NowProgress < _maxProgress)
        {
            ExportProgressBar.Dispatcher.BeginInvoke((ThreadStart)delegate
            {
                NowProgress =
                    Math.Min(1, _maxProgress - NowProgress) +
                    NowProgress;
            });
            //0.1秒刷新一次
            Thread.Sleep(100);
        }
    }

    private async void ExportWorks()
    {
        _isExporting = true;
        NowProgress  = 0;
        _maxProgress = 0;
        UpdateProcessBar();

        //计算进度条的值用
        var imageLength   = ImageGridUc.ImagePathList.Count;
        var sectionLength = SectionGridUc.EpisodeList.Count;

        var totalLength = imageLength + sectionLength + 5;
        //创建根目录，和固定的文件
        var rootPath = ExportUtils.MakeDir("result");
        _maxProgress += 85.0 / totalLength;

        //创建彩页
        if (imageLength > 0)
        {
            for (_i = 0; _i < imageLength; _i++)
            {
                ExportUtils.MakeImagePage(rootPath, ImageGridUc.ImagePathList[_i].Path, _i + 1, imageLength);
                _maxProgress += 85.0 / totalLength;
            }
        }

        //创建封面
        ExportUtils.MakeCoverPage(rootPath, BookCoverEdit.Text);
        _maxProgress += 85.0 / totalLength;

        //创建章节页面，和文件名+标题的列表
        var titleList = new List<FileItem>();
        for (_i = 0; _i < sectionLength; _i++)
        {
            var jsonText = FileUtils.ReadFile("data/user.json");
            var user     = JsonUtils.FromJson<UserInfo>(jsonText);
            if (user == null)
            {
                _isExporting = false;
                return;
            }

            var subUrl = SectionGridUc.EpisodeList[_i].SubUrl;
            if (subUrl.StartsWith("https://masiro.me"))
            {
                subUrl = subUrl[17..];
            }

            var service = new MasiroService(user.Cookie);
            var result = await service.GetNovelHtmlWithAutoLoginAsync(
                subUrl,
                user.UserName,
                user.Password,
                Window.GetWindow(this));

            if (!result.Success)
            {
                MessageBox.Show($"获取内容失败:{result.ErrorMessage}");
                _isExporting = false;
                return;
            }

            user.Cookie = result.Cookies;

            var fileName = await ExportUtils.MakeSection(rootPath,
                                                         result.Html,
                                                         _i + 1,
                                                         sectionLength);
            _maxProgress += 85.0 / totalLength;
            titleList.Add(new FileItem(fileName, SectionGridUc.EpisodeList[_i].Title));
        }

        //目录页面
        ExportUtils.MakeContentsXhtml(rootPath, titleList);
        _maxProgress += 85.0 / totalLength;
        ExportUtils.MakeToc(rootPath, titleList, BookTitleEdit.Text);
        _maxProgress += 85.0 / totalLength;
        ExportUtils.MakeContentOpf(rootPath, BookTitleEdit.Text);
        _maxProgress += 85.0 / totalLength;

        var zipPath  = $"result/{BookTitleEdit.Text}.zip";
        var epubName = $"{BookTitleEdit.Text}.epub";
        var epubPath = $"result/{epubName}";
        await ZipFile.CreateFromDirectoryAsync(rootPath, zipPath);
        _maxProgress += 5;
        FileUtils.DeleteDirectory(rootPath);
        _maxProgress += 5;
        File.Move(zipPath, epubPath);
        _maxProgress = 100;
        _finalName   = "/select,result\\" + epubName;
    }
}
