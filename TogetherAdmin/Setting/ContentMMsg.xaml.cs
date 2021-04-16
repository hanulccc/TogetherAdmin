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
    public partial class ContentMMsg : UserControl
    {
        BasePage _page = new BasePage();
        MainWindow mw;
        public ContentMMsg(MainWindow mw)
        {
            InitializeComponent();

            this.mw = mw;

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            Query();
            /*txtFrDate.SelectedDate = DateTime.Now.AddDays(-5);
            txtToDate.SelectedDate = DateTime.Now;*/
        }
        #endregion

        #region [ 조회 ]
        private void Query()
        {
            try
            {
                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string data = string.Format("proc_type={0}", "80");
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/sms", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable smsTable = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlGrid.ItemsSource = smsTable.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("자동발송문자/PUSH 데이터 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (ctlGrid != null)
                {
                    ctlGrid.Cursor = Cursors.Arrow;
                }
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
                sfdlg.FileName = "문자및푸쉬.xls";

                if (!(bool)sfdlg.ShowDialog()) { return; }

                //파일 있다면 제거
                FileInfo excelFile = new FileInfo(sfdlg.FileName);
                if (excelFile.Exists) { excelFile.Delete(); }

                // 첫번째 워크시트 가져오기
                excelApp = new Excel.Application();
                Excel.Workbook wb = excelApp.Workbooks.Add(true);
                Excel._Worksheet ws = wb.Worksheets.get_Item(1) as Excel._Worksheet;

                int i = 1;

                ws.Cells[i, 1] = "'NO";
                ws.Cells[i, 2] = "'자동발송";
                ws.Cells[i, 3] = "'제목";
                ws.Cells[i, 4] = "'내용";
                ws.Cells[i, 5] = "'발신번호";
                ws.Cells[i, 6] = "'문자발송유무";
                ws.Cells[i, 7] = "'PUSH발송유무";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["send_seq"].ToString();
                    ws.Cells[i, 2] = "'" + row["title"].ToString();
                    ws.Cells[i, 3] = "'" + row["send_title"].ToString();
                    ws.Cells[i, 4] = "'" + row["send_msg"].ToString();
                    ws.Cells[i, 5] = "'" + row["send_sender"].ToString();
                    ws.Cells[i, 6] = "'" + row["send_sms_yn"].ToString();
                    ws.Cells[i, 7] = "'" + row["send_push_yn"].ToString();
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

            DlgAutoMsgSetting dms = new DlgAutoMsgSetting(null, this.mw);
            dms.ShowDialog();

            Query();

            this.mw.borBackground.Visibility = Visibility.Collapsed;
        }

        private void ctlGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.mw.borBackground.Visibility = Visibility.Visible;

                DataRowView drv = ctlGrid.SelectedItem as DataRowView;

                if (drv == null) { return; }

                DlgAutoMsgSetting dms = new DlgAutoMsgSetting(drv, this.mw);
                dms.ShowDialog();

                Query();
            }
            finally
            {
                this.mw.borBackground.Visibility = Visibility.Collapsed;
            }
        }
    }
}
