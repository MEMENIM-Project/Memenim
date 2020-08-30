using AnonymDesktopClient.DataStructs;
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
using System.Data;
using AnonymDesktopClient.Pages;
using AnonymDesktopClient.Widgets;

namespace AnonymDesktopClient
{
    /// <summary>
    /// Interaction logic for PostsPage.xaml
    /// </summary>
    public partial class PostsPage : UserControl
    {

        public ApplicationPage ParrentPage {  set { m_ParrentPage = value; } }

        private ApplicationPage m_ParrentPage;

        public PostsPage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var posts = await ApiHelper.GetAllPosts();

            if(posts != null)
            {
                foreach (var post in posts)
                {
                    PostWidget widget = new PostWidget();
                    widget.PostText = post.text;
                    widget.ImageURL = post.attachments[0].photo.photo_medium;
                    widget.CurrentPostData = post;
                    postsPanel.Children.Add(widget);
                }
            }
        }
    }
}
