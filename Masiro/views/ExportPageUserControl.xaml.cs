using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Masiro.lib;
using Masiro.reference;

namespace Masiro.views
{
    public partial class ExportPageUserControl : UserControl
    {
        private bool   _isExporting = false;
        private double _maxProgress = 0;
        private int    _i           = 0;
        private string _finalName;

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

        private async void ExportButtonClick(object sender, RoutedEventArgs e)
        {
            if (_isExporting) return;

            bool coverIsLocal = false;
            if (BookTitleUc.BookTitleEdit.Text == "")
            {
                MessageBox.Show("请输入标题");
                return;
            }

            var coverPath = CoverUc.BookCoverEdit.Text;
            if (coverPath == "")
            {
                MessageBox.Show("请选择封面");
                return;
            }

            if (coverPath.StartsWith("http"))
            {
                var flag = await NetworkUnitTool.IsImageUrl(coverPath);

                if (!flag)
                {
                    MessageBox.Show("封面链接错误");
                    return;
                }

                coverIsLocal = false;
            }

            else if (!FileUnitTool.JudgeFileExist(coverPath))
            {
                MessageBox.Show("封面图片不存在");
                return;
            }
            else
            {
                coverIsLocal = true;
            }

            if (SectionGridUc.EpisodeList.Any(episode => episode.Title == "" || episode.SubUrl == "" ||
                                                         (!episode.SubUrl
                                                                  .StartsWith("https://masiro.me/admin/novelReading?cid=") &&
                                                          !episode.SubUrl
                                                                  .StartsWith("/admin/novelReading?cid="))
                                             ))

            {
                MessageBox.Show("请检查章节名和链接");
                return;
            }

            var jsonText = FileUnitTool.ReadFile("data/user.json");
            var user     = JsonUtility.FromJson<UserInfoJson>(jsonText);
            if (user == null)
            {
                MessageBox.Show("请登录账号");
                return;
            }

            var epubPath = $"result/{BookTitleUc.BookTitleEdit.Text}.epub";
            if (FileUnitTool.JudgeFileExist(epubPath))
            {
                if (MessageBox.Show("文件已存在，是否删除覆盖？", "Confirm Message", MessageBoxButton.OKCancel) ==
                    MessageBoxResult.OK)
                {
                    FileUnitTool.DeleteFile(epubPath);
                    ExportWorks();
                }
            }
            else
            {
                ExportWorks();
            }
        }

        //后台更新进度条的函数，调用一次即可
        private void UpdateProcessBar()
        {
            var thread = new Thread(new ThreadStart(() =>
                                                    {
                                                        //只要在导出，就一直关注更新进度条
                                                        while (_isExporting)
                                                        {
                                                            UpdateValue();
                                                            if (NowProgress == 100)
                                                            {
                                                                _isExporting = false;
                                                                System.Diagnostics.Process.Start("Explorer.exe",
                                                                    _finalName);
                                                            }
                                                        }
                                                    }))
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

        async void ExportWorks()
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
            var rootPath = EpubExportUnitTool.MakeDir("result");
            _maxProgress += 85.0 / totalLength;

            //创建彩页
            if (imageLength > 0)
            {
                for (_i = 0; _i < imageLength; _i++)
                {
                    EpubExportUnitTool.MakeImagePage(rootPath, ImageGridUc.ImagePathList[_i].Path, _i + 1, imageLength);
                    _maxProgress += 85.0 / totalLength;
                }
            }

            //创建封面
            EpubExportUnitTool.MakeCoverPage(rootPath, CoverUc.BookCoverEdit.Text);
            _maxProgress += 85.0 / totalLength;

            //创建章节页面，和文件名+标题的列表
            var titleList = new List<FileItem>();
            for (_i = 0; _i < sectionLength; _i++)
            {
                var jsonText = FileUnitTool.ReadFile("data/user.json");
                var user     = JsonUtility.FromJson<UserInfoJson>(jsonText);
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

                var originHtml = await NetworkUnitTool.MasiroHtml(user.Cookie, subUrl);
                if (originHtml.MyToken.StartsWith("fail"))
                {
                    var token1 = await NetworkUnitTool.GetToken();
                    var token2 = await NetworkUnitTool.LoginMasiro(token1, user.UserName, user.Password);
                    if (token2.MyToken != "success")
                    {
                        var errorMessage = token2.MyToken[5..];
                        MessageBox.Show($"登录失败:{errorMessage}");
                        _isExporting = false;
                        return;
                    }

                    user.Cookie = token2.MyCookie;
                    var final = await NetworkUnitTool.MasiroHtml(token2.MyCookie, subUrl);
                    if (final.MyToken.StartsWith("fail"))
                    {
                        _isExporting = false;
                        return;
                    }

                    originHtml = final;
                }

                user.Cookie = originHtml.MyCookie;

                var fileName = await EpubExportUnitTool.MakeSection(rootPath,
                                                                    originHtml.MyToken,
                                                                    _i + 1,
                                                                    sectionLength);
                _maxProgress += 85.0 / totalLength;
                titleList.Add(new FileItem(fileName, SectionGridUc.EpisodeList[_i].Title));
            }

            //目录页面
            EpubExportUnitTool.MakeContentsXhtml(rootPath, titleList);
            _maxProgress += 85.0 / totalLength;
            EpubExportUnitTool.MakeToc(rootPath, titleList, BookTitleUc.BookTitleEdit.Text);
            _maxProgress += 85.0 / totalLength;
            EpubExportUnitTool.MakeContentOpf(rootPath, BookTitleUc.BookTitleEdit.Text);
            _maxProgress += 85.0 / totalLength;

            var zipPath  = $"result/{BookTitleUc.BookTitleEdit.Text}.zip";
            var epubName = $"{BookTitleUc.BookTitleEdit.Text}.epub";
            var epubPath = $"result/{epubName}";
            ZipFile.CreateFromDirectory(rootPath, zipPath);
            _maxProgress += 5;
            FileUnitTool.DeleteDirectory(rootPath);
            _maxProgress += 5;
            File.Move(zipPath, epubPath);
            _maxProgress = 100;
            _finalName   = "/select,result\\" + epubName;
        }
    }
}