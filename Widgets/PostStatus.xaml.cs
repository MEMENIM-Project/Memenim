using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Memenim.Widgets
{
    
    public enum EPostStatus
    {
        Premoderating = 0,
        Published = 1,
        Rejected = 2
    }

    public partial class PostStatus : UserControl
    {
        public static readonly DependencyProperty StatusValueProperty =
                DependencyProperty.Register(nameof(StatusValue), typeof(EPostStatus), typeof(PostStatus),
                    new PropertyMetadata(EPostStatus.Premoderating));


        public EPostStatus StatusValue 
        { 
            get
            {
                return (EPostStatus)GetValue(StatusValueProperty);
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
