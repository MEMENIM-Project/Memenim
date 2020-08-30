using AnonymDesktopClient.DataStructs;
using AnonymDesktopClient.Pages;
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

namespace AnonymDesktopClient.Widgets
{
    /// <summary>
    /// Interaction logic for PostWidget.xaml
    /// </summary>
    public partial class PostWidget : UserControl
    {
        public string ImageURL { get; set; }
        public string PostText { get; set; }
        public PostData CurrentPostData { get; set; }

        public PostWidget()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ViewPost_Click(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EPostData, CurrentPostData);
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, new PostsPage());
            PageNavigationManager.SwitchToSubpage(new PostPage());
        }

        private void CopyPostID_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CurrentPostData.id.ToString());
        }
    }
}
