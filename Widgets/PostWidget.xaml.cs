using System;
using System.Globalization;
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

namespace Memenim.Widgets
{
    public partial class PostWidget : UserControl
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
        public bool CommentsIsOpen
        {
            get
            {
                return CurrentPostData?.open_comments == 1;
            }
        }
        public bool PreviewMode { get; set; }
        public bool ImageSizeLimit { get; set; }

        public PostWidget()
        {
            InitializeComponent();
            DataContext = this;
        }

        public async Task UpdatePost()
        {
            var result = await PostApi.GetById(
                    SettingsManager.PersistentSettings.CurrentUserToken,
                    CurrentPostData.id)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);
                return;
            }

            if (result.data == null)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);
                return;
            }

            CurrentPostData = result.data;
        }

        public void LoadImage(string url)
        {
            if (url == null || !Uri.TryCreate(url, UriKind.Absolute, out Uri _))
                return;

            CurrentPostData.attachments[0].photo.photo_medium = url;
        }

        public void ClearImage()
        {
            CurrentPostData.attachments[0].photo.photo_medium = string.Empty;
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

            wdgPoster.PostTime = TimeUtils.UnixTimeStampToDateTime(CurrentPostData?.date ?? 0L)
                .ToString(CultureInfo.CurrentCulture);
            wdgPoster.IsAnonymous = CurrentPostData?.author_watch != 2;

            btnEdit.Visibility =
                (CurrentPostData?.owner_id ?? -1) != SettingsManager.PersistentSettings.CurrentUserId
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            btnDelete.Visibility =
                (CurrentPostData?.owner_id ?? -1) != SettingsManager.PersistentSettings.CurrentUserId
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            stComments.Visibility = !CommentsIsOpen
                ? Visibility.Collapsed
                : Visibility.Visible;

            if (PreviewMode)
            {
                wdgPoster.PostTime = (CurrentPostData?.date ?? 0L) == 0L
                    ? DateTime.UtcNow.ToLocalTime().ToString(CultureInfo.CurrentCulture)
                    : wdgPoster.PostTime;
                wdgPoster.IsAnonymous = CurrentPostData?.author_watch != 1;

                postMenu.IsEnabled = false;
                postMenu.Visibility = Visibility.Collapsed;

                stLikes.IsEnabled = false;
                stDislikes.IsEnabled = false;
                stComments.IsEnabled = false;
                stViews.IsEnabled = false;
                stReposts.IsEnabled = false;
            }
        }

        private void Post_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnPostClicked));
        }

        private void CopyPostId_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CurrentPostData.id.ToString());
        }

        private void CopyPostText_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CurrentPostData.text);
        }

        private async void Edit_Click(object sender, RoutedEventArgs e)
        {
            btnEdit.IsEnabled = false;

            string oldValue = CurrentPostData.text;
            string value = await DialogManager.ShowMultilineTextDialog("Edit post",
                    "Enter post text", oldValue)
                .ConfigureAwait(true);

            if (value == null)
            {
                btnEdit.IsEnabled = true;
                return;
            }

            PostEditSchema postEditData = new PostEditSchema
            {
                id = CurrentPostData.id,
                text = value,
                adult = CurrentPostData.adult,
                author_watch = CurrentPostData.author_watch.ToString(),
                category = CurrentPostData.category,
                filter = CurrentPostData.filter,
                hidden = CurrentPostData.hidden,
                open_comments = CurrentPostData.open_comments,
                type = CurrentPostData.type
            };

            var request = await PostApi.Edit(
                    SettingsManager.PersistentSettings.CurrentUserToken,
                    postEditData)
                .ConfigureAwait(true);

            if (request.error)
            {
                await DialogManager.ShowDialog("F U C K", request.message)
                    .ConfigureAwait(true);

                btnEdit.IsEnabled = true;
                return;
            }

            CurrentPostData.text = value;

            btnEdit.IsEnabled = true;
        }

        private async void Delete_Click(object sender, RoutedEventArgs e)
        {
            btnDelete.IsEnabled = false;

            var confirmResult = await DialogManager.ShowDialog("Confirmation", "Are you sure?",
                    MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative)
                .ConfigureAwait(true);

            if (confirmResult != MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                btnDelete.IsEnabled = true;
                return;
            }

            var result = await PostApi.Remove(
                    SettingsManager.PersistentSettings.CurrentUserToken,
                    CurrentPostData.id)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);

                btnDelete.IsEnabled = true;
                return;
            }

            Visibility = Visibility.Collapsed;

            RaiseEvent(new RoutedEventArgs(OnPostDeleted));

            btnDelete.IsEnabled = true;
        }

        private async void Like_Click(object sender, RoutedEventArgs e)
        {
            stLikes.IsEnabled = false;

            ApiResponse<CountSchema> result;

            if (CurrentPostData.likes.my == 0)
            {
                result = await PostApi.AddLike(
                        SettingsManager.PersistentSettings.CurrentUserToken,
                        CurrentPostData.id)
                    .ConfigureAwait(true);
            }
            else
            {
                result = await PostApi.RemoveLike(
                        SettingsManager.PersistentSettings.CurrentUserToken,
                        CurrentPostData.id)
                    .ConfigureAwait(true);
            }

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);

                stLikes.IsEnabled = true;
                return;
            }

            if (CurrentPostData.likes.my == 0)
                ++CurrentPostData.likes.my;
            else
                --CurrentPostData.likes.my;

            CurrentPostData.likes.count = result.data.count;

            stLikes.IsEnabled = true;
        }

        private async void Dislike_Click(object sender, RoutedEventArgs e)
        {
            stDislikes.IsEnabled = false;

            ApiResponse<CountSchema> result;

            if (CurrentPostData.dislikes.my == 0)
            {
                result = await PostApi.AddDislike(
                        SettingsManager.PersistentSettings.CurrentUserToken,
                        CurrentPostData.id)
                    .ConfigureAwait(true);
            }
            else
            {
                result = await PostApi.RemoveDislike(
                        SettingsManager.PersistentSettings.CurrentUserToken,
                        CurrentPostData.id)
                    .ConfigureAwait(true);
            }

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);

                stDislikes.IsEnabled = true;
                return;
            }

            if (CurrentPostData.dislikes.my == 0)
                ++CurrentPostData.dislikes.my;
            else
                --CurrentPostData.dislikes.my;

            CurrentPostData.dislikes.count = result.data.count;

            stDislikes.IsEnabled = true;
        }

        private void PostImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPostData.attachments[0].photo.photo_medium))
                return;

            NavigationController.Instance.RequestOverlay<ImagePreviewOverlayPage>(new ImagePreviewOverlayViewModel()
            {
                ImageSource = CurrentPostData.attachments[0].photo.photo_medium
            });

            e.Handled = true;
        }
    }
}
