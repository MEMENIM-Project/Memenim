using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Core.Data;

namespace AnonymDesktopClient.Core.Pages
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
            wdgPost.CurrentPostData = PostInfo;
            wdgCommentsList.PostID = PostInfo.id;
            wdgCommentsList.CommentsCount = PostInfo.comments.count;
            wdgUserComment.PostID = PostInfo.id;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PageNavigationManager.CloseOverlay();
        }
    }
}
