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

namespace SangAdmin.VirtualAccnt
{
    /// <summary>
    /// ViewVirtualAccnt.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ViewVirtualAccnt : UserControl
    {
        ContentUserAccnt contentUser;
        ContentAccntList contentAccnt;
        ContentDepositList contentDeposit;
        MainWindow mw;

        public ViewVirtualAccnt(string pageName, MainWindow mw)
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
                case "가상계좌 발급 현황":
                    btnUserAccnt.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnAccntList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnDepositList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnUserAccnt.Tag = "choice";
                    btnAccntList.Tag = "unchoice";
                    btnDepositList.Tag = "unchoice";

                    if (contentUser == null) { this.contentUser = new ContentUserAccnt(this.mw); }
                    else { contentUser.Restart(); }

                    this.contentControl.DataContext = contentUser;
                    break;
                case "가상계좌 변동 현황":
                    btnUserAccnt.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnAccntList.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnDepositList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnUserAccnt.Tag = "unchoice";
                    btnAccntList.Tag = "choice";
                    btnDepositList.Tag = "unchoice";

                    if (contentAccnt == null) { this.contentAccnt = new ContentAccntList(); }
                    else { contentAccnt.Restart(); }

                    contentControl.DataContext = contentAccnt;
                    break;
                case "입출금 현황":
                    btnUserAccnt.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnAccntList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnDepositList.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnUserAccnt.Tag = "unchoice";
                    btnAccntList.Tag = "unchoice";
                    btnDepositList.Tag = "choice";

                    if (contentDeposit == null) { this.contentDeposit = new ContentDepositList(); }
                    else { contentDeposit.Restart(); }

                    this.contentControl.DataContext = contentDeposit;
                    break;
            }
        }
    }
}
