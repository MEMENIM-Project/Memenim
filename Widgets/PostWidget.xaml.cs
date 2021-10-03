using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Memenim.Utils;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Settings;
using Memenim.Pages;
using Memenim.Pages.ViewModel;
using RIS.Localization;

namespace Memenim.Widgets
{
    public partial class PostWidget : WidgetContent
    {
        public static readonly RoutedEvent OnPostClicked =
            EventManager.RegisterRoutedEvent(nameof(PostClick), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(PostWidget));
        public static readonly RoutedEvent OnPostDeleted =
            EventManager.RegisterRoutedEvent(nameof(PostDelete), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(PostWidget));
        public static readonly DependencyProperty CurrentPostDataProperty =
            DependencyProperty.Register(nameof(CurrentPostData), typeof(PostSchema), typeof(PostWidget),
                new PropertyMetadata((PostSchema) null));

        public event EventHandler<RoutedEventArgs> PostClick
        {
            add
            {
                AddHandler(OnPostClicked, value);
            }
            remove
            {
                RemoveHandler(OnPostClicked, value);
            }
        }
        public event EventHandler<RoutedEventArgs> PostDelete
        {
            add
            {
                AddHandler(OnPostDeleted, value);
            }
            remove
            {
                RemoveHandler(OnPostDeleted, value);
            }
        }

        public PostSchema CurrentPostData
        {
            get
            {
                return (PostSchema)GetValue(CurrentPostDataProperty);
            }
            set
            {
                SetValue(CurrentPostDataProperty, value);
            }
        }
        public bool PreviewMode { get; set; }
        public bool ImageSizeLimit { get; set; }
        public bool TextSizeLimit { get; set; }

        public PostWidget()
        {
            InitializeComponent();
            DataContext = this;

            LocalizationUtils.LocalizationChanged += OnLocalizationChanged;
            SettingsManager.PersistentSettings.CurrentUserChanged += OnCurrentUserChanged;
        }

        ~PostWidget()
        {
            LocalizationUtils.LocalizationChanged -= OnLocalizationChanged;
            SettingsManager.PersistentSettings.CurrentUserChanged -= OnCurrentUserChanged;
        }

        public void UpdateContextMenus()
        {
            btnEdit
                .GetBindingExpression(VisibilityProperty)?
                .UpdateTarget();
            btnDelete
                .GetBindingExpression(VisibilityProperty)?
                .UpdateTarget();
        }

        public async Task UpdatePost()
        {
            var result = await PostApi.GetById(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    CurrentPostData.Id)
                .ConfigureAwait(true);

            if (result.IsError)
            {
                await DialogManager.ShowErrorDialog(result.Message)
                    .ConfigureAwait(true);
                return;
            }

            if (result.Data == null)
            {
                await DialogManager.ShowErrorDialog(result.Message)
                    .ConfigureAwait(true);
                return;
            }

            CurrentPostData = result.Data;

            UpdateContextMenus();
        }

        public void LoadImage(string url)
        {
            if (url == null || !Uri.TryCreate(url, UriKind.Absolute, out Uri _))
                return;

            CurrentPostData.Attachments[0].Photo.MediumUrl = url;
        }

        public void ClearImage()
        {
            CurrentPostData.Attachments[0].Photo.MediumUrl = string.Empty;
        }

        public async Task Share()
        {
            stShares.IsEnabled = false;

            var link = $"memenim://app/post/id/{CurrentPostData.Id}";

            Clipboard.SetText(link);

            var result = await PostApi.AddRepost(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    CurrentPostData.Id)
                .ConfigureAwait(true);

            if (result.IsError)
            {
                await DialogManager.ShowErrorDialog(result.Message)
                    .ConfigureAwait(true);

                stShares.IsEnabled = true;
                return;
            }

            ++CurrentPostData.Shares;

            stShares.IsEnabled = true;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (ImageSizeLimit)
            {
                PostImageGrid.MaxWidth = 500;

                PostImage.Stretch = Stretch.Uniform;
                PostImage.HorizontalAlignment = HorizontalAlignment.Stretch;
                PostImage.VerticalAlignment = VerticalAlignment.Stretch;
            }

            if (TextSizeLimit)
            {
                txtPostCategory.FontSize = 13;

                txtContent.FontSize = 13;

                stViews.StatValueFontSize = 9;
                stViews.ButtonSize = 20;
            }

            if (PreviewMode)
            {
#pragma warning disable SS002 // DateTime.Now was referenced
                if (CurrentPostData != null)
                    CurrentPostData.UtcDate = TimeUtils.ToUnixTimeStamp(DateTime.Now);
#pragma warning restore SS002 // DateTime.Now was referenced

                postMenu.IsEnabled = false;
                postMenu.Visibility = Visibility.Collapsed;

                stLikes.IsEnabled = false;
                stDislikes.IsEnabled = false;
                stComments.IsEnabled = false;
                stViews.IsEnabled = false;
                stShares.IsEnabled = false;
            }
        }

        private void OnLocalizationChanged(object sender, LocalizationChangedEventArgs e)
        {
            txtPostCategory
                .GetBindingExpression(TextBlock.TextProperty)?
                .UpdateTarget();
        }

