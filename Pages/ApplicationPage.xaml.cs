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

namespace AnonymDesktopClient.Pages
{
    /// <summary>
    /// Interaction logic for ApplicationPage.xaml
    /// </summary>
    public partial class ApplicationPage : UserControl
    {
        PostsPage postView;
        MemesPage memesPage;

        public ApplicationPage()
        {
            InitializeComponent();
            postView = new PostsPage();
            memesPage = new MemesPage();
            contentArea.Content = postView;
        }

        private void NavigationPersistent_RedirectRequested(object sender, RoutedEventArgs e)
        {
            string page = GeneralBlackboard.TryGetValue<string>(BlackBoardValues.EPageToRedirect);
            if(page != null)
            {
                switch(page)
                {
                    case "AllPosts":
                        contentArea.Content = postView;
                        break;
                    case "Memes":
                        contentArea.Content = memesPage;
                        break;
                    case "Settings":
                        break;
                    default:
                        break;
                }
            }
        }

        private void NavigationPersistent_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
