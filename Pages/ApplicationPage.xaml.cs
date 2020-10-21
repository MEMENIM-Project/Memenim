using System.Windows;
using System.Windows.Controls;

namespace Memenim.Pages
{
    public partial class ApplicationPage : UserControl
    {
        private readonly SubmitPostPage _submitPostPage;
        private readonly FeedPage _feedPage;
        private readonly MemesPage _memesPage;
        private readonly PlaceholderPage _placeholderPage;
        private readonly SettingsPage _settingsPage;

        public ApplicationPage()
        {
            InitializeComponent();
            DataContext = this;

            _feedPage = new FeedPage();
            _submitPostPage = new SubmitPostPage();
            _memesPage = new MemesPage();
            _settingsPage = new SettingsPage();
            _placeholderPage = new PlaceholderPage();

            PageNavigationManager.SubPageContentControl = contentArea;
            PageNavigationManager.OverlayContentControl = overlayArea;
            PageNavigationManager.SwitchToSubPage(_feedPage);
        }

        public void NavigateTo(string pageName)
        {
            switch (pageName)
            {
                case "AllPosts":
                    PageNavigationManager.SwitchToSubPage(_feedPage);
                    break;
                case "CreatePost":
                    PageNavigationManager.SwitchToSubPage(_submitPostPage);
                    break;
                case "Memes":
                    PageNavigationManager.SwitchToSubPage(_memesPage);
                    break;
                case "Placeholder":
                    PageNavigationManager.SwitchToSubPage(_placeholderPage);
                    break;
                case "Settings":
                    PageNavigationManager.SwitchToSubPage(_settingsPage);
                    break;
                case "Back":
                    PageNavigationManager.GoBack();
                    break;
                default:
                    break;
            }
        }

        private void NavigationPersistent_RedirectRequested(object sender, RoutedEventArgs e)
        {
            string page = GeneralBlackboard.TryGetValue<string>(BlackBoardValues.EPageToRedirect);

            if (page != null)
                NavigateTo(page);
        }
    }
}
