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
        PostsPage postsView;
        PostPage postView;
        MemesPage memesPage;

        public ApplicationPage()
        {
            InitializeComponent();
            postView = new PostPage();
            postsView = new PostsPage();
            //postsView.ParrentPage = this;
            memesPage = new MemesPage();
            PageNavigationManager.SubpageContentControl = contentArea;
            PageNavigationManager.SwitchToSubpage(postsView);
        }


        private void NavigationPersistent_RedirectRequested(object sender, RoutedEventArgs e)
        {
            string page = GeneralBlackboard.TryGetValue<string>(BlackBoardValues.EPageToRedirect);
            if(page != null)
            {
                NavigateTo(page);
            }
        }

        public void NavigateTo(string PageName)
        {
            switch (PageName)
            {
                case "AllPosts":
                    PageNavigationManager.SwitchToSubpage(postsView);
                    break;
                case "Post":
                    PageNavigationManager.SwitchToSubpage(postView);
                    break;
                case "Memes":
                    PageNavigationManager.SwitchToSubpage(memesPage);
                    break;
                case "Settings":
                    TriggerSettingsMenu();
                    break;
                case "Back":
                    UserControl backPage = GeneralBlackboard.TryGetValue<UserControl>(BlackBoardValues.EBackPage);
                    if (backPage != null)
                    {
                        PageNavigationManager.SwitchToSubpage(backPage);
                    }
                    break;
                default:
                    break;
            }

        }

        void TriggerSettingsMenu()
        {
            flyoutSettings.IsOpen = !flyoutSettings.IsOpen;
        }
    }
}
