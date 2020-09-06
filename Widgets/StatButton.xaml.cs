using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.IconPacks;

namespace AnonymDesktopClient.Widgets
{
    /// <summary>
    /// Interaction logic for StatButton.xaml
    /// </summary>
    public partial class StatButton : UserControl
    {
        public PackIconModernKind IconKind { get; set; } = PackIconModernKind.Xbox;
        public int ButtonSize { get; set; } = 48;
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly RoutedEvent OnButtonClicked = EventManager.RegisterRoutedEvent("ButtonClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(StatButton));

        public static readonly DependencyProperty ValueProperty =
                                                    DependencyProperty.Register("Value", typeof(string), typeof(StatButton), new
                                                    PropertyMetadata("", new PropertyChangedCallback(OnSetValueChanged)));

        // expose our event
        public event RoutedEventHandler ButtonClick
        {
            add { AddHandler(OnButtonClicked, value); }
            remove { RemoveHandler(OnButtonClicked, value); }
        }


        public StatButton()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(OnButtonClicked);
            RaiseEvent(newEventArgs);
        }

        private static void OnSetValueChanged(DependencyObject d,  DependencyPropertyChangedEventArgs e)
        {
            StatButton UserControl1Control = d as StatButton;
            UserControl1Control.OnSetValueChanged(e);
        }


        private void OnSetValueChanged(DependencyPropertyChangedEventArgs e)
        {
            tbValue.Text = e.NewValue.ToString();
        }

    }
}
