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
            await ApiHelper.SendComment(PostID, txtContent.Text, bAnon.IsChecked);

            MessageBox.Show("Comment sent");

            txtContent.Text = "";
            bAnon.IsChecked = false;
        }
    }
}
