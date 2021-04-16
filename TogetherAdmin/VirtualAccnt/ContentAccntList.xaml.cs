using SangAdmin.Common;
using Microsoft.Win32;
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
    /// ContentAccntList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentAccntList : UserControl
    {
        BasePage _page = new BasePage();

        public ContentAccntList()
        {
            InitializeComponent();
            SetDefault();

            if (Query() == false) return;

        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            List<ComboBoxPairs> ComboItemlist = new List<ComboBoxPairs>();
            ComboItemlist.Add(new ComboBoxPairs("전체", ""));
            ComboItemlist.Add(new ComboBoxPairs("기업은행", "기업은행"));
            ComboItemlist.Add(new ComboBoxPairs("국민은행", "국민은행"));
            ComboItemlist.Add(new ComboBoxPairs("하나은행", "하나은행"));
            ComboItemlist.Add(new ComboBoxPairs("우리은행", "우리은행"));
            ComboItemlist.Add(new ComboBoxPairs("신한은행", "신한은행"));

            cboSrchBank.Items.Clear();
            cboSrchBank.SelectedValuePath = "Value";
            cboSrchBank.DisplayMemberPath = "Name";
            cboSrchBank.ItemsSource = ComboItemlist;
            cboSrchBank.SelectedIndex = 0;
        }

        public void Restart()
        {
            cboSrchBank.SelectedIndex = 0;
            txtSrch.Text = "";
            Query();
        }
        #endregion


        #region [ 조회 ]
        private bool Query()
        {

            DataSet dsData = null;
            try
            {
                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string data = string.Format("start_date={0}&end_date={1}&account={2}", "", "", txtSrch.Text);
                string strResult = _page.HttpSendData("http://114.207.112.42:5455/history", "GET", data);
                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show("가상계좌 현황 조회 오류" + strResult, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                //Json resultData Parse
                JArray jArray = JArray.Parse(jObject["resultData"].ToString());
                
                DataTable dt = new DataTable("TableName");
                DataColumn col = new DataColumn();
                col.DataType = System.Type.GetType("System.String");
                col.ColumnName = "log_date";
                dt.Columns.Add(col);

                col = new DataColumn();
                col.DataType = System.Type.GetType("System.String");
                col.ColumnName = "log_user_id";
                dt.Columns.Add(col);

                col = new DataColumn();
                col.DataType = System.Type.GetType("System.String");
                col.ColumnName = "log_bank_name";
                dt.Columns.Add(col);

                col = new DataColumn();
                col.DataType = System.Type.GetType("System.String");
                col.ColumnName = "log_vname";
                dt.Columns.Add(col);

                col = new DataColumn();
                col.DataType = System.Type.GetType("System.String");
                col.ColumnName = "log_account";
                dt.Columns.Add(col);

                col = new DataColumn();
                col.DataType = System.Type.GetType("System.String");
                col.ColumnName = "log_mng_memo";
                dt.Columns.Add(col);

                foreach (var item in jArray.Children())
                {
                    if (cboSrchBank.SelectedValue.ToString() == "" || cboSrchBank.SelectedValue.ToString() == item["log_bank_name"].ToString())
                    {
                        DataRow dr = dt.NewRow();

                        dr["log_date"] = item["log_date"].ToString();
                        dr["log_user_id"] = item["log_user_id"].ToString();
                        dr["log_bank_name"] = item["log_bank_name"].ToString();
                        dr["log_vname"] = item["log_vname"].ToString();
                        dr["log_account"] = item["log_account"].ToString();
                        dr["log_mng_memo"] = item["log_mng_memo"].ToString();
                        dt.Rows.Add(dr);
                    }
                }

                ctlGrid.ItemsSource = dt.DefaultView;
            }
            catch (Exception e)
            {
                MessageBox.Show("가상계좌 거래내역 데이터 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                if (dsData != null) dsData.Dispose();
                ctlGrid.Cursor = Cursors.Arrow;
            }

            return true;
        }
        #endregion

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
                //파일 저장경로 받기
                SaveFileDialog sfdlg = new SaveFileDialog();
                Excel.Application excelApp = null;

                sfdlg.CreatePrompt = true;
                sfdlg.OverwritePrompt = true;
                sfdlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                sfdlg.Filter = "모든엑셀(excel) 파일 | *.xls;*.xlsx;";
                sfdlg.FileName = "가상계좌변동현황_목록.xls";

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

                ws.Cells[i, 1] = "'일자";
                ws.Cells[i, 2] = "'회원ID";
                ws.Cells[i, 3] = "'은행명";
                ws.Cells[i, 4] = "'업체명";
                ws.Cells[i, 5] = "'가상계좌번호";
                ws.Cells[i, 6] = "'메모";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["log_date"].ToString();
                    ws.Cells[i, 2] = "'" + row["log_user_id"].ToString();
                    ws.Cells[i, 3] = "'" + row["log_bank_name"].ToString();
                    ws.Cells[i, 4] = "'" + row["log_vname"].ToString();
                    ws.Cells[i, 5] = "'" + row["log_account"].ToString();
                    ws.Cells[i, 6] = "'" + row["log_mng_memo"].ToString();
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
    }
}
