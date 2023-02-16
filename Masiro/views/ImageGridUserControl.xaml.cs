using Masiro.reference;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Masiro.views
{
    /// <summary>
    /// ImageGridUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class ImageGridUserControl : UserControl
    {
        //原图片路径，用于彩页表格
        public ObservableCollection<ImagePath> ImagePaths { get; set; }

        public ImageGridUserControl()
        {
            InitializeComponent();
            ImagePaths = new ObservableCollection<ImagePath>();
        }
    }
}