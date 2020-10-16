using System.Windows;
using System.Windows.Controls;

namespace Memenim.Pages
{
    /// <summary>
    /// Interaction logic for ApplicationPage.xaml
    /// </summary>
    public partial class ApplicationPage : UserControl
    {
        SubmitPostPage m_SubmitPostPage;
        FeedPage m_FeedPage;
        PostPage m_PostPage;
        MemesPage m_MemesPage;
        PlaceholderPage m_PlaceholderPage;
        SettingsPage m_SettingsPage;

        public ApplicationPage()
        {
            InitializeComponent();
            m_PostPage = new PostPage();
            m_FeedPage = new FeedPage();
            m_SubmitPostPage = new SubmitPostPage();
            m_MemesPage = new MemesPage();
            m_SettingsPage = new SettingsPage();
            m_PlaceholderPage = new PlaceholderPage();
            PageNavigationManager.SubpageContentControl = contentArea;
            PageNavigationManager.OverlayContentControl = overlayArea;
            PageNavigationManager.SwitchToSubpage(m_FeedPage);
        }


        private void NavigationPersistent_RedirectRequested(object sender, RoutedEventArgs e)
        {
            string page = GeneralBlackboard.TryGetValue<string>(BlackBoardValues.EPageToRedirect);
            if (page != null)
            {
                NavigateTo(page);
            }
        }

        public void NavigateTo(string PageName)
        {
            switch (PageName)
            {
                case "AllPosts":
                    PageNavigationManager.SwitchToSubpage(m_FeedPage);
                    break;
                case "Post":
                    PageNavigationManager.SwitchToSubpage(m_PostPage);
                    break;
                case "CreatePost":
                    PageNavigationManager.SwitchToSubpage(m_SubmitPostPage);
                    break;
                case "Memes":
                    PageNavigationManager.SwitchToSubpage(m_MemesPage);
                    break;
                case "Placeholder":
                    PageNavigationManager.SwitchToSubpage(m_PlaceholderPage);
                    break;
                case "Settings":
                    TriggerSettingsMenu();
                    break;
                case "Back":
                    PageNavigationManager.GoBack();
                    break;
                default:
                    break;
            }

        }

        void TriggerSettingsMenu()
        {
            PageNavigationManager.SwitchToSubpage(m_SettingsPage);
        }
    }
}
