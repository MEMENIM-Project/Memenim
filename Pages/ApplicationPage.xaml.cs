using Memenim.Managers;
using System.Windows;
using System.Windows.Controls;

namespace Memenim.Pages
{
    /// <summary>
    /// Interaction logic for ApplicationPage.xaml
    /// </summary>
    public partial class ApplicationPage : PageContent
    {
        public ApplicationPage() : base()
        {
            InitializeComponent();
            PageNavigationManager.SubpageContentControl = contentArea;
            PageNavigationManager.OverlayContentControl = overlayArea;
            PageNavigationManager.SwitchToSubpage<FeedPage>();
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
                    PageNavigationManager.SwitchToSubpage<FeedPage>();
                    break;
                case "CreatePost":
                    PageNavigationManager.SwitchToSubpage<SubmitPostPage>();
                    break;
                case "Memes":
                    PageNavigationManager.SwitchToSubpage<MemesPage>();
                    break;
                case "Placeholder":
                    PageNavigationManager.SwitchToSubpage<PlaceholderPage>();
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
            PageNavigationManager.SwitchToSubpage<SettingsPage>();
        }
    }
}
