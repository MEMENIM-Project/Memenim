using System;
using System.Windows;
using System.Windows.Controls;

namespace Memenim.Pages
{
    /// <summary>
    /// Interaction logic for PlaceholderPage.xaml
    /// </summary>
    public partial class PlaceholderPage : PageContent
    {
        private string[] SmilesPool = new string[]
        {
            "(ﾟдﾟ；)",
            "(ó﹏ò｡)",
            "(´ω｀*)",
            "(┛ಠДಠ)┛彡┻━┻",
            "(* _ω_)…"
        };

        public PlaceholderPage() : base()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected override void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);
            Random rnd = new Random();
            int idx = rnd.Next(0, SmilesPool.Length - 1);
            txtSmile.Text = SmilesPool[idx];
        }
    }
}
