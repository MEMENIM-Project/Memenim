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
            bool result = await ApiHelper.LikeComment(CommentID);
            if(result)
            {
                DialogManager.ShowDialog("Liked", "Liked");
            }
        }

        private async void Dislike_Click(object sender, RoutedEventArgs e)
        {
            bool result = await ApiHelper.DislikeComment(CommentID);
            if (result)
            {
                DialogManager.ShowDialog("Disliked", "Disliked");
            }

        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            stLikes.StatValue = Likes;
            stDislikes.StatValue = Dislikes;
        }
    }
}
