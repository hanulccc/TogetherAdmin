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

namespace SangAdmin.Matching
{
    /// <summary>
    /// ViewMatching.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ViewMatching : UserControl
    {
        ContentMLocation contentMLocation;
        ContentMPickup contentMPickup;
        ContentMTaxi contentMTaxi;
        MainWindow mw;

        public ViewMatching(string pageName, MainWindow mw)
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
                case "사용자 위치 현황":
                    btnContentMLocation.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnContentMPickup.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnContentMTaxi.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnContentMLocation.Tag = "choice";
                    btnContentMPickup.Tag = "unchoice";
                    btnContentMTaxi.Tag = "unchoice";

                    if (this.contentMLocation == null) { this.contentMLocation = new ContentMLocation(this.mw); }
                    else { this.contentMLocation.Restart(); }

                    contentControl.DataContext = contentMLocation;
                    break;
                case "픽업 매칭 현황":
                    btnContentMLocation.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnContentMPickup.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnContentMTaxi.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnContentMLocation.Tag = "unchoice";
                    btnContentMPickup.Tag = "choice";
                    btnContentMTaxi.Tag = "unchoice";

                    if (this.contentMPickup == null) { this.contentMPickup = new ContentMPickup(this.mw); }
                    else { this.contentMPickup.Restart(); }

                    contentControl.DataContext = contentMPickup;
                    break;
                case "택시 동승 현황":
                    btnContentMLocation.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnContentMPickup.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnContentMTaxi.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnContentMLocation.Tag = "unchoice";
                    btnContentMPickup.Tag = "unchoice";
                    btnContentMTaxi.Tag = "choice";

                    if (contentMTaxi == null) { this.contentMTaxi = new ContentMTaxi(); }
                    else { this.contentMTaxi.Restart(); }

                    this.contentControl.DataContext = contentMTaxi;
                    break;
            }
        }
    }
}
