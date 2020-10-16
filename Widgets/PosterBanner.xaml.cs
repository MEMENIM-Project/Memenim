using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Pages;

namespace Memenim.Widgets
{
    /// <summary>
    /// Interaction logic for PosterBanner.xaml
    /// </summary>
    public partial class PosterBanner : UserControl
    {
        public int PosterID { get; set; }
        public string PosterImageSource { get; set; }
        public string PosterName { get; set; } = "Unknown";
        public string PostTime { get; set; }
        public bool IsAnonymousPost { get; set; }

        public PosterBanner()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
            if (IsAnonymousPost)
            {
                txtAnon.Visibility = Visibility.Hidden;
            }
            else
            {
                txtAnon.Visibility = Visibility.Visible;
            }
        }

        private void OnPoster_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (PosterID > 0)
            {
                //PageNavigationManager.SwitchToSubpage(new UserProfilePage() { UserID = PosterID });
                //PageNavigationManager.CloseOverlay();

            }
        }
    }
}