        private void OnCurrentUserChanged(object sender, UserChangedEventArgs e)
        {
            UpdateContextMenus();
        }

        private void Post_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnPostClicked));
        }

        private async void CopyPostLink_Click(object sender, RoutedEventArgs e)
        {
            await Share()
                .ConfigureAwait(true);
        }

        private void CopyPostId_Click(object sender, RoutedEventArgs e)
        {
            var id = CurrentPostData.Id.ToString();

            Clipboard.SetText(id);
        }

        private async void CopyPostText_Click(object sender, RoutedEventArgs e)
        {
            var text = CurrentPostData.Text;

            if (text == null)
            {
                var message = LocalizationUtils
                    .GetLocalized("CopyingToClipboardErrorMessage");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            Clipboard.SetText(text);
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            btnEdit.IsEnabled = false;

            var title = LocalizationUtils.GetLocalized("EditingPostTitle");
            var message = LocalizationUtils.GetLocalized("EditingPostMessage");

            string oldValue = CurrentPostData.Text;
            string value = await DialogManager.ShowMultilineTextDialog(
                    title, message, oldValue)
                .ConfigureAwait(true);

            if (value == null)
            {
                btnEdit.IsEnabled = true;
                return;
            }

            PostEditSchema postEditData = new PostEditSchema
            {
                Id = CurrentPostData.Id,
                Text = value,
                IsAdult = CurrentPostData.IsAdult,
                IsAnonymous = CurrentPostData.IsAnonymous,
                CategoryId = CurrentPostData.CategoryId,
                Filter = CurrentPostData.Filter,
                IsHidden = CurrentPostData.IsHidden,
                IsCommentsOpen = CurrentPostData.IsCommentsOpen,
                Type = CurrentPostData.Type
            };

            var request = await PostApi.Edit(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    postEditData)
                .ConfigureAwait(true);

            if (request.IsError)
            {
                await DialogManager.ShowErrorDialog(request.Message)
                    .ConfigureAwait(true);

                btnEdit.IsEnabled = true;
                return;
            }

            CurrentPostData.Text = value;

            btnEdit.IsEnabled = true;
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            btnDelete.IsEnabled = false;

            var confirmResult = await DialogManager.ShowConfirmationDialog()
                .ConfigureAwait(true);

            if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                btnDelete.IsEnabled = true;
                return;
            }

            var result = await PostApi.Remove(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    CurrentPostData.Id)
                .ConfigureAwait(true);

            if (result.IsError)
            {
                await DialogManager.ShowErrorDialog(result.Message)
                    .ConfigureAwait(true);

                btnDelete.IsEnabled = true;
                return;
            }

            Visibility = Visibility.Collapsed;

            RaiseEvent(new RoutedEventArgs(OnPostDeleted));

            btnDelete.IsEnabled = true;
        }

        private async void Share_Click(object sender, RoutedEventArgs e)
        {
            await Share()
                .ConfigureAwait(true);
        }

        private async void Like_Click(object sender, RoutedEventArgs e)
        {
            stLikes.IsEnabled = false;

            ApiResponse<CountSchema> result;

            if (CurrentPostData.Likes.MyCount == 0)
            {
                result = await PostApi.AddLike(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        CurrentPostData.Id)
                    .ConfigureAwait(true);
            }
            else
            {
                result = await PostApi.RemoveLike(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        CurrentPostData.Id)
                    .ConfigureAwait(true);
            }

            if (result.IsError)
            {
                await DialogManager.ShowErrorDialog(result.Message)
                    .ConfigureAwait(true);

                stLikes.IsEnabled = true;
                return;
            }

            if (CurrentPostData.Likes.MyCount == 0)
                ++CurrentPostData.Likes.MyCount;
            else
                --CurrentPostData.Likes.MyCount;

            CurrentPostData.Likes.TotalCount = result.Data.Count;

            stLikes.IsEnabled = true;
        }

        private async void Dislike_Click(object sender, RoutedEventArgs e)
        {
            stDislikes.IsEnabled = false;

            ApiResponse<CountSchema> result;

            if (CurrentPostData.Dislikes.MyCount == 0)
            {
                result = await PostApi.AddDislike(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        CurrentPostData.Id)
                    .ConfigureAwait(true);
            }
            else
            {
                result = await PostApi.RemoveDislike(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        CurrentPostData.Id)
                    .ConfigureAwait(true);
            }

            if (result.IsError)
            {
                await DialogManager.ShowErrorDialog(result.Message)
                    .ConfigureAwait(true);

                stDislikes.IsEnabled = true;
                return;
            }

            if (CurrentPostData.Dislikes.MyCount == 0)
                ++CurrentPostData.Dislikes.MyCount;
            else
                --CurrentPostData.Dislikes.MyCount;

            CurrentPostData.Dislikes.TotalCount = result.Data.Count;

            stDislikes.IsEnabled = true;
        }

        private void PostImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPostData.Attachments[0].Photo.MediumUrl))
                return;

            NavigationController.Instance.RequestOverlay<ImagePreviewOverlayPage>(new ImagePreviewOverlayViewModel()
            {
                ImageSource = CurrentPostData.Attachments[0].Photo.MediumUrl
            });

            e.Handled = true;
        }
    }
}
