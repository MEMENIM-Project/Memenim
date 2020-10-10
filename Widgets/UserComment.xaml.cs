using AnonymDesktopClient.Core;
using AnonymDesktopClient.Pages;
using Memenim.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AnonymDesktopClient
{
    /// <summary>
    /// Interaction logic for UserComment.xaml
    /// </summary>
    public partial class UserComment : UserControl
    {
        public UserComment()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public string UserName { get; set; }
        public string Comment { get; set; }
        public string ImageURL { get; set; }
        public string Likes { get; set; } = "0";
        public string Dislikes { get; set; } = "0";
        public int UserID { get; set; }
        public int CommentID { get; set; }

        private async void Like_Click(object sender, RoutedEventArgs e)
        {
            var result = await PostAPI.LikeComment(CommentID, AppPersistent.UserToken);
            if (!result.error)
            {
                DialogManager.ShowDialog("Liked", "Liked");
            }
            else
            {
                DialogManager.ShowDialog("Error", result.message);
            }
        }

        private async void Dislike_Click(object sender, RoutedEventArgs e)
        {
            var result = await PostAPI.DislikeComment(CommentID, AppPersistent.UserToken);
            if (!result.error)
            {
                DialogManager.ShowDialog("Disliked", "Disliked");
            }
            else
            {
                DialogManager.ShowDialog("F U C K", result.message);
            }

        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            stLikes.StatValue = Likes;
            stDislikes.StatValue = Dislikes;
        }

        private void Avatar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PageNavigationManager.SwitchToSubpage(new UserProfilePage() { UserID = this.UserID });
            PageNavigationManager.CloseOverlay();
        }
    }
}
