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

namespace SangAdmin.Setting
{
    /// <summary>
    /// ViewMatching.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PageMsg : UserControl
    {
        ContentMMsgList contentMMsgList;
        ContentMMsg contentMMsg;
        ContentMNotiCall contentMNotiCall;
        MainWindow mw;
        public PageMsg(string pageName, MainWindow mw)
        {
            InitializeComponent();

            this.mw = mw;

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
                case "발송내역":
                    lbMsgList.Visibility = Visibility.Visible;
                    lbMsg.Visibility = Visibility.Hidden;
                    lbNotiCall.Visibility = Visibility.Hidden;

                    if (contentMMsgList == null) { this.contentMMsgList = new ContentMMsgList(this.mw); }
                    this.contentControl.DataContext = contentMMsgList;
                    break;
                case "발송설정":
                    lbMsgList.Visibility = Visibility.Hidden;
                    lbMsg.Visibility = Visibility.Visible;
                    lbNotiCall.Visibility = Visibility.Hidden;

                    if (contentMMsg == null) { this.contentMMsg = new ContentMMsg(this.mw); }
                    this.contentControl.DataContext = contentMMsg;
                    break;
                case "안내콜":
                    lbMsgList.Visibility = Visibility.Hidden;
                    lbMsg.Visibility = Visibility.Hidden;
                    lbNotiCall.Visibility = Visibility.Visible;

                    if (contentMNotiCall == null) { this.contentMNotiCall = new ContentMNotiCall(this.mw); }
                    contentControl.DataContext = contentMNotiCall;
                    break;
            }
        }
    }
}
