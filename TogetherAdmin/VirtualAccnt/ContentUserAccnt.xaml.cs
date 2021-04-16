using SangAdmin.Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using Excel = Microsoft.Office.Interop.Excel;

namespace SangAdmin.VirtualAccnt
{
    /// <summary>
    /// ContentUserAccnt.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentUserAccnt : UserControl
    {
        BasePage _page = new BasePage();
        MainWindow mw;

        public ContentUserAccnt(MainWindow mw)
        {
            InitializeComponent();

            this.mw = mw;

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
            cboSearch.SelectedIndex = 0;

            txtSrch.Text = "";
            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;

            typeChange("전체");
        }
        #endregion


        #region [ 조회 ]
        private bool Query()
        {
            try
            {
                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string fr_date = "";
                string to_date = "";
                string userType = "";
                string SrchText = "";
                string SrchType = "name";

                if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }
                if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }

                if (lbTypeDriver.Visibility == Visibility.Visible) { userType = "드라이버"; }
                else if (lbTypeShelper.Visibility == Visibility.Visible) { userType = "쉘퍼"; }

                SrchText = txtSrch.Text;

                if (cboSearch.SelectedIndex == 1) { SrchType = "phone"; }

                string data = string.Format("proc_type={0}&type={1}&start_date={2}&end_date={3}&srch_text={4}&srch_type={5}", "10", userType, fr_date, to_date, SrchText, SrchType);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/vraccnt", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable userTable = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());
                    ctlGrid.ItemsSource = userTable.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("가상계좌부여 현황 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                ctlGrid.Cursor = Cursors.Arrow;
            }

            return true;
        }
        #endregion

        private void query_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Query();
        }

        private void btnType_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            typeChange(btn.Content.ToString());
        }

        private void typeChange(string type)
        {
            switch (type)
            {
                case "전체":
                    lbTypeAll.Visibility = Visibility.Visible;
                    lbTypeDriver.Visibility = Visibility.Hidden;
                    lbTypeShelper.Visibility = Visibility.Hidden;
                    break;

                case "드라이버":
                    lbTypeAll.Visibility = Visibility.Hidden;
                    lbTypeDriver.Visibility = Visibility.Visible;
                    lbTypeShelper.Visibility = Visibility.Hidden;
                    break;

                case "쉘퍼":
                    lbTypeAll.Visibility = Visibility.Hidden;
                    lbTypeDriver.Visibility = Visibility.Hidden;
                    lbTypeShelper.Visibility = Visibility.Visible;
                    break;
            }

            Query();
        }

        private void txtSrch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { Query(); }
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            Query();
        }

        private void btnVirtAccnt_Click(object sender, RoutedEventArgs e)
        {
            this.mw.borBackground.Visibility = Visibility.Visible;

            Window child = new DlgAccntInfo(this.mw);
            
            child.Owner = Application.Current.MainWindow;
            child.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            child.ShowDialog();

            this.mw.borBackground.Visibility = Visibility.Collapsed;
        }

        private void btnExcelDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //파일 저장경로 받기
                SaveFileDialog sfdlg = new SaveFileDialog();
                Excel.Application excelApp = null;

                sfdlg.CreatePrompt = true;
                sfdlg.OverwritePrompt = true;
                sfdlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                sfdlg.Filter = "모든엑셀(excel) 파일 | *.xls;*.xlsx;";
                sfdlg.FileName = "가상계좌발급현황_목록.xls";

                if (!(bool)sfdlg.ShowDialog()) { return; }

                Mouse.OverrideCursor = Cursors.Wait;

                //파일 있다면 제거
                FileInfo excelFile = new FileInfo(sfdlg.FileName);
                if (excelFile.Exists) { excelFile.Delete(); }

                // 첫번째 워크시트 가져오기
                excelApp = new Excel.Application();
                Excel.Workbook wb = excelApp.Workbooks.Add(true);
                Excel._Worksheet ws = wb.Worksheets.get_Item(1) as Excel._Worksheet;

                int i = 1;

                ws.Cells[i, 1] = "'회원구분";
                ws.Cells[i, 2] = "'회원ID";
                ws.Cells[i, 3] = "'성명";
                ws.Cells[i, 4] = "'핸드폰번호";
                ws.Cells[i, 5] = "'소속사";
                ws.Cells[i, 6] = "'차량소지여부";
                ws.Cells[i, 7] = "'약관동의일";
                ws.Cells[i, 8] = "'은행명";
                ws.Cells[i, 9] = "'가상계좌번호";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["user_type"].ToString();
                    ws.Cells[i, 2] = "'" + row["user_id"].ToString();
                    ws.Cells[i, 3] = "'" + row["user_name"].ToString();
                    ws.Cells[i, 4] = "'" + row["user_ph"].ToString();
                    ws.Cells[i, 5] = "'" + row["agent"].ToString();
                    ws.Cells[i, 6] = "'" + row["have_car"].ToString();
                    ws.Cells[i, 7] = "'" + row["terms_auth_dt"].ToString();
                    ws.Cells[i, 8] = "'" + row["account_bank"].ToString();
                    ws.Cells[i, 9] = "'" + row["account"].ToString();
                    i++;
                }

                // 엑셀파일 저장
                wb.SaveAs(sfdlg.FileName, Excel.XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, Type.Missing
                    , Type.Missing, Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing
                    , Type.Missing, Type.Missing);

                wb.Close(true);

                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                excelApp = null;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(ws);
                ws = null;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(wb);
                wb = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류발생: " + ex.Message);
                Console.WriteLine("오류: " + ex);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        private void btnWithdrawPop_Click(object sender, RoutedEventArgs e)
        {
            bdWithdraw.Visibility = Visibility.Visible;
        }

        private void btnWdClose_Click(object sender, RoutedEventArgs e)
        {
            bdWithdraw.Visibility = Visibility.Collapsed;
        }

        private void btnWithdraw_Click(object sender, RoutedEventArgs e)
        {
            string url = "http://114.207.112.42:5455/withdraw";
            DataRowView row = ctlGrid.SelectedItem as DataRowView;
            ComboBoxItem cbi = cboBank.SelectedItem as ComboBoxItem;
            NameValueCollection nv = new NameValueCollection();

            if (cbi == null || cbi.Tag == "all")
            {
                MessageBox.Show("은행을 선택하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (txtAccount.Text == "")
            {
                MessageBox.Show("출금 계좌를 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (txtPrice.Text == "")
            {
                MessageBox.Show("출금 금액를 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (row == null)
            {
                MessageBox.Show("오류가 발생되었습니다.\n다시 시작해주세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            nv.Add("account", txtAccount.Text);
            nv.Add("program_code", "together");
            nv.Add("user_id", row["user_id"].ToString());
            nv.Add("bank_code", cbi.Tag.ToString());
            nv.Add("user_name", row["user_name"].ToString());
            nv.Add("memo", "수동출금");
            nv.Add("price", txtPrice.Text);

            JObject jObj = Api.PostResponseJObject(url, nv);

            if (jObj == null) return;

            if (jObj["resultCode"].ToString() != "200")
            {
                MessageBox.Show(jObj["resultMsg"].ToString());
                return;
            }

            MessageBox.Show("출금되었습니다.");
            bdWithdraw.Visibility = Visibility.Collapsed;
        }
    }
}
