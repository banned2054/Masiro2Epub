using Masiro.reference;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Masiro.views
{
    public partial class SectionGridUserControl : UserControl
    {
        public ObservableCollection<Episode> EpisodeList { get; set; }

        public SectionGridUserControl()
        {
            InitializeComponent();
            SectionGrid.DataContext = this;
            EpisodeList             = new ObservableCollection<Episode>();
        }
    }
}