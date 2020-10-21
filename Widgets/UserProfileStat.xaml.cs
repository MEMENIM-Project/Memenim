﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Memenim.Widgets
{
    public partial class UserProfileStat : UserControl
    {
        public static readonly DependencyProperty StatValueProperty =
            DependencyProperty.Register("StatValue", typeof(string), typeof(UserProfileStat), new PropertyMetadata(string.Empty));

        public string StatTitle { get; set; }
        public string StatValue
        {
            get
            {
                return (string)GetValue(StatValueProperty);
            }
            set
            {
                SetValue(StatValueProperty, value);
            }
        }

        public UserProfileStat()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
