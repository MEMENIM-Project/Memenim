using AnonymDesktopClient.Core;
using Memenim.Core;
using Memenim.Core.Data;
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

namespace AnonymDesktopClient.Pages
{
    /// <summary>
    /// Interaction logic for PostOverlayPage.xaml
    /// </summary>
    public partial class PostOverlayPage : UserControl
    {
        public PostData PostInfo { get; set; }

        public PostOverlayPage()
        {
            InitializeComponent();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PageNavigationManager.CloseOverlay();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            //var res = await PostAPI.GetPostById(PostID, AppPersistent.UserToken);
            //if(res.error)
            //{
            //    DialogManager.ShowDialog("F U C K", "Failed to load post");
            //}
            wdgPost.CurrentPostData = PostInfo;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PageNavigationManager.CloseOverlay();
        }
    }
}
