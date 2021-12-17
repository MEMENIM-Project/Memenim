using System;
using System.Windows;
using System.Windows.Media;

namespace Memenim.Widgets
{
    public partial class UserAvatar : WidgetContent
    {
        public static readonly DependencyProperty MinSizeProperty =
            DependencyProperty.Register(nameof(MinSize), typeof(double), typeof(UserAvatar),
                new PropertyMetadata(50D));
        public static readonly DependencyProperty MaxSizeProperty =
            DependencyProperty.Register(nameof(MaxSize), typeof(double), typeof(UserAvatar),
                new PropertyMetadata(double.PositiveInfinity));
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(nameof(Size), typeof(double), typeof(UserAvatar),
                new PropertyMetadata(double.NaN));
        public static readonly DependencyProperty ImageUrlProperty =
            DependencyProperty.Register(nameof(ImageUrl), typeof(string), typeof(UserAvatar),
                new PropertyMetadata((string)null));
        public static readonly DependencyProperty ImageLoadedBackgroundProperty =
            DependencyProperty.Register(nameof(ImageLoadedBackground), typeof(Brush), typeof(UserAvatar),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent), ImageLoadedBackgroundChangedCallback));
        public static readonly DependencyProperty ImageUnloadedBackgroundProperty =
            DependencyProperty.Register(nameof(ImageUnloadedBackground), typeof(Brush), typeof(UserAvatar),
                new PropertyMetadata(new SolidColorBrush(Colors.Transparent), ImageUnloadedBackgroundChangedCallback));



        public double MinSize
        {
            get
            {
                return (double)GetValue(MinSizeProperty);
            }
            set
            {
                SetValue(MinSizeProperty, value);
            }
        }
        public double MaxSize
        {
            get
            {
                return (double)GetValue(MaxSizeProperty);
            }
            set
            {
                SetValue(MaxSizeProperty, value);
            }
        }
        public double Size
        {
            get
            {
                return (double)GetValue(SizeProperty);
            }
            set
            {
                SetValue(SizeProperty, value);
            }
        }
        public string ImageUrl
        {
            get
            {
                return (string)GetValue(ImageUrlProperty);
            }
            set
            {
                SetValue(ImageUrlProperty, value);
            }
        }
        public Brush ImageLoadedBackground
        {
            get
            {
                return (Brush)GetValue(ImageLoadedBackgroundProperty);
            }
            set
            {
                SetValue(ImageLoadedBackgroundProperty, value);
            }
        }
        public Brush ImageUnloadedBackground
        {
            get
            {
                return (Brush)GetValue(ImageUnloadedBackgroundProperty);
            }
            set
            {
                SetValue(ImageUnloadedBackgroundProperty, value);
            }
        }



        public UserAvatar()
        {
            InitializeComponent();
        }



        private static void ImageLoadedBackgroundChangedCallback(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var target = sender as UserAvatar;

            target?.OnImageLoadedBackgroundChanged(e);
        }

        private static void ImageUnloadedBackgroundChangedCallback(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var target = sender as UserAvatar;

            target?.OnImageUnloadedBackgroundChanged(e);
        }



#pragma warning disable IDE0060 // Удалите неиспользуемый параметр
        // ReSharper disable UnusedParameter.Local

        private void OnImageLoadedBackgroundChanged(
            DependencyPropertyChangedEventArgs e)
        {
            UpdateImageBackground();
        }

        private void OnImageUnloadedBackgroundChanged(
            DependencyPropertyChangedEventArgs e)
        {
            UpdateImageBackground();
        }

        // ReSharper restore UnusedParameter.Local
#pragma warning restore IDE0060 // Удалите неиспользуемый параметр



        private void UpdateImageBackground()
        {
            UpdateImageBackground(
                new Size(Image.ActualWidth, Image.ActualHeight));
        }
        private void UpdateImageBackground(
            Size imageSize)
        {
            if (imageSize.Width > 0 && imageSize.Height > 0)
            {
                ImageBackgroundBorder.Background = ImageLoadedBackground;

                return;
            }

            ImageBackgroundBorder.Background = ImageUnloadedBackground;
        }



        private void Image_SizeChanged(object sender,
            SizeChangedEventArgs e)
        {
            UpdateImageBackground(e.NewSize);
        }
    }
}
