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

namespace AnonymDesktopClient.Widgets
{
    /// <summary>
    /// Interaction logic for CommentsList.xaml
    /// </summary>
    public partial class CommentsList : UserControl
    {
        public int PostID { get; set; }
        public int CommentsCount { get; set; }

        private int m_Offset { get; set; }

        private const int CommentsOffsetValue = 20;

        public CommentsList()
        {
            InitializeComponent();
        }

        private async void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            lstComments.Children.Clear();
            var res = await PostAPI.GetCommentsForPost(PostID);
            if(res.error)
            {
                DialogManager.ShowDialog("F U C K", "Cannot load comments");
                return;
            }
            AddComments(res.data);
            m_Offset += CommentsOffsetValue;
        }

        public void AddComments(List<CommentData> comments)
        {
            foreach (var comment in comments)
            {
                UserComment commentWidget = new UserComment();
                commentWidget.UserName = comment.user.name;
                commentWidget.Comment = comment.text;
                commentWidget.ImageURL = comment.user.photo;
                commentWidget.UserID = comment.user.id;
                commentWidget.CommentID = comment.id;
                lstComments.Children.Insert(0, commentWidget);
            }
            if (lstComments.Children.Count >= CommentsCount - 1)
            {
                btnLoadMore.Visibility = Visibility.Hidden;
            }
        }

        private async void btnLoadMore_Click(object sender, RoutedEventArgs e)
        {
            var res = await PostAPI.GetCommentsForPost(PostID, m_Offset);
            btnLoadMore.IsEnabled = false;
            if (res.error)
            {
                DialogManager.ShowDialog("F U C K", "Cannot load comments");
                return;
            }
            AddComments(res.data);
            btnLoadMore.IsEnabled = true;
            m_Offset += CommentsOffsetValue;
        }
    }
}
