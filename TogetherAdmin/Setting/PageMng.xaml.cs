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
    public partial class PageMng : UserControl
    {
        ContentM2List contentM2List;
        ContentM2Login contentM2Login;
        MainWindow mw;

        public PageMng(string pageName, MainWindow mw)
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
                case "운영진 목록":
                    lbAdmin.Visibility = Visibility.Visible;
                    lbLoginList.Visibility = Visibility.Hidden;

                    if (contentM2List == null) { this.contentM2List = new ContentM2List(mw); }
                    else { contentM2List.Restart(); }

                    this.contentControl.DataContext = contentM2List;
                    break;
                case "로그인 내역":
                    lbAdmin.Visibility = Visibility.Hidden;
                    lbLoginList.Visibility = Visibility.Visible;

                    if (contentM2Login == null) { this.contentM2Login = new ContentM2Login(); }
                    else { contentM2Login.Restart(); }

                    contentControl.DataContext = contentM2Login;
                    break;
            }
        }
    }
}
