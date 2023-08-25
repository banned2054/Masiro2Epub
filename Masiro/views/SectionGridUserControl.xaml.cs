using Masiro.reference;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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

        private void AddSectionButton(object sender, RoutedEventArgs e)
        {
            EpisodeList.Add(new Episode());
        }

        private void DeleteSectionButton(object sender, RoutedEventArgs e)
        {
            var selectedItemList = SectionGrid.SelectedItems.Cast<Episode>().ToList();
            foreach (var episode in selectedItemList)
            {
                EpisodeList.Remove(episode);
            }
        }
    }
}