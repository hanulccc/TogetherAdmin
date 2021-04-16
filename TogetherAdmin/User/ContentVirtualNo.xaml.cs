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
    /// ContentVirtualNo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentVirtualNo : UserControl
    {
        BasePage _page = new BasePage();
        MainWindow mw;

        public ContentVirtualNo(MainWindow mw)
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
            ComboItemlist.Add(new ComboBoxPairs("아이디", "아이디"));
            ComboItemlist.Add(new ComboBoxPairs("유저 연락처", "유저 연락처"));
            ComboItemlist.Add(new ComboBoxPairs("가상번호", "가상번호"));

            cboSearch.Items.Clear();
            cboSearch.SelectedValuePath = "Value";
            cboSearch.DisplayMemberPath = "Name";
            cboSearch.ItemsSource = ComboItemlist;
            cboSearch.SelectedIndex = 0;

            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;

            Query();
        }

        public void Restart()
        {
            cboSearch.SelectedIndex = 0;
            cboCarrier.SelectedIndex = 0;

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
                if (ctlGridList.Visibility == Visibility.Collapsed)
                {
                    ctlGrid.Cursor = Cursors.Wait;
                    ctlGrid.ItemsSource = null;

                    string data = string.Format("program_code={0}&vr_num={1}", "together", txtSrch.Text);
                    string strResult = _page.HttpSendData("http://175.207.13.166:15454/list", "GET", data);

                    JObject jObject = JObject.Parse(strResult); //json 객체로

                    if (jObject["resultCode"].ToString() != "200")
                    {
                        MessageBox.Show("가상번호 현황 조회 오류" + strResult, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    DataTable VirtTable = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());


                    data = string.Format("proc_type={0}", "10");
                    strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/vrnum", "GET", data);

                    jObject = JObject.Parse(strResult); //json 객체로

                    DataTable UserTable = null;
                    if (jObject["resultCode"].ToString() == "200")
                    {
                        UserTable = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());
                    }

                    DataTable dtResult = new DataTable("VirtualNo");
                    DataColumn col = new DataColumn();
                    col.DataType = System.Type.GetType("System.String");
                    col.ColumnName = "vr_num";
                    dtResult.Columns.Add(col);

                    col = new DataColumn();
                    col.DataType = System.Type.GetType("System.String");
                    col.ColumnName = "conn_date";
                    dtResult.Columns.Add(col);

                    col = new DataColumn();
                    col.DataType = System.Type.GetType("System.String");
                    col.ColumnName = "user_id";
                    dtResult.Columns.Add(col);

                    col = new DataColumn();
                    col.DataType = System.Type.GetType("System.String");
                    col.ColumnName = "user_name";
                    dtResult.Columns.Add(col);

                    col = new DataColumn();
                    col.DataType = System.Type.GetType("System.String");
                    col.ColumnName = "user_ph";
                    dtResult.Columns.Add(col);

                    col = new DataColumn();
                    col.DataType = System.Type.GetType("System.String");
                    col.ColumnName = "user_type";
                    dtResult.Columns.Add(col);

                    col = new DataColumn();
                    col.DataType = System.Type.GetType("System.String");
                    col.ColumnName = "leave_yn";
                    dtResult.Columns.Add(col);

                    col = new DataColumn();
                    col.DataType = System.Type.GetType("System.String");
                    col.ColumnName = "leave_date";
                    dtResult.Columns.Add(col);

                    col = new DataColumn();
                    col.DataType = System.Type.GetType("System.String");
                    col.ColumnName = "button_name";
                    dtResult.Columns.Add(col);

                    var query = from a in VirtTable.AsEnumerable()
                                join b in UserTable.AsEnumerable()
                                  on a.Field<string>("user_id") equals b.Field<string>("user_id") into v
                                from leftJoin in v.DefaultIfEmpty()
                                orderby leftJoin == null ? "" : leftJoin.Field<string>("user_id") descending, a.Field<string>("vr_num")

                                select dtResult.LoadDataRow(new object[]
                                 {
                                a.Field<string>("vr_num"),
                                leftJoin == null ? "" : a.Field<string>("conn_date"),
                                leftJoin == null ? "" : leftJoin.Field<string>("user_id"),
                                leftJoin == null ? "" : leftJoin.Field<string>("user_name"),
                                leftJoin == null ? "" : leftJoin.Field<string>("user_ph"),
                                leftJoin == null ? "" : leftJoin.Field<string>("user_type"),
                                leftJoin == null ? "" : leftJoin.Field<string>("leave_yn"),
                                leftJoin == null ? "" : leftJoin.Field<string>("leave_date"),
                                leftJoin == null ? "할당" : "회수"
                                 }, false);

                    query.CopyToDataTable();

                    //var query = from a in VirtTable.AsEnumerable()
                    //            join b in UserTable.AsEnumerable()
                    //              on a.Field<string>("user_id") equals b.Field<string>("user_id") into v
                    //            from leftJoin in v.DefaultIfEmpty()
                    //            select new
                    //            {
                    //                vr_num = a.Field<string>("vr_num"),
                    //                conn_date = leftJoin == null ? "" : a.Field<string>("conn_date"),
                    //                user_id = leftJoin == null ? "" : leftJoin.Field<string>("user_id"),
                    //                user_name = leftJoin == null ? "" : leftJoin.Field<string>("user_name"),
                    //                user_ph = leftJoin == null ? "" : leftJoin.Field<string>("user_ph"),
                    //                user_type = leftJoin == null ? "" : leftJoin.Field<string>("user_type"),
                    //                leave_yn = leftJoin == null ? "" : leftJoin.Field<string>("leave_yn"),
                    //                leave_date = leftJoin == null ? "" : leftJoin.Field<string>("leave_date"),
                    //                button_name = leftJoin == null ? "할당" : "회수"
                    //            };

                    //foreach (var item in query)
                    //{
                    //    DataRow dr = dtResult.NewRow();

                    //    if (item.vr_num.ToString().Contains(txtSrchText.Text) || txtSrchText.Text == "")
                    //    {
                    //        dr["vr_num"] = item.vr_num.ToString();
                    //        dr["conn_date"] = item.conn_date.ToString();
                    //        dr["user_id"] = item.user_id.ToString();
                    //        dr["user_name"] = item.user_name.ToString();
                    //        dr["user_ph"] = item.user_ph.ToString();
                    //        dr["user_type"] = item.user_type.ToString();
                    //        dr["leave_yn"] = item.leave_yn.ToString();
                    //        dr["leave_date"] = item.leave_date.ToString();
                    //        dr["button_name"] = item.button_name.ToString();
                    //        dtResult.Rows.Add(dr);
                    //    }
                    //}

                    ctlGrid.ItemsSource = dtResult.DefaultView;
                }
                else
                {
                    ctlGridList.Cursor = Cursors.Wait;
                    ctlGridList.ItemsSource = null;

                    string fr_date = "";
                    string to_date = "";
                    string userType = "";
                    string SrchId = "";
                    string SrchPh = "";
                    string SrchVph = "";

                    if (lbTypeDriver.Visibility == Visibility.Visible) { userType = "드라이버"; }
                    else if (lbTypeShelper.Visibility == Visibility.Visible) { userType = "쉘퍼"; }

                    if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyyMMdd"); }
                    if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyyMMdd"); }

                    if (cboSearch.SelectedIndex == 0) { SrchId = txtSrch.Text; }
                    else if (cboSearch.SelectedIndex == 1) { SrchPh = txtSrch.Text; }
                    else { SrchVph = txtSrch.Text; }

                    string data = string.Format("proc_type={0}&start_date={1}&end_date={2}&user_type={3}&user_id={4}&user_ph={5}&user_vph={6}", "30", fr_date, to_date, userType, SrchId, SrchPh, SrchVph);
                    string strResult = _page.HttpSendData(_page.GetServerUrl+"/admin/vrnum", "GET", data);

                    JObject jObject = JObject.Parse(strResult); //json 객체로

                    if (jObject["resultCode"].ToString() == "200")
                    {
                        DataTable dt = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());
                        ctlGridList.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("가상번호 거래내역 데이터 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                ctlGrid.Cursor = Cursors.Arrow;
                ctlGridList.Cursor = Cursors.Arrow;
            }

            return true;
        }

        private void QueryList()
        {

        }

        #endregion

        private void btnType_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            switch (btn.Content.ToString())
            {
                case "가상번호 현황":
                    lbVirtualNo.Visibility = Visibility.Visible;
                    lbVirtualNoList.Visibility = Visibility.Hidden;

                    // 검색부분
                    lbSearch.Content = "가상번호";
                    txtSrch.Tag = "가상번호 입력";
                    cboSearch.Visibility = Visibility.Collapsed;
                    dpanSearch.Visibility = Visibility.Collapsed;

                    // 내용부분
                    lbTitle.Content = "가상번호 현황";
                    ctlGrid.Visibility = Visibility.Visible;
                    ctlGridList.Visibility = Visibility.Collapsed;
                    break;

                case "가상번호 이용내역":
                    lbVirtualNo.Visibility = Visibility.Hidden;
                    lbVirtualNoList.Visibility = Visibility.Visible;

                    // 검색부분
                    lbSearch.Content = "검색";
                    txtSrch.Tag = "검색어 입력";
                    cboSearch.Visibility = Visibility.Visible;
                    dpanSearch.Visibility = Visibility.Visible;

                    // 내용부분
                    lbTitle.Content = "가상번호 이용내역";
                    ctlGrid.Visibility = Visibility.Collapsed;
                    ctlGridList.Visibility = Visibility.Visible;
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

        private void btnExcelDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ctlGridList.Visibility == Visibility.Collapsed)
                {
                    //파일 저장경로 받기
                    SaveFileDialog sfdlg = new SaveFileDialog();
                    Excel.Application excelApp = null;

                    sfdlg.CreatePrompt = true;
                    sfdlg.OverwritePrompt = true;
                    sfdlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    sfdlg.Filter = "모든엑셀(excel) 파일 | *.xls;*.xlsx;";
                    sfdlg.FileName = "가상번호현황_목록.xls";

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

                    ws.Cells[i, 1] = "'사용자 구분";
                    ws.Cells[i, 2] = "'가상번호";
                    ws.Cells[i, 3] = "'핸드폰번호";
                    ws.Cells[i, 4] = "'회원ID";
                    ws.Cells[i, 5] = "'회원명";
                    ws.Cells[i, 6] = "'탈퇴여부";
                    ws.Cells[i, 7] = "'탈퇴일자";
                    ws.Cells[i, 8] = "'할당일자";

                    i++;

                    //엑셀 작성
                    foreach (DataRowView row in ctlGrid.Items)
                    {
                        ws.Cells[i, 1] = "'" + row["user_type"].ToString();
                        ws.Cells[i, 2] = "'" + row["vr_num"].ToString();
                        ws.Cells[i, 3] = "'" + row["user_ph"].ToString();
                        ws.Cells[i, 4] = "'" + row["user_id"].ToString();
                        ws.Cells[i, 5] = "'" + row["user_name"].ToString();
                        ws.Cells[i, 6] = "'" + row["leave_yn"].ToString();
                        ws.Cells[i, 7] = "'" + row["leave_date"].ToString();
                        ws.Cells[i, 8] = "'" + row["conn_date"].ToString();
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
                else
                {
                    //파일 저장경로 받기
                    SaveFileDialog sfdlg = new SaveFileDialog();
                    Excel.Application excelApp = null;

                    sfdlg.CreatePrompt = true;
                    sfdlg.OverwritePrompt = true;
                    sfdlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    sfdlg.Filter = "모든엑셀(excel) 파일 | *.xls;*.xlsx;";
                    sfdlg.FileName = "가상번호이용_목록.xls";

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

                    ws.Cells[i, 1] = "'안심번호";
                    ws.Cells[i, 2] = "'호인입시간";
                    ws.Cells[i, 3] = "'발신번호";
                    ws.Cells[i, 4] = "'착신번호";
                    ws.Cells[i, 5] = "'통화시도시간";
                    ws.Cells[i, 6] = "'통화종료시간";
                    ws.Cells[i, 7] = "'호처리결과";

                    i++;

                    //엑셀 작성
                    foreach (DataRowView row in ctlGridList.Items)
                    {
                        ws.Cells[i, 1] = "'" + row["vrNum"].ToString();
                        ws.Cells[i, 2] = "'" + row["inTime"].ToString();
                        ws.Cells[i, 3] = "'" + row["senderNum"].ToString();
                        ws.Cells[i, 4] = "'" + row["receiverNum"].ToString();
                        ws.Cells[i, 5] = "'" + row["startTime"].ToString();
                        ws.Cells[i, 6] = "'" + row["endTime"].ToString();
                        ws.Cells[i, 7] = "'" + row["result"].ToString();
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

        private void btnVirtNo_Click(object sender, RoutedEventArgs e)
        {
            // 발급, 회수
            DataRowView dataRow = (DataRowView)((Button)e.Source).DataContext;
            Button btn = (Button)e.Source;
            string user_id = dataRow["user_id"].ToString();
            string user_ph = dataRow["user_ph"].ToString();
            string user_vph = dataRow["vr_num"].ToString();

            if (btn.Content.ToString() == "발급")
            {
                if (MessageBox.Show("가상번호를 발급 하시겠습니까?", "질문", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                string data = string.Format("proc_type={0}&user_id={1}&user_ph={2}", "40", user_id, user_ph);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/vrnum", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show("가상번호 발급 오류: " + strResult, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("가상번호를 발급 하였습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (MessageBox.Show("가상번호를 회수 하시겠습니까?", "알림", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                string data = string.Format("proc_type={0}&user_id={1}&user_vph={2}", "20", user_id, user_vph);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/vrnum", "POST", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show("가상번호 회수 오류: " + strResult, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("가상번호를 회수 하였습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            Query();
        }

        private void query_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            QueryList();
        }

        private void btnType2_Click(object sender, RoutedEventArgs e)
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
    }
}
