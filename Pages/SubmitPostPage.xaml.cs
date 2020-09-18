using AnonymDesktopClient.Core;
using Memenim.Core;
using Memenim.Core.Data;
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

namespace AnonymDesktopClient.Pages
{
    /// <summary>
    /// Interaction logic for SubmitPostPage.xaml
    /// </summary>
    public partial class SubmitPostPage : UserControl
    {
        private PostData m_PostData;

        public SubmitPostPage()
        {
            InitializeComponent();
            m_PostData = new PostData();
            m_PostData.attachments = new List<AttachmentData>();
            m_PostData.attachments.Add(new AttachmentData());
            m_PostData.attachments[0].photo = new PhotoData() { photo_big = "", photo_medium = "", photo_small = "",
                size = new SizeData()
                { 
                    photo_big = new RectData(), 
                    photo_medium = new RectData(), 
                    photo_small= new RectData() 
                } };
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        { 
            DataContext = m_PostData;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                m_PostData.author_watch++;
                var res = await PostAPI.SubmitPost(m_PostData, AppPersistent.UserToken);
                if (!res.error)
                {
                    DialogManager.ShowDialog("S U C C", "Post submitted. Get a tea and wait");
                }
            }
            catch (Exception ex)
            {
                DialogManager.ShowDialog("Some rtarded shit happened", ex.Message);
            }

        }
    }
}
