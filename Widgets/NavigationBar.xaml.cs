using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.TabLayouts;
using Memenim.TabLayouts.NavigationBar;
using Memenim.Utils;

namespace Memenim.Widgets
{
    public partial class NavigationBar : WidgetContent
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
                    IconKind = node.IconKind,
                    Information = node.PageName,
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
                    if (button.Information == "Back")
                    {
                        NavigationController.Instance.GoBack();
                    }
                    else if (button.Information == "SettingsFlyout")
                    {
                        if (!MainWindow.Instance.IsOpenSettings())
                        {
                            MainWindow.Instance.ShowSettings();
                        }
                        else
                        {
                            MainWindow.Instance.HideSettings();
                        }
                    }
                    else if (button.Information == "UserProfilePage")
                    {
                        if (NavigationController.Instance.PageContent.Content is UserProfilePage page
                            && page.DataContext is UserProfileViewModel viewModel
                            && viewModel.CurrentProfileData.Id == SettingsManager.PersistentSettings.CurrentUser.Id)
                        {
                            return;
                        }

                        NavigationController.Instance.RequestPage<UserProfilePage>(new UserProfileViewModel
                        {
                            CurrentProfileData = new ProfileSchema
                            {
                                Id = SettingsManager.PersistentSettings.CurrentUser.Id
                            }
                        });
                    }
                    else
                    {
                        NavigationController.Instance.RequestPage(Type.GetType($"Memenim.Pages.{button.Information}"));
                    }
                }
            }
            catch (Exception ex)
            {
                var title = LocalizationUtils.GetLocalized("NavigationErrorTitle");

                await DialogManager.ShowMessageDialog(title, ex.Message)
                    .ConfigureAwait(true);
            }

            RaiseEvent(new RoutedEventArgs(OnRedirectRequested));
        }
    }
}
