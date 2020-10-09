using AnonymDesktopClient.Core;
using AnonymDesktopClient.Core.Utils;
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

namespace AnonymDesktopClient.Widgets
{
    /// <summary>
    /// Interaction logic for WriteComment.xaml
    /// </summary>
    public partial class WriteComment : UserControl
    {

        public string UserAvatarSource { get; set; }
        public ICommand AnononymCommandPress { get; set; }
        private bool m_AnonSend = false;


        public WriteComment()
        {
            InitializeComponent();
            AnononymCommandPress = new BasicCommand(o=> true, _ => { m_AnonSend = !m_AnonSend; });
            this.DataContext = this;
        }

        public int PostID { get; set; }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var res = await PostAPI.SendComment(PostID, txtContent.Text, m_AnonSend, AppPersistent.UserToken);
            if (!res.error)
            {
                DialogManager.ShowDialog("S U C C", "Comment sent");
            }
            else
            {
                DialogManager.ShowDialog("Not S U C C", res.message);
            }

            txtContent.Text = "";
            m_AnonSend = false;
        }
    }
}
