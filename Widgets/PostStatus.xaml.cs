using System;
using System.Windows;
using Memenim.Core.Schema;

namespace Memenim.Widgets
{
    public partial class PostStatus : WidgetContent
    {
        public static readonly DependencyProperty StatusValueProperty =
                DependencyProperty.Register(nameof(StatusValue), typeof(PostStatusType), typeof(PostStatus),
                    new PropertyMetadata(PostStatusType.Published));



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
