using Masiro.lib;
using Masiro.reference;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Masiro
{
    public partial class MainWindow : Window
    {
        //当true时，禁止其他按钮操作
        private bool _isExporting = false;
        private int  _i;

        //章节名+原文件路径，用于章节表格
        public ObservableCollection<HtmlPath> HtmlPaths { get; set; }

        //原图片路径，用于彩页表格
        public ObservableCollection<ImagePath> ImagePaths { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            HtmlPaths  = new ObservableCollection<HtmlPath>();
            ImagePaths = new ObservableCollection<ImagePath>();

            ImageGrid.DataContext   = this;
            SectionGrid.DataContext = this;
        }

        //选择封面
        private void ChooseCoverButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isExporting)
            {
                return;
            }

            var selectedFilePath = FileUnitTool.SelectFile();
            CoverPathEdit.Text = selectedFilePath;
        }

        //添加彩页图片
        private void AddImageButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isExporting)
            {
                return;
            }

            var fileList = FileUnitTool.SelectFiles(FileType.Image);
            foreach (var filePath in fileList)
            {
                ImagePaths.Add(new ImagePath { Path = filePath });
            }
        }

        //删除选中的彩页图片
        private void DeletedSelectedImagesButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isExporting)
            {
                return;
            }

            var seletedItemList = ImageGrid.SelectedItems.Cast<ImagePath>().ToList();
            if (seletedItemList.Count > 0)
            {
                foreach (var item in seletedItemList)
                {
                    ImagePaths.Remove(item);
                }
            }
        }

        //添加章节
        private void AddSectionsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isExporting)
            {
                return;
            }

            var fileList   = FileUnitTool.SelectFiles(FileType.Html);
            var onHoldList = new List<HtmlPath>();
            foreach (var filePath in fileList)
            {
                var originText = FileUnitTool.ReadFile(filePath);
                var title      = StringUnitTool.GetTitle(originText);

                if (title == "")
                {
                    var fileName = System.IO.Path.GetFileName(filePath);
                    MessageBox.Show($"错误！请检查 {fileName} 文件");
                    return;
                }

                onHoldList.Add(new HtmlPath { Title = title, Path = filePath });
            }

            foreach (var html in onHoldList)
            {
                HtmlPaths.Add(html);
            }
        }

        //删除选中的章节
        private void DeletedSelectedSectionsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isExporting)
            {
                return;
            }

            var seletedItemList = SectionGrid.SelectedItems.Cast<HtmlPath>().ToList();
            if (seletedItemList.Count > 0)
            {
                foreach (var item in seletedItemList)
                {
                    HtmlPaths.Remove(item);
                }
            }
        }

        private void ExportButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_isExporting)
            {
                return;
            }

            if (TitleEdit.Text == "")
            {
                MessageBox.Show("请输入标题");
                return;
            }

            if (CoverPathEdit.Text == "" || !FileUnitTool.JudgeFileExist(CoverPathEdit.Text))
            {
                MessageBox.Show("封面路径错误");
                return;
            }

            if (HtmlPaths.Count < 1)
            {
                MessageBox.Show("请添加章节");
                return;
            }

            var epubPath = $"result/{TitleEdit.Text}.epub";
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

        private double _exportValue;
        private string _finalName;

        //后台更新进度条的函数，调用一次即可
        private void UpdateProcessBar()
        {
            var thread = new Thread(new ThreadStart(() =>
                                                    {
                                                        //只要在导出，就一直关注更新进度条
                                                        while (_isExporting)
                                                        {
                                                            UpdateValue();
                                                        }
                                                    }))
                         {
                             IsBackground = true
                         };
            thread.Start();
        }

        private void UpdateValue()
        {
            //获取当前的进度条的的值
            double nowValue = 0;
            //使用BeginInvoke获得进度条的使用权
            ExportProgressBar.Dispatcher.BeginInvoke((ThreadStart)delegate { nowValue = ExportProgressBar.Value; });

            while (nowValue < _exportValue)
            {
                ExportProgressBar.Dispatcher.BeginInvoke((ThreadStart)delegate
                                                                      {
                                                                          //每次增加1%或者小于1%
                                                                          ExportProgressBar.Value =
                                                                              Math.Min(1, _exportValue - nowValue) +
                                                                              nowValue;
                                                                          nowValue = ExportProgressBar.Value;
                                                                      });
                //0.1秒刷新一次
                Thread.Sleep(100);
            }
        }


        private async Task ExportWorks()
        {
            //打开锁，进度条初始化
            _isExporting = true;
            await ExportProgressBar.Dispatcher.BeginInvoke((ThreadStart)delegate { ExportProgressBar.Value = 0; });
            _exportValue = 0;
            UpdateProcessBar();

            //计算进度条的值用
            var imageLength   = ImagePaths.Count;
            var sectionLength = HtmlPaths.Count;

            var totalLength = imageLength + sectionLength + 5;

            //创建根目录，和固定的文件
            var rootPath = EpubExportUnitTool.MakeDir("result");
            _exportValue += 85.0 / totalLength;

            //创建彩页
            if (imageLength > 0)
            {
                for (_i = 0; _i < imageLength; _i++)
                {
                    EpubExportUnitTool.MakeImagePage(rootPath, ImagePaths[_i].Path, _i + 1, imageLength);
                    _exportValue += 85.0 / totalLength;
                }
            }

            //创建封面
            EpubExportUnitTool.MakeCoverPage(rootPath, CoverPathEdit.Text);
            _exportValue += 85.0 / totalLength;

            //创建章节页面，和文件名+标题的列表
            var titleList = new List<FileItem>();
            for (_i = 0; _i < sectionLength; _i++)
            {
                var fileName = await EpubExportUnitTool.MakeSection(rootPath,
                                                                    HtmlPaths[_i].Path,
                                                                    _i + 1,
                                                                    sectionLength);
                _exportValue += 85.0 / totalLength;
                titleList.Add(new FileItem(fileName, HtmlPaths[_i].Title));
            }

            //目录页面
            EpubExportUnitTool.MakeContentsXhtml(rootPath, titleList);
            _exportValue += 85.0 / totalLength;
            EpubExportUnitTool.MakeToc(rootPath, titleList, TitleEdit.Text);
            _exportValue += 85.0 / totalLength;
            EpubExportUnitTool.MakeContentOpf(rootPath, TitleEdit.Text);
            _exportValue += 85.0 / totalLength;

            var zipPath  = $"result/{TitleEdit.Text}.zip";
            var epubName = $"{TitleEdit.Text}.epub";
            var epubPath = $"result/{epubName}";
            ZipFile.CreateFromDirectory(rootPath, zipPath);
            _exportValue += 5;
            FileUnitTool.DeleteDirectory(rootPath);
            _exportValue += 5;
            File.Move(zipPath, epubPath);
            _exportValue = 100;
            _finalName   = "/select,result\\" + epubName;
            EndProcess();
        }

        private void EndProcess()
        {
            var thread = new Thread(new ThreadStart(() =>
                                                    {
                                                        Thread.Sleep(1000);
                                                        System.Diagnostics.Process.Start("Explorer.exe", _finalName);
                                                        _isExporting = false;
                                                    }))
                         {
                             IsBackground = true
                         };
            thread.Start();
        }
    }
}