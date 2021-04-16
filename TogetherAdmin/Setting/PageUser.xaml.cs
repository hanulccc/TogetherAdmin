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
    public partial class PageUser: UserControl
    {
        ContentUNotice contentUNotice;
        ContentUFaq contentUFaq;
        ContentUQna contentUQna;
        MainWindow mw;

        public PageUser(string pageName, MainWindow mw)
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
                case "공지사항":
                    lbNotice.Visibility = Visibility.Visible;
                    lbFaq.Visibility = Visibility.Hidden;
                    lbQna.Visibility = Visibility.Hidden;

                    if (contentUNotice == null) { this.contentUNotice = new ContentUNotice(this.mw); }
                    else { contentUNotice.Restart(); }

                    this.contentControl.DataContext = contentUNotice;
                    break;
                case "자주 묻는 질문":
                    lbNotice.Visibility = Visibility.Hidden;
                    lbFaq.Visibility = Visibility.Visible;
                    lbQna.Visibility = Visibility.Hidden;

                    if (contentUFaq == null) { this.contentUFaq = new ContentUFaq(this.mw); }
                    else { contentUFaq.Restart(); }

                    this.contentControl.DataContext = contentUFaq;
                    break;
                case "묻고 답하기":
                    lbNotice.Visibility = Visibility.Hidden;
                    lbFaq.Visibility = Visibility.Hidden;
                    lbQna.Visibility = Visibility.Visible;

                    if (contentUQna == null) { this.contentUQna = new ContentUQna(this.mw); }
                    else { contentUQna.Restart(); }

                    this.contentControl.DataContext = contentUQna;
                    break;
            }
        }
    }
}
