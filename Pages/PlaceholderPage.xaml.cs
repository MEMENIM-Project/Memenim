using System;
using System.Windows;
using Memenim.Pages.ViewModel;
using RIS.Randomizing;

namespace Memenim.Pages
{
    public partial class PlaceholderPage : PageContent
    {
        private static readonly FastSecureRandom RandomGenerator = new FastSecureRandom();
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
            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            var biasZone =
                int.MaxValue - (int.MaxValue % Smiles.Length) - 1;
            int smileIndex =
                (int)RandomGenerator.GetUInt32((uint)biasZone) % Smiles.Length;

            if (Smiles[smileIndex] != txtSmile.Text)
            {
                txtSmile.Text = Smiles[smileIndex];
                return;
            }

            if (smileIndex == 0)
            {
                ++smileIndex;
            }
            else if (smileIndex == Smiles.Length - 1)
            {
                --smileIndex;
            }
            else
            {
                if (Rand.Current.NextBoolean(0.5))
                    ++smileIndex;
                else
                    --smileIndex;
            }

            txtSmile.Text = Smiles[smileIndex];
        }
    }
}
