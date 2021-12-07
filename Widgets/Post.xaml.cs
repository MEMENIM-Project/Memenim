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
    public partial class Post : WidgetContent
    {
        public static readonly RoutedEvent ClickEvent =
            EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(Post));
        public static readonly RoutedEvent PostDeleteEvent =
            EventManager.RegisterRoutedEvent(nameof(PostDelete), RoutingStrategy.Direct,
                typeof(EventHandler<RoutedEventArgs>), typeof(Post));
        
        

        public static readonly DependencyProperty CurrentPostDataProperty =
            DependencyProperty.Register(nameof(CurrentPostData), typeof(PostSchema), typeof(Post),
                new PropertyMetadata((PostSchema)null));



        public event EventHandler<RoutedEventArgs> Click
        {
            add
            {
                AddHandler(ClickEvent, value);
            }
            remove
            {
                RemoveHandler(ClickEvent, value);
            }
        }
        public event EventHandler<RoutedEventArgs> PostDelete
        {
            add
            {
                AddHandler(PostDeleteEvent, value);
            }
            remove
            {
                RemoveHandler(PostDeleteEvent, value);
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



        public Post()
        {
            InitializeComponent();
            DataContext = this;

            LocalizationUtils.LocalizationChanged += OnLocalizationChanged;
            SettingsManager.PersistentSettings.CurrentUserChanged += OnCurrentUserChanged;
        }

        ~Post()
        {
            LocalizationUtils.LocalizationChanged -= OnLocalizationChanged;
            SettingsManager.PersistentSettings.CurrentUserChanged -= OnCurrentUserChanged;
        }



        public void UpdateContextMenus()
        {
            EditButton
                .GetBindingExpression(VisibilityProperty)?
                .UpdateTarget();
            DeleteButton
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

        public async Task Share()
        {
            ShareButton.IsEnabled = false;

            try
            {
                if (CurrentPostData.Id == -1)
                    return;

                var link = MemenimProtocolApiUtils.GetPostLink(
                    CurrentPostData.Id);

                Clipboard.SetText(link);

                var result = await PostApi.AddRepost(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        CurrentPostData.Id)
                    .ConfigureAwait(true);

                if (result.IsError)
                {
                    await DialogManager.ShowErrorDialog(result.Message)
                        .ConfigureAwait(true);

                    return;
                }

                ++CurrentPostData.Shares;
            }
            finally
            {
                ShareButton.IsEnabled = true;
            }
        }



#pragma warning disable SS002 // DateTime.Now was referenced
        protected override void OnEnter(object sender,
            RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            if (ImageSizeLimit)
            {
                ImageGrid.MaxWidth = 500;

                Image.Stretch = Stretch.Uniform;
                Image.HorizontalAlignment = HorizontalAlignment.Stretch;
                Image.VerticalAlignment = VerticalAlignment.Stretch;
            }

            if (TextSizeLimit)
            {
                PostCategoryTextBlock.FontSize = 13;

                ContentTextBlock.FontSize = 13;

                ViewButton.StatValueFontSize = 9;
                ViewButton.ButtonSize = 20;
            }

            if (PreviewMode)
            {
                if (CurrentPostData != null)
                {
                    CurrentPostData.UtcDate = TimeUtils
                        .ToUnixTimeStamp(DateTime.Now);
                }

                LikeButton.IsEnabled = false;
                DislikeButton.IsEnabled = false;
                CommentButton.IsEnabled = false;
                ViewButton.IsEnabled = false;
                ShareButton.IsEnabled = false;
            }
        }
#pragma warning restore SS002 // DateTime.Now was referenced



        private void OnLocalizationChanged(object sender,
            LocalizationChangedEventArgs e)
        {
            PostCategoryTextBlock
                .GetBindingExpression(TextBlock.TextProperty)?
                .UpdateTarget();
        }

        private void OnCurrentUserChanged(object sender,
            UserChangedEventArgs e)
        {
            UpdateContextMenus();
        }



        private async void CopyPostLink_Click(object sender,
            RoutedEventArgs e)
        {
            await Share()
                .ConfigureAwait(true);
        }

        private void CopyPostId_Click(object sender,
            RoutedEventArgs e)
        {
            var id = CurrentPostData.Id
                .ToString();

            Clipboard.SetText(id);
        }

        private async void CopyPostText_Click(object sender,
            RoutedEventArgs e)
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



        private void Post_Click(object sender,
            RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent));
        }

        private async void Edit_Click(object sender,
            RoutedEventArgs e)
        {
            EditButton.IsEnabled = false;

            try
            {
                if (!CurrentPostData.OwnerId.HasValue || CurrentPostData.OwnerId == -1)
                    return;
                if (CurrentPostData.OwnerId != SettingsManager.PersistentSettings.CurrentUser.Id)
                    return;

                var title = LocalizationUtils.GetLocalized("EditingPostTitle");
                var message = LocalizationUtils.GetLocalized("EditingPostMessage");

                var oldValue = CurrentPostData.Text;
                var value = await DialogManager.ShowMultilineTextDialog(
                        title, message, oldValue)
                    .ConfigureAwait(true);

                if (value == null)
                    return;

                var postEditData = new PostEditSchema
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

                    return;
                }

                CurrentPostData.Text = value;
            }
            finally
            {
                EditButton.IsEnabled = true;
            }
        }

        private async void Delete_Click(object sender,
            RoutedEventArgs e)
        {
            DeleteButton.IsEnabled = false;

            try
            {
                if (!CurrentPostData.OwnerId.HasValue || CurrentPostData.OwnerId == -1)
                    return;
                if (CurrentPostData.OwnerId != SettingsManager.PersistentSettings.CurrentUser.Id)
                    return;

                var confirmResult = await DialogManager.ShowConfirmationDialog()
                    .ConfigureAwait(true);

                if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
                    return;

                var result = await PostApi.Remove(
                        SettingsManager.PersistentSettings.CurrentUser.Token,
                        CurrentPostData.Id)
                    .ConfigureAwait(true);

                if (result.IsError)
                {
                    await DialogManager.ShowErrorDialog(result.Message)
                        .ConfigureAwait(true);

                    return;
                }

                Visibility = Visibility.Collapsed;

                RaiseEvent(new RoutedEventArgs(PostDeleteEvent, sender));
            }
            finally
            {
                DeleteButton.IsEnabled = true;
            }
        }

        private async void Share_Click(object sender,
            RoutedEventArgs e)
        {
            await Share()
                .ConfigureAwait(true);
        }

        private async void Like_Click(object sender,
            RoutedEventArgs e)
        {
            LikeButton.IsEnabled = false;

            try
            {
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

                    return;
                }

                if (CurrentPostData.Likes.MyCount == 0)
                    ++CurrentPostData.Likes.MyCount;
                else
                    --CurrentPostData.Likes.MyCount;

                CurrentPostData.Likes.TotalCount = result.Data.Count;
            }
            finally
            {
                LikeButton.IsEnabled = true;
            }
        }

        private async void Dislike_Click(object sender,
            RoutedEventArgs e)
        {
            DislikeButton.IsEnabled = false;

            try
            {
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
                    if (string.Compare(result.Message, "id is not defined", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (CurrentPostData.Dislikes.MyCount == 0)
                        {
                            ++CurrentPostData.Dislikes.MyCount;
                            ++CurrentPostData.Dislikes.TotalCount;
                        }
                        else
                        {
                            --CurrentPostData.Dislikes.MyCount;
                            --CurrentPostData.Dislikes.TotalCount;
                        }

                        return;
                    }

                    await DialogManager.ShowErrorDialog(result.Message)
                        .ConfigureAwait(true);

                    return;
                }

                if (CurrentPostData.Dislikes.MyCount == 0)
                    ++CurrentPostData.Dislikes.MyCount;
                else
                    --CurrentPostData.Dislikes.MyCount;

                CurrentPostData.Dislikes.TotalCount = result.Data.Count;
            }
            finally
            {
                DislikeButton.IsEnabled = true;
            }
        }

        private void Image_MouseLeftButtonUp(object sender,
            MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPostData.Attachments[0].Photo.MediumUrl))
                return;

            NavigationController.Instance.RequestOverlay<ImagePreviewOverlayPage>(new ImagePreviewOverlayViewModel
            {
                ImageSource = CurrentPostData.Attachments[0].Photo.MediumUrl
            });

            e.Handled = true;
        }
    }
}
