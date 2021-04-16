using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SangAdmin.Stats.Access;

namespace SangAdmin.Stats
{
    /// <summary>
    /// ViewMatching.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PageAccess : UserControl
    {
        ContentDay contentDay;
        ContentTime contentTime;

        public PageAccess(string pageName)
        {
            InitializeComponent();
            SetDefault(pageName);
        }

        #region [ 초기값설정 ]
        private void SetDefault(string pageName)
        {
            setPage(pageName);
        }
        #endregion

        private void btnChangePage_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            setPage(btn.Content.ToString());
        }

        public void setPage(string pageName)
        {
            switch (pageName)
            {
                case "일/주/월간 통계":
                    lbNotice.Visibility = Visibility.Visible;
                    lbFaq.Visibility = Visibility.Hidden;

                    if (contentDay == null) { this.contentDay = new ContentDay(); }
                    else { contentDay.Restart(); }

                    this.contentControl.DataContext = contentDay;
                    break;
                case "시간대별 통계":
                    lbNotice.Visibility = Visibility.Hidden;
                    lbFaq.Visibility = Visibility.Visible;

                    if (contentTime == null) { this.contentTime = new ContentTime(); }
                    else { contentTime.Restart(); }

                    contentControl.DataContext = contentTime;
                    break;
            }
        }
    }
}
