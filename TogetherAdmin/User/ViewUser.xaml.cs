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

namespace SangAdmin.User
{
    /// <summary>
    /// ViewUser.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ViewUser : UserControl
    {
        MainWindow mw;
        ContentUserList contentUser;
        ContentShelperList contentShelper;
        ContentVirtualNo contentVirtual;

        public ViewUser(string pageName, MainWindow mw)
        {
            InitializeComponent();

            this.mw = mw;

            setPage(pageName);
        }

        private void btnChangePage_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            //if (btn.Tag.ToString() == "choice") { return; }

            setPage(btn.Content.ToString());
        }

        public void setPage(string pageName)
        {
            switch (pageName)
            {
                case "사용자 목록":
                    btnUserList.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnShelperList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnVirtualNo.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnUserList.Tag = "choice";
                    btnShelperList.Tag = "unchoice";
                    btnVirtualNo.Tag = "unchoice";

                    if (contentUser == null) { this.contentUser = new ContentUserList(this.mw); }
                    else { contentUser.Restart(); }

                    this.contentControl.DataContext = contentUser;
                    break;
                case "쉘퍼 가입 관리":
                    btnUserList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnShelperList.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnVirtualNo.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnUserList.Tag = "unchoice";
                    btnShelperList.Tag = "choice";
                    btnVirtualNo.Tag = "unchoice";

                    if (contentShelper == null) { this.contentShelper = new ContentShelperList(this.mw); }
                    else { contentShelper.Restart(); }

                    contentControl.DataContext = contentShelper;
                    break;
                case "가상번호 현황":
                    btnUserList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnShelperList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnVirtualNo.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnUserList.Tag = "unchoice";
                    btnShelperList.Tag = "unchoice";
                    btnVirtualNo.Tag = "choice";

                    if (contentVirtual == null) { this.contentVirtual = new ContentVirtualNo(this.mw); }
                    else { contentVirtual.Restart(); }

                    this.contentControl.DataContext = contentVirtual;
                    break;
            }
        }
    }
}
