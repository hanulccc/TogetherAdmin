using LiveCharts;
using SangAdmin.Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
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
using SangAdmin.Other;
using Excel = Microsoft.Office.Interop.Excel;

namespace SangAdmin.Stats.Pickup
{
    /// <summary>
    /// ContentMPickup.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentCancel : UserControl
    {
        BasePage _page = new BasePage();
        public ChartValues<double> cnt_value { get; set; }
        public string[] label { get; set; }

        public ContentCancel()
        {
            InitializeComponent();
            SetDefault();

            if (Query() == false) return;
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;
        }

        public void Restart()
        {
            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;
        }
        #endregion

        private bool Query()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                // chart
                cnt_value = new ChartValues<double> { };
                label = new string[] { };
                this.DataContext = null;

                string yoil = "";
                string fr_date = "";
                string to_date = "";
                int i = 0;

                if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }
                if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }


                JObject jObj = Api.GetResponseJObject(Api.statPick_url + "?proc_type=50&start_date=" + fr_date + "&end_date=" + to_date + "&yoil=" + yoil);

                if (jObj == null) return false;

                if (jObj["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObj["resultMsg"].ToString());
                    return false;
                }
                if (jObj["resultData"].ToString() == "") return false;

                JArray jAry = JArray.Parse(jObj["resultData"].ToString());
                string[] la = new string[jAry.Count];

                foreach (JObject jItem in jAry)
                {
                    StatData d = new StatData();

                    d.stat_cancel = jItem["stat_cancel"].ToString();
                    d.stat_cnt = jItem["stat_cnt"].ToString();

                    cnt_value.Add(double.Parse(d.stat_cnt));

                    la[i] = d.stat_cancel;

                    i++;
                }

                label = la;
                this.DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show("통계 데이터 조회 중 오류 : " + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine("통계 데이터 조회 중 오류 : " + ex);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }

            return true;
        }

        private void txtDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtFrDate.Text != "" && txtToDate.Text != "")
            {
                Query();
            }
        }
    }
}
