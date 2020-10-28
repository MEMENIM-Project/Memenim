using System;
using System.Windows;

namespace Memenim.Pages
{
    public partial class PlaceholderPage : PageContent
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

        protected override void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            txtSmile.Text = Smiles[Random.Next(0, Smiles.Length - 1)];
        }
    }
}
