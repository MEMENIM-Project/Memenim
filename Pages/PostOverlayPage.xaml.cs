using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Managers;
using Memenim.Core.Data;

namespace Memenim.Pages
{
    /// <summary>
    /// Interaction logic for PostOverlayPage.xaml
    /// </summary>
    public partial class PostOverlayPage : PageContent
    {
        public PostOverlayPage()
        {
            InitializeComponent();
        }

        protected override void OnEnter(object sender, RoutedEventArgs e)
        {
            wdgPost.CurrentPostData = (DataContext as PostData);
            wdgCommentsList.PostID = (DataContext as PostData).id;
            wdgCommentsList.CommentsCount = (DataContext as PostData).comments.count; 
            wdgUserComment.PostID = (DataContext as PostData).id;
        }
    }
}
