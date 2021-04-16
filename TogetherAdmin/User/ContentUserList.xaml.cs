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
using Excel = Microsoft.Office.Interop.Excel;

namespace SangAdmin.User
{
    /// <summary>
    /// ContentUserList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentUserList : UserControl
    {
        BasePage _page = new BasePage();
        MainWindow mw;

        public ContentUserList(MainWindow mw)
        {
            InitializeComponent();

            this.mw = mw;

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            List<ComboBoxPairs> ComboItemlist = new List<ComboBoxPairs>();
            ComboItemlist.Add(new ComboBoxPairs("전체", ""));
            ComboItemlist.Add(new ComboBoxPairs("회원", "0"));
            ComboItemlist.Add(new ComboBoxPairs("탈퇴회원", "1"));

            cboLeaveYn.Items.Clear();
            cboLeaveYn.SelectedValuePath = "Value";
            cboLeaveYn.DisplayMemberPath = "Name";
            cboLeaveYn.ItemsSource = ComboItemlist;
            cboLeaveYn.SelectedIndex = 0;

            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;

            Sign();
        }

        public void Restart()
        {
            cboLeaveYn.SelectedIndex = 0;
            cboSearch.SelectedIndex = 0;

            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;

            txtBohumDate.Text = "";
            txtSrch.Text = "";

            Sign();
            typeChange("전체");
        }

        private void Sign()
        {
            try
            {
                string data = string.Format("proc_type={0}", "140");
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show("회원현황판 조회 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                txtTodayNew.Text = jObject["resultData"][0]["today_new"].ToString();
                txtTodayDriver.Text = jObject["resultData"][0]["today_driver"].ToString();
                txtTodayShelper.Text = jObject["resultData"][0]["today_helper"].ToString();
                //txtTotalCnt.Text = jObject["resultData"][0]["total_cnt"].ToString();
                txtShelperCnt.Text = jObject["resultData"][0]["helper_cnt"].ToString();
                txtDriverCnt.Text = jObject["resultData"][0]["driver_cnt"].ToString();
                txtBohumEnd.Text = jObject["resultData"][0]["bohum_end"].ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show("회원현황판 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
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
                string UserType = "";
                string SrchUser = "";
                string SrchHp = "";

                ComboBoxPairs ComboItem = (ComboBoxPairs)cboLeaveYn.SelectedItem;
                string leaveYn = ComboItem.Value;

                if (lbTypeDriver.Visibility == Visibility.Visible) { UserType = "드라이버"; }
                else if (lbTypeShelper.Visibility == Visibility.Visible) { UserType = "쉘퍼"; }

                if (cboSearch.SelectedIndex == 0) { SrchUser = txtSrch.Text; }
                else { SrchHp = txtSrch.Text; }

                if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }
                if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }

                string data = string.Format("proc_type={0}&usertype={1}&frdate={2}&todate={3}&srchuser={4}&srchhp={5}&bohum_date={6}&leaveyn={7}", "10", UserType, fr_date, to_date, SrchUser, SrchHp, txtBohumDate.Text, leaveYn);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable userTable = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlGrid.ItemsSource = userTable.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("회원현황 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                ctlGrid.Cursor = Cursors.Arrow;
            }

            return true;
        }
        #endregion


        private void btnType_Click(object sender, RoutedEventArgs e)
        {
            // 검색 - 사용자 타입 변경

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

        private void query_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Query();
        }

        private void btnDateChange_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            DateTime dt = DateTime.Now.Date;

            switch (btn.Content.ToString())
            {
                case "전일":
                    txtFrDate.SelectedDate = dt.AddDays(-1);
                    txtToDate.SelectedDate = dt.AddDays(-1);
                    break;
                case "당일":
                    txtFrDate.SelectedDate = dt;
                    txtToDate.SelectedDate = dt;
                    break;
                case "일주일":
                    txtFrDate.SelectedDate = dt.AddDays(-7);
                    txtToDate.SelectedDate = dt;
                    break;
                case "한달":
                    txtFrDate.SelectedDate = dt.AddDays(-30);
                    txtToDate.SelectedDate = dt;
                    break;
            }

            Query();
        }

        private void txtSrch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { Query(); }
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
                sfdlg.FileName = "사용자_목록.xls";

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

                ws.Cells[i, 1] = "'승인여부";
                ws.Cells[i, 2] = "'회원구분";
                ws.Cells[i, 3] = "'회원ID";
                ws.Cells[i, 4] = "'성명";
                ws.Cells[i, 5] = "'핸드폰번호";
                ws.Cells[i, 6] = "'가상번호";
                ws.Cells[i, 7] = "'소속사";
                ws.Cells[i, 8] = "'차량소지여부";
                ws.Cells[i, 9] = "'대리기사경력";
                ws.Cells[i, 10] = "'활동지역(도)";
                ws.Cells[i, 11] = "'활동지역(시/구)";
                ws.Cells[i, 12] = "'약관동의일";
                ws.Cells[i, 13] = "'탈퇴여부";
                ws.Cells[i, 14] = "'탈퇴일";
                ws.Cells[i, 15] = "'탈퇴사유";
                ws.Cells[i, 16] = "'차량번호";
                ws.Cells[i, 17] = "'제조사";
                ws.Cells[i, 18] = "'모델명";
                ws.Cells[i, 19] = "'탑승인원";
                ws.Cells[i, 20] = "'차량명의";
                ws.Cells[i, 21] = "'가상계좌";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["user_status"].ToString();
                    ws.Cells[i, 2] = "'" + row["user_type"].ToString();
                    ws.Cells[i, 3] = "'" + row["user_id"].ToString();
                    ws.Cells[i, 4] = "'" + row["user_name"].ToString();
                    ws.Cells[i, 5] = "'" + row["user_ph"].ToString();
                    ws.Cells[i, 6] = "'" + row["user_vph"].ToString();
                    ws.Cells[i, 7] = "'" + row["agent"].ToString();
                    ws.Cells[i, 8] = "'" + row["have_car"].ToString();
                    ws.Cells[i, 9] = "'" + row["career"].ToString();
                    ws.Cells[i, 10] = "'" + row["active_area1"].ToString();
                    ws.Cells[i, 11] = "'" + row["active_area2"].ToString();
                    ws.Cells[i, 12] = "'" + row["terms_auth_dt"].ToString();
                    ws.Cells[i, 13] = "'" + row["leave_yn"].ToString();
                    ws.Cells[i, 14] = "'" + row["leave_date"].ToString();
                    ws.Cells[i, 15] = "'" + row["leave_memo"].ToString();
                    ws.Cells[i, 16] = "'" + row["car_num"].ToString();
                    ws.Cells[i, 17] = "'" + row["car_brand"].ToString();
                    ws.Cells[i, 18] = "'" + row["car_model"].ToString();
                    ws.Cells[i, 19] = "'" + row["car_max"].ToString();
                    ws.Cells[i, 20] = "'" + row["car_owner"].ToString();
                    ws.Cells[i, 21] = "'" + row["account"].ToString();

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

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            Query();
        }

        private void ctlGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.mw.borBackground.Visibility = Visibility.Visible;

                DataGrid dg = (DataGrid)sender;

                DataRowView row = dg.SelectedItem as DataRowView;
                if (row != null)
                {
                    string strUserId = row["user_id"].ToString();

                    Window child = new DlgUserDetail(strUserId, mw);

                    child.Owner = Application.Current.MainWindow;

                    child.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                    child.ShowDialog();

                    //Dialog화면에서 데이터 변경이 있는경우 다시 조회처리
                    if (child.DialogResult.HasValue && child.DialogResult.Value)
                    {
                        Query();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.mw.borBackground.Visibility = Visibility.Collapsed;
            }
        }

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            DlgTestView testView = new DlgTestView();
            testView.ShowDialog();
        }
    }
}
