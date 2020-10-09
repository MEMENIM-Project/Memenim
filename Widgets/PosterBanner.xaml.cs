using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnonymDesktopClient.Widgets
{
    /// <summary>
    /// Interaction logic for PosterBanner.xaml
    /// </summary>
    public partial class PosterBanner : UserControl
    {
        public string PosterImageSource { get; set; }
        public string PosterName { get; set; }
        public string PostTime { get; set; }

        public PosterBanner()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
        }
    }
}
