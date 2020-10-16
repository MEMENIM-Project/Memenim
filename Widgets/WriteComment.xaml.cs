using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memenim.Utils;
using Memenim.Core.Api;

namespace Memenim.Widgets
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
            AnononymCommandPress = new BasicCommand(o => true, _ => { m_AnonSend = !m_AnonSend; });
            this.DataContext = this;
        }

        public int PostID { get; set; }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var res = await PostApi.AddComment(PostID, txtContent.Text, m_AnonSend, AppPersistent.UserToken);
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
