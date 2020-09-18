using AnonymDesktopClient.Core;
using Memenim.Core;
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

namespace AnonymDesktopClient
{
    /// <summary>
    /// Interaction logic for WriteComment.xaml
    /// </summary>
    public partial class WriteComment : UserControl
    {
        public WriteComment()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public int PostID { get; set; }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var res = await PostAPI.SendComment(PostID, txtContent.Text, bAnon.IsChecked, AppPersistent.UserToken);
            if (!res.error)
            {
                DialogManager.ShowDialog("S U C C", "Comment sent");
            }
            else
            {
                DialogManager.ShowDialog("Not S U C C", res.message);
            }

            txtContent.Text = "";
            bAnon.IsChecked = false;
        }
    }
}
