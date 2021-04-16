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

namespace SangAdmin.VirtualAccnt
{
    /// <summary>
    /// ContentDepositList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentDepositList : UserControl
    {
        BasePage _page = new BasePage();

        public ContentDepositList()
        {
            InitializeComponent();

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;
        }

        public void Restart()
        {
            cboInoutType.SelectedIndex = 0;
            cboSearch.SelectedIndex = 0;
            cboSrchBank.SelectedIndex = 0;

            txtSrch.Text = "";
            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;

            typeChange("전체");
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


                ComboBoxItem ComboItem = (ComboBoxItem)cboSrchBank.SelectedItem;
                string van_name = ComboItem.Content.ToString();

                ComboItem = (ComboBoxItem)cboInoutType.SelectedItem;
                string inout_type = ""; // ComboItem.Content.ToString();

                ComboItem = (ComboBoxItem)cboSearch.SelectedItem;
                string srch_type = ComboItem.Tag.ToString();

                string fr_date = "";
                string to_date = "";

                if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }
                if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }


                string data = string.Format("program_code={0}&start_date={1}&end_date={2}&van_name={3}&user_type={4}&inout_type={5}&srch_type={6}&srch_word={7}", "together", fr_date, to_date, van_name, "", inout_type, srch_type, txtSrch.Text);
                string strResult = _page.HttpSendData("http://114.207.112.42:5455/inout", "GET", data);

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
                // !-- 이상!! 수정예정 --! //
                //파일 저장경로 받기
                SaveFileDialog sfdlg = new SaveFileDialog();
                Excel.Application excelApp = null;

                sfdlg.CreatePrompt = true;
                sfdlg.OverwritePrompt = true;
                sfdlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                sfdlg.Filter = "모든엑셀(excel) 파일 | *.xls;*.xlsx;";
                sfdlg.FileName = "입출금현황_목록.xls";

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

                ws.Cells[i, 1] = "'거래일자";
                ws.Cells[i, 2] = "'거래시간";
                ws.Cells[i, 3] = "'벤사";
                ws.Cells[i, 4] = "'사용자";
                ws.Cells[i, 5] = "'아이디";
                ws.Cells[i, 6] = "'입출금";
                ws.Cells[i, 7] = "'구분상세";
                ws.Cells[i, 8] = "'사용자 은행명";
                ws.Cells[i, 9] = "'사용자 계좌번호";
                ws.Cells[i, 10] = "'대상자명";
                ws.Cells[i, 11] = "'대상자 은행명";
                ws.Cells[i, 12] = "'대상자 계좌번호";
                ws.Cells[i, 13] = "'금액";
                ws.Cells[i, 14] = "'유저메모";
                ws.Cells[i, 15] = "'관리자메모";
                ws.Cells[i, 16] = "'잔액";
                ws.Cells[i, 17] = "'상태";
                ws.Cells[i, 18] = "'처리자 아이디";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["log_date"].ToString();
                    ws.Cells[i, 2] = "'" + row["log_time"].ToString();
                    ws.Cells[i, 3] = "'" + row["log_vname"].ToString();
                    ws.Cells[i, 4] = "'" + row["log_etc1"].ToString();
                    ws.Cells[i, 5] = "'" + row["log_user_id"].ToString();
                    ws.Cells[i, 6] = "'" + row["log_type"].ToString();
                    ws.Cells[i, 7] = "'" + row["log_type2"].ToString();
                    ws.Cells[i, 8] = "'" + row["log_bank_name"].ToString();
                    ws.Cells[i, 9] = "'" + row["log_account"].ToString();
                    ws.Cells[i, 10] = "'" + row["log_target_name"].ToString();
                    ws.Cells[i, 11] = "'" + row["log_target_bank_name"].ToString();
                    ws.Cells[i, 12] = "'" + row["log_target_account"].ToString();
                    ws.Cells[i, 13] = "'" + row["log_price"].ToString();
                    ws.Cells[i, 14] = "'" + row["log_memo"].ToString();
                    ws.Cells[i, 15] = "'" + row["log_memo2"].ToString();
                    ws.Cells[i, 16] = "'" + row["log_balance"].ToString();
                    ws.Cells[i, 17] = "'" + row["log_state"].ToString();
                    ws.Cells[i, 18] = "'" + row["log_mng_id"].ToString();
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


        private void query_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Query();
        }

        private void txtSrch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { Query(); }
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
    }
}
