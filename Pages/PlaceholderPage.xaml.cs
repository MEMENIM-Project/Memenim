using System;
using System.Windows;
using Memenim.Generating;
using Memenim.Pages.ViewModel;

namespace Memenim.Pages
{
    public partial class PlaceholderPage : PageContent
    {
        private static readonly string[] Smiles =
        {
            "(ﾟдﾟ；)",
            "(ó﹏ò｡)",
            "(´ω｀*)",
            "(┛ಠДಠ)┛彡┻━┻",
            "(* _ω_)…",
            "(ﾉ･д･)ﾉ",
            "(⊃｡•́‿•̀｡)⊃",
            "ლ(๏‿๏ ◝ლ)",
            "ლ(*꒪ヮ꒪*)ლ",
            "(ﾉ･ｪ･)ﾉ",
            "(＾▽＾)",
            "(•‿•)",
            "(☉_☉)",
            "(,,◕ ⋏ ◕,,)",
            "(๑❛ꇳ❛๑)",
            "(-, – )…zzzZZZ",
            "┬─┬ノ( º _ ºノ)",
            "(⌒‿⌒)",
            "\\ (•◡•) /",
            "⚆ _ ⚆",
            "(づ￣ ³￣)づ",
            "ಠ‿↼"
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

            txtSmile.Text = GeneratingManager.GetRandomSmile();
        }
    }
}
