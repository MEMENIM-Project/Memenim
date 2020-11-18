using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.TabLayouts;
using Memenim.TabLayouts.NavigationBar;

namespace Memenim.Widgets
{
    public partial class NavigationBar : UserControl
    {
        public static readonly RoutedEvent OnRedirectRequested =
            EventManager.RegisterRoutedEvent(nameof(RedirectRequest), RoutingStrategy.Bubble, typeof(EventHandler<RoutedEventArgs>), typeof(NavigationBar));
        public static readonly DependencyProperty TopNavButtonsProperty =
            DependencyProperty.Register(nameof(TopNavButtons), typeof(ObservableCollection<IconButton>), typeof(NavigationBar),
                new PropertyMetadata(new ObservableCollection<IconButton>()));
        public static readonly DependencyProperty CentralNavButtonsProperty =
            DependencyProperty.Register(nameof(CentralNavButtons), typeof(ObservableCollection<IconButton>), typeof(NavigationBar),
                new PropertyMetadata(new ObservableCollection<IconButton>()));
        public static readonly DependencyProperty BottomNavButtonsProperty =
            DependencyProperty.Register(nameof(BottomNavButtons), typeof(ObservableCollection<IconButton>), typeof(NavigationBar),
                new PropertyMetadata(new ObservableCollection<IconButton>()));

        public event EventHandler<RoutedEventArgs> RedirectRequest
        {
            add
            {
                AddHandler(OnRedirectRequested, value);
            }
            remove
            {
                RemoveHandler(OnRedirectRequested, value);
            }
        }

        public ObservableCollection<IconButton> TopNavButtons
        {
            get
            {
                return (ObservableCollection<IconButton>)GetValue(TopNavButtonsProperty);
            }
            set
            {
                SetValue(TopNavButtonsProperty, value);
            }
        }
        public ObservableCollection<IconButton> CentralNavButtons
        {
            get
            {
                return (ObservableCollection<IconButton>)GetValue(CentralNavButtonsProperty);
            }
            set
            {
                SetValue(CentralNavButtonsProperty, value);
            }
        }
        public ObservableCollection<IconButton> BottomNavButtons
        {
            get
            {
                return (ObservableCollection<IconButton>)GetValue(BottomNavButtonsProperty);
            }
            set
            {
                SetValue(BottomNavButtonsProperty, value);
            }
        }

        public NavigationBar()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ChangeButtons(ObservableCollection<IconButton> buttonsList,
            NavRedirectButtonNode[] nodesList)
        {
            buttonsList.Clear();

            foreach (var node in nodesList)
            {
                IconButton button = new IconButton
                {
                    PageName = node.PageName,
                    IconKind = node.IconKind,
                    Height = 40
                };
                button.IconButtonClick += OnNavButtonClick;

                buttonsList.Add(button);
            }
        }

        public async Task SwitchLayout(NavBarLayoutType type)
        {
            ResourceDictionary dictionary = await TabLayoutsManager.GetLayout(this, type.ToString())
                .ConfigureAwait(true);

            if (dictionary == null)
                return;

            ChangeButtons(TopNavButtons, (NavRedirectButtonNode[])dictionary[nameof(TopNavButtons)]);
            ChangeButtons(CentralNavButtons, (NavRedirectButtonNode[])dictionary[nameof(CentralNavButtons)]);
            ChangeButtons(BottomNavButtons, (NavRedirectButtonNode[])dictionary[nameof(BottomNavButtons)]);

        }

        private async void OnNavButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is IconButton button)
                {
                    if (button.PageName == "Back")
                    {
                        NavigationController.Instance.GoBack();
                    }
                    else if (button.PageName == "UserProfilePage")
                    {
                        if (NavigationController.Instance.PageContent.Content is UserProfilePage page
                            && page.DataContext is UserProfileViewModel viewModel
                            && viewModel.CurrentProfileData.id == SettingsManager.PersistentSettings.CurrentUserId)
                        {
                            return;
                        }

                        NavigationController.Instance.RequestPage<UserProfilePage>(new UserProfileViewModel
                        {
                            CurrentProfileData = new ProfileSchema
                            {
                                id = SettingsManager.PersistentSettings.CurrentUserId
                            }
                        });
                    }
                    else
                    {
                        NavigationController.Instance.RequestPage(Type.GetType($"Memenim.Pages.{button.PageName}"));
                    }
                }
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Navigation error", ex.Message)
                    .ConfigureAwait(true);
            }

            RaiseEvent(new RoutedEventArgs(OnRedirectRequested));
        }
    }
}
