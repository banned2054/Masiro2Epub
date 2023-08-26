using Masiro.lib;
using Masiro.model;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Masiro.view
{
    /// <summary>
    /// ImageGridUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ImageGridUserControl : UserControl
    {
        //原图片路径，用于彩页表格
        public ObservableCollection<ImagePath> ImagePathList { get; set; }

        public ImageGridUserControl()
        {
            InitializeComponent();
            ImagePathList         = new ObservableCollection<ImagePath>();
            ImageGrid.DataContext = this;
        }

        private void AddImageButtonClick(object sender, RoutedEventArgs e)
        {
            var imageList = FileUnitTool.SelectFiles(FileType.Image);
            foreach (var imagePath in imageList)
            {
                ImagePathList.Add(new ImagePath { Path = imagePath });
            }
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var selectedItemList = ImageGrid.SelectedItems.Cast<ImagePath>().ToList();
            foreach (var imagePath in selectedItemList)
            {
                ImagePathList.Remove(imagePath);
            }
        }
    }
}