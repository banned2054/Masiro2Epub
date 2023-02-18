using System;
using System.Windows;
using System.Windows.Controls;

namespace Masiro.views
{
    public partial class SelectPageUserControl : UserControl
    {
        public event EventHandler LogoutButtonClicked = null!;

        public SelectPageUserControl()
        {
            InitializeComponent();
        }

        private void LogoutButton_OnClick(object sender, RoutedEventArgs e)
        {
            LogoutButtonClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}