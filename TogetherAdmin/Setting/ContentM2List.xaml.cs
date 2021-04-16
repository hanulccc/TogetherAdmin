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
    public partial class ContentM2List : UserControl
    {
        BasePage _page = new BasePage();
        MainWindow mw;

        public ContentM2List(MainWindow mw)
        {
            InitializeComponent();

            this.mw = mw;

            SetDefault();

            Query();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            if (mw.intAdPower > 0)
            {
                btnSave.Visibility = Visibility.Visible;
            }
        }

        public void Restart()
        {
            Query();
        }
        #endregion

        #region [ 조회 ]
        private void Query()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                JObject jObject = Api.GetResponseJObject(Api.adminInfo_url + "?proc_type=10");

                if (jObject == null) return;

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObject["resultMsg"].ToString());
                    return;
                }
                if (jObject["resultData"].ToString() == "") return;


                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable dt = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("회원현황 조회 중 오류발생: " + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine("회원현황 조회 중 오류발생: " + ex);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        #endregion

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.mw.borBackground.Visibility = Visibility.Visible;

            Window child = new DlgAdmin(null);
            child.Owner = Application.Current.MainWindow;
            child.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            child.ShowDialog();

            Query();
            this.mw.borBackground.Visibility = Visibility.Collapsed;
        }

        private void ctlGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.mw.borBackground.Visibility = Visibility.Visible;

                if (mw.intAdPower < 1) { MessageBox.Show("권한이 없습니다."); return; }

                DataRowView drv = ctlGrid.SelectedItem as DataRowView;

                if (drv == null) { return; }

                Window child = new DlgAdmin(drv);

                child.Owner = Application.Current.MainWindow;
                child.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                child.ShowDialog();

                Query();
            }
            finally
            {
                this.mw.borBackground.Visibility = Visibility.Collapsed;
            }
        }
    }
}
