using System;
using System.Windows;
using Memenim.Pages.ViewModel;

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

        public PlaceholderViewModel ViewModel
        {
            get
            {
                return DataContext as PlaceholderViewModel;
            }
        }

        public PlaceholderPage()
        {
            InitializeComponent();
            DataContext = new PlaceholderViewModel();
        }

        protected override void OnEnter(object sender, RoutedEventArgs e)
        {
            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            base.OnEnter(sender, e);

            txtSmile.Text = Smiles[Random.Next(0, Smiles.Length - 1)];
        }
    }
}
