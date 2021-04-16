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

namespace SangAdmin.Stats
{
    /// <summary>
    /// ViewUser.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ViewStats: UserControl
    {
        MainWindow mw;
        PagePickup pagePickup;
        PageTaxi pageTaxi;
        PageAccess pageAccess;
        PageUser pageUser;

        public ViewStats(string pageName, MainWindow mw)
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
                case "픽업 매칭 통계":
                    btnUserList.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnShelperList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnVirtualNo.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnVirtualNo2.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnUserList.Tag = "choice";
                    btnShelperList.Tag = "unchoice";
                    btnVirtualNo.Tag = "unchoice";

                    if (pagePickup == null) { this.pagePickup = new PagePickup("일/주/월간 통계"); }

                    this.contentControl.DataContext = pagePickup;
                    break;
                case "택시 동승 통계":
                    btnUserList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnShelperList.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnVirtualNo.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnVirtualNo2.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnUserList.Tag = "unchoice";
                    btnShelperList.Tag = "choice";
                    btnVirtualNo.Tag = "unchoice";

                    if (pageTaxi == null) { this.pageTaxi = new PageTaxi(); }
                    else { pageTaxi.Restart(); }

                    contentControl.DataContext = pageTaxi;
                    break;
                case "접속 통계":
                    btnUserList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnShelperList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnVirtualNo.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnVirtualNo2.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnUserList.Tag = "unchoice";
                    btnShelperList.Tag = "unchoice";
                    btnVirtualNo.Tag = "choice";

                    if (pageAccess == null) { this.pageAccess = new PageAccess("일/주/월간 통계"); }
                    this.contentControl.DataContext = pageAccess;
                    break;
                case "사용자 통계":
                    btnUserList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnShelperList.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnVirtualNo.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnVirtualNo2.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnUserList.Tag = "unchoice";
                    btnShelperList.Tag = "unchoice";
                    btnVirtualNo.Tag = "choice";

                    if (pageUser == null) { this.pageUser = new PageUser("일/주/월간 통계"); }

                    this.contentControl.DataContext = pageUser;
                    break;
            }
        }
    }
}
