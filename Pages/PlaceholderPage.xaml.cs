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

namespace AnonymDesktopClient.Core.Pages
{
    /// <summary>
    /// Interaction logic for PlaceholderPage.xaml
    /// </summary>
    public partial class PlaceholderPage : UserControl
    {
        private string[] SmilesPool = new string[]
        {
            "(ﾟдﾟ；)",
            "(ó﹏ò｡)",
            "(´ω｀*)",
            "(┛ಠДಠ)┛彡┻━┻",
            "(* _ω_)…"
        };

        public PlaceholderPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();
            int idx = rnd.Next(0, SmilesPool.Length - 1);
            txtSmile.Text = SmilesPool[idx];
        }
    }
}
