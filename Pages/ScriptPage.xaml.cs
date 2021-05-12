using System;
using System.Windows;
using Memenim.Pages.ViewModel;

namespace Memenim.Pages
{
    public partial class ScriptPage : PageContent
    {
        public ScriptViewModel ViewModel
        {
            get
            {
                return DataContext as ScriptViewModel;
            }
        }

        public ScriptPage()
        {
            InitializeComponent();
            DataContext = new ScriptViewModel();
        }

        protected override void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }
        }
    }
}
