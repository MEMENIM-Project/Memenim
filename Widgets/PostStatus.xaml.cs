using System;
using System.Windows;
using System.Windows.Controls;
using Memenim.Converters;

namespace Memenim.Widgets
{
    public partial class PostStatus : UserControl
    {
        public static readonly DependencyProperty StatusValueProperty =
                DependencyProperty.Register(nameof(StatusValue), typeof(PostStatusType), typeof(PostStatus),
                    new PropertyMetadata(PostStatusType.Premoderating));

        public PostStatusType StatusValue
        {
            get
            {
                return (PostStatusType)GetValue(StatusValueProperty);
            }
            set
            {
                SetValue(StatusValueProperty, value);
            }
        }

        public PostStatus()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
