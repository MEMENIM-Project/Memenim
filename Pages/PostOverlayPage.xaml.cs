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
    public partial class PostOverlayPage : Page
    {
        public PostData PostInfo { get; set; }

        public PostOverlayPage()
        {
            InitializeComponent();
        }

        protected override void OnEnter(object sender, RoutedEventArgs e)
        {
            //wdgPost.CurrentPostData = PostInfo;
            //wdgCommentsList.PostID = PostInfo.id;
            //wdgCommentsList.CommentsCount = PostInfo.comments.count;
            //wdgUserComment.PostID = PostInfo.id;
        }
    }
}
