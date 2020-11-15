using Client.ClientService;
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
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для RateCrosswordModal.xaml
    /// </summary>
    public partial class RateCrosswordModal : Window
    {
        private int lastStarIndex = 0;
        private long crosswordId;
        public RateCrosswordModal(long crosswordId)
        {
            InitializeComponent();
            this.crosswordId = crosswordId;
        }

        private void CloseModal_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MouseEnterAtStar(object sender, MouseEventArgs e)
        {
            string uri = @"E:\labs\5sem\OSISP\DBService\Client\Images\fiilledStar.png";
            Image img = (Image)sender;
            int starsCount = Int32.Parse(img.Name.Substring(4, 1));
            int start = 0;
            int end = starsCount;
            if (starsCount < lastStarIndex)
            {
                start = starsCount;
                end = 5;
                uri = @"E:\labs\5sem\OSISP\DBService\Client\Images\emptyStar.png";
            }
            for (int i = start; i < end; i++)
            {
                Image tmp = (Image)FindName("star" + (i + 1));
                tmp.Source = new BitmapImage(new Uri(uri));
            }
            lastStarIndex = starsCount;
        }

        private void RateCrosswordBtnClick(object sender, MouseButtonEventArgs e)
        {
            DBConnectionClient client = new DBConnectionClient("BasicHttpBinding_IDBConnection");
            client.updateRathing(crosswordId, lastStarIndex);
            for(int i = 0; i < 5; i++)
            {
                Image img = (Image)FindName("star" + (i + 1));
                img.Visibility = Visibility.Collapsed;
            }
            proposal.Visibility = Visibility.Collapsed;
            thanksForFeedback.Visibility = Visibility.Visible;
            done.Visibility = Visibility.Visible;
            client.Close();
        }
    }
}
