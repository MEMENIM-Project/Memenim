using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace Memenim.Widgets
{
    /// <summary>
    /// Interaction logic for StatButton.xaml
    /// </summary>
    public partial class StatButton : UserControl
    {
        public PackIconModernKind Icon { get; set; } = PackIconModernKind.Xbox;
        public int ButtonSize { get; set; } = 48;
        public string StatValue { get; set; } = "000";

        public static readonly RoutedEvent OnButtonClicked = EventManager.RegisterRoutedEvent("ButtonClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(StatButton));


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

        private void StackPanel_Loaded(object sender, RoutedEventArgs e)
        {
            tbValue.Text = StatValue;
        }
    }
}
