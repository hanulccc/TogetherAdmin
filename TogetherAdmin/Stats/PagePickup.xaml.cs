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
using SangAdmin.Stats.Pickup;

namespace SangAdmin.Stats
{
    /// <summary>
    /// ViewMatching.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PagePickup : UserControl
    {
        ContentDay contentDay;
        ContentTime contentTime;
        ContentArea contentArea;
        ContentFee contentFee;
        ContentCancel contentCancel; 

        public PagePickup(string pageName)
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
                    lb1.Visibility = Visibility.Visible;
                    lb2.Visibility = Visibility.Hidden;
                    lb3.Visibility = Visibility.Hidden;
                    lb4.Visibility = Visibility.Hidden;
                    lb5.Visibility = Visibility.Hidden;

                    if (contentDay == null) { this.contentDay = new ContentDay(); }
                    else { contentDay.Restart(); }

                    this.contentControl.DataContext = contentDay;
                    break;
                case "시간대별 통계":
                    lb1.Visibility = Visibility.Hidden;
                    lb2.Visibility = Visibility.Visible;
                    lb3.Visibility = Visibility.Hidden;
                    lb4.Visibility = Visibility.Hidden;
                    lb5.Visibility = Visibility.Hidden;

                    if (contentTime == null) { this.contentTime = new ContentTime(); }
                    else { contentTime.Restart(); }
                    contentControl.DataContext = contentTime;
                    break;
                case "지역별 통계":
                    lb1.Visibility = Visibility.Hidden;
                    lb2.Visibility = Visibility.Hidden;
                    lb3.Visibility = Visibility.Visible;
                    lb4.Visibility = Visibility.Hidden;
                    lb5.Visibility = Visibility.Hidden;

                    if (contentArea == null) { this.contentArea = new ContentArea(); }
                    else { contentArea.Restart(); }

                    contentControl.DataContext = contentArea;
                    break;
                case "요금별 통계":
                    lb1.Visibility = Visibility.Hidden;
                    lb2.Visibility = Visibility.Hidden;
                    lb3.Visibility = Visibility.Hidden;
                    lb4.Visibility = Visibility.Visible;
                    lb5.Visibility = Visibility.Hidden;

                    if (contentFee == null) { this.contentFee = new ContentFee(); }
                    else { contentFee.Restart(); }

                    contentControl.DataContext = contentFee;
                    break;
                case "취소 사유별 통계":
                    lb1.Visibility = Visibility.Hidden;
                    lb2.Visibility = Visibility.Hidden;
                    lb3.Visibility = Visibility.Hidden;
                    lb4.Visibility = Visibility.Hidden;
                    lb5.Visibility = Visibility.Visible;

                    if (contentCancel == null) { this.contentCancel = new ContentCancel(); }
                    else { contentCancel.Restart(); }

                    contentControl.DataContext = contentCancel;
                    break;
            }
        }
    }
}
