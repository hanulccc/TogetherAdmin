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

namespace SangAdmin.Matching
{
    /// <summary>
    /// ContentMPickup.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentMPickup : UserControl
    {
        BasePage _page = new BasePage();
        MainWindow mw;

        public ContentMPickup(MainWindow mw)
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
            ComboItemlist.Add(new ComboBoxPairs("요청", "R"));
            ComboItemlist.Add(new ComboBoxPairs("매칭", "A"));
            ComboItemlist.Add(new ComboBoxPairs("승차", "I"));
            ComboItemlist.Add(new ComboBoxPairs("하차", "O"));
            ComboItemlist.Add(new ComboBoxPairs("취소", "C"));

            cboPickupStatus.Items.Clear();
            cboPickupStatus.SelectedValuePath = "Value";
            cboPickupStatus.DisplayMemberPath = "Name";
            cboPickupStatus.ItemsSource = ComboItemlist;
            cboPickupStatus.SelectedIndex = 0;

            ComboItemlist = new List<ComboBoxPairs>();
            ComboItemlist.Add(new ComboBoxPairs("전체", ""));
            ComboItemlist.Add(new ComboBoxPairs("일반", "일반"));
            ComboItemlist.Add(new ComboBoxPairs("다이렉트", "다이렉트"));

            cboCallType.Items.Clear();
            cboCallType.SelectedValuePath = "Value";
            cboCallType.DisplayMemberPath = "Name";
            cboCallType.ItemsSource = ComboItemlist;
            cboCallType.SelectedIndex = 0;

            txtFrDate.SelectedDate = DateTime.Now.AddDays(-5);
            txtToDate.SelectedDate = DateTime.Now;

            Sign();
        }

        public void Restart()
        {
            cboPickupStatus.SelectedIndex = 0;
            cboCallType.SelectedIndex = 0;
            cboSearch.SelectedIndex = 0;
            txtSrch.Text = "";
            txtFrDate.SelectedDate = DateTime.Now.AddDays(-5);
            txtToDate.SelectedDate = DateTime.Now;

            Sign();
            Query();
        }

        private void Sign()
        {
            try
            {
                string data = string.Format("proc_type={0}", "50");
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/pickup", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show("픽업현황판 조회 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                txtTodayCnt.Text = jObject["resultData"][0]["today_cnt"].ToString();
                txtFinishCnt.Text = jObject["resultData"][0]["finish_cnt"].ToString();
                txtCancelCnt.Text = jObject["resultData"][0]["cancel_cnt"].ToString();
                txtWaitCnt.Text = jObject["resultData"][0]["wait_cnt"].ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show("픽업현황판 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region [ 조회 ]
        private void Query()
        {
            DataSet dsData = null;
            try
            {
                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                if (cboCallType.SelectedItem == null) return;

                ComboBoxPairs ComboItem = (ComboBoxPairs)cboPickupStatus.SelectedItem;
                string PickupStatus = ComboItem.Value;

                string fr_date = "";
                string to_date = "";
                string SrchUser = "";
                string SrchAddr = "";

                if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }
                if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }

                if (cboSearch.SelectedIndex == 0) { SrchUser = txtSrch.Text; }
                else { SrchAddr = txtSrch.Text; }

                string data = string.Format("proc_type={0}&status={1}&start_date={2}&end_date={3}&user_name={4}&addr={5}&call_type={6}", "10", PickupStatus, fr_date, to_date, SrchUser, SrchAddr, cboCallType.SelectedValue.ToString());
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/pickup", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable pickupTable = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlGrid.ItemsSource = pickupTable.DefaultView;
                }

            }
            catch (Exception e)
            {
                MessageBox.Show("픽업상태별현황 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (dsData != null) dsData.Dispose();
                ctlGrid.Cursor = Cursors.Arrow;
            }
        }
        #endregion

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
                sfdlg.FileName = "픽업매칭현황_목록.xls";

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

                ws.Cells[i, 1] = "'상태";
                ws.Cells[i, 2] = "'요청번호";
                ws.Cells[i, 3] = "'요청타입";
                ws.Cells[i, 4] = "'드라이버 이름";
                ws.Cells[i, 5] = "'쉘퍼 이름";
                ws.Cells[i, 6] = "'요청일";
                ws.Cells[i, 7] = "'요청시간";
                ws.Cells[i, 8] = "'매칭시간";
                ws.Cells[i, 9] = "'승차시간";
                ws.Cells[i, 10] = "'하차시간";
                ws.Cells[i, 11] = "'취소시간";
                ws.Cells[i, 12] = "'출발지";
                ws.Cells[i, 13] = "'도착지";
                ws.Cells[i, 14] = "'감사포인트";
                ws.Cells[i, 15] = "'전동휠유무";
                ws.Cells[i, 16] = "'기사메모";
                ws.Cells[i, 17] = "'사유";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["pickup_status"].ToString();
                    ws.Cells[i, 2] = "'" + row["order_id"].ToString();
                    ws.Cells[i, 3] = "'" + row["call_type"].ToString();
                    ws.Cells[i, 4] = "'" + row["driver_name"].ToString();
                    ws.Cells[i, 5] = "'" + row["helper_name"].ToString();
                    ws.Cells[i, 6] = "'" + row["req_date"].ToString();
                    ws.Cells[i, 7] = "'" + row["req_time"].ToString();
                    ws.Cells[i, 8] = "'" + row["accept_date"].ToString();
                    ws.Cells[i, 9] = "'" + row["geton_date"].ToString();
                    ws.Cells[i, 10] = "'" + row["getoff_date"].ToString();
                    ws.Cells[i, 11] = "'" + row["cancel_date"].ToString();
                    ws.Cells[i, 12] = "'" + row["start_addr"].ToString();
                    ws.Cells[i, 13] = "'" + row["end_addr"].ToString();
                    ws.Cells[i, 14] = "'" + row["drive_fee"].ToString();
                    ws.Cells[i, 15] = "'" + row["wheel_yn"].ToString();
                    ws.Cells[i, 16] = "'" + row["driver_memo"].ToString();
                    ws.Cells[i, 17] = "'" + row["memo"].ToString();
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

        private void txtSrch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { Query(); }
        }

        private void query_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Query();
        }

        private void btnMsgSend_Click(object sender, RoutedEventArgs e)
        {
            this.mw.borBackground.Visibility = Visibility.Visible;

            DlgMsgSend dms = new DlgMsgSend(null, this.mw);
            dms.ShowDialog();

            this.mw.borBackground.Visibility = Visibility.Collapsed;
        }
    }
}
