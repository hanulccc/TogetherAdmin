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

namespace SangAdmin.Setting
{
    /// <summary>
    /// ContentMPickup.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentM2Login : UserControl
    {
        BasePage _page = new BasePage();

        public ContentM2Login()
        {
            InitializeComponent();
            SetDefault();
            Query();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;
        }

        public void Restart()
        {
            txtSrch.Text = "";

            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;
        }
        #endregion

        #region [ 조회 ]
        private void Query()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string fr_date = "";
                string to_date = "";


                if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }
                if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }

                JObject jObject = Api.GetResponseJObject(Api.adminInfo_url + "?proc_type=20&search_text=" + txtSrch.Text + "&start_date=" + fr_date + "&end_date=" + to_date);

                if (jObject == null || jObject["resultCode"] == null) return;

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObject["resultMsg"].ToString());
                    return;
                }
                if (jObject["resultData"].ToString() == "") return;

                JArray jAry = JArray.Parse(jObject["resultData"].ToString());

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable dt = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("회원현황 조회 중 오류발생: " + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        #endregion

        private void txtSrch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { Query(); }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Query();
        }

        private void query_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Query();
        }
    }
}