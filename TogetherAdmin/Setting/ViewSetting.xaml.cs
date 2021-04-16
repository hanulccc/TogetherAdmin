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
    public partial class ViewSetting: UserControl
    {
        PageUser pageUser;
        PageMsg pageMsg;
        PageMng pageMng;
        MainWindow mw;

        public ViewSetting(string pageName, MainWindow mw)
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

            //if (btn.Tag.ToString() == "choice") { return; }

            setPage(btn.Content.ToString());
        }

        public void setPage(string pageName)
        {
            switch (pageName)
            {
                case "고객지원":
                    btnContentMLocation.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnContentMPickup.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnContentMTaxi.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnContentMLocation.Tag = "choice";
                    btnContentMPickup.Tag = "unchoice";
                    btnContentMTaxi.Tag = "unchoice";

                    if (pageUser == null) { this.pageUser = new PageUser("공지사항", this.mw); }

                    this.contentControl.DataContext = pageUser;
                    break;
                case "운영진 관리":
                    btnContentMLocation.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnContentMPickup.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnContentMTaxi.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnContentMLocation.Tag = "unchoice";
                    btnContentMPickup.Tag = "choice";
                    btnContentMTaxi.Tag = "unchoice";

                    if (pageMng == null) { this.pageMng = new PageMng("운영진 목록", this.mw); }

                    contentControl.DataContext = pageMng;
                    break;
                case "메세지 관리":
                    btnContentMLocation.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnContentMPickup.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnContentMTaxi.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnContentMLocation.Tag = "unchoice";
                    btnContentMPickup.Tag = "unchoice";
                    btnContentMTaxi.Tag = "choice";

                    if (pageMsg == null) { this.pageMsg = new PageMsg("발송내역", this.mw); }

                    this.contentControl.DataContext = pageMsg;
                    break;
            }
        }
    }
}
