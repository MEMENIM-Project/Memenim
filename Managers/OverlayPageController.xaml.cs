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

namespace Memenim.Managers
{
    /// <summary>
    /// Interaction logic for OverlayPageController.xaml
    /// </summary>
    public partial class OverlayPageController : UserControl
    {
        public static OverlayPageController Instance { get; set; }

        public OverlayPageController()
        {
            InitializeComponent();
            Instance = this;
            HideOverlay();
        }

        private void OverlayBackgroundClick(object sender, RoutedEventArgs e)
        {
            HideOverlay();
        }

        public void RequestOverlay<T>() where T : Pages.Page
        {
            Pages.Page pg = PageNavigationManager.GetPage<T>();
            OverlayContent.Content = pg;
            rootLayout.Visibility = Visibility.Visible;
        }

        private void HideOverlay()
        {
            rootLayout.Visibility = Visibility.Collapsed;
        }
    }
}
