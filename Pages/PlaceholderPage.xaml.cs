using System;
using System.Windows;
using System.Windows.Controls;

namespace Memenim.Pages
{
    public partial class PlaceholderPage : UserControl
    {
        private static readonly Random Random = new Random();
        private static readonly string[] Smiles =
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
            txtSmile.Text = Smiles[Random.Next(0, Smiles.Length - 1)];
        }
    }
}
