using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SangAdmin.Common;
using SangAdmin.Other;
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

namespace SangAdmin.Setting
{
    /// <summary>
    /// ContentMMsgList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentMMsgList : UserControl
    {
        BasePage _page = new BasePage();
        MainWindow mw;

        public ContentMMsgList(MainWindow mw)
        {
            InitializeComponent();

            this.mw = mw;

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
            txtSrch.Text = "";

            typeChange("전체");

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
                string type = "";

                if (lbTypeSMS.Visibility == Visibility.Visible) { type = "SMS"; }
                else if (lbTypePUSH.Visibility == Visibility.Visible) { type = "PUSH"; }

                if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }
                if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }

                JObject jObject = Api.GetResponseJObject(Api.sms_url + "?proc_type=130&srch_text=" + txtSrch.Text + "&start_date=" + fr_date + "&end_date=" + to_date + "&type=" + type);

                if (jObject == null || jObject["resultCode"] == null) return;

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
                MessageBox.Show("발송내역 조회 중 오류발생: " + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        #endregion

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
                    lbTypeSMS.Visibility = Visibility.Hidden;
                    lbTypePUSH.Visibility = Visibility.Hidden;
                    break;

                case "SMS":
                    lbTypeAll.Visibility = Visibility.Hidden;
                    lbTypeSMS.Visibility = Visibility.Visible;
                    lbTypePUSH.Visibility = Visibility.Hidden;
                    break;

                case "PUSH":
                    lbTypeAll.Visibility = Visibility.Hidden;
                    lbTypeSMS.Visibility = Visibility.Hidden;
                    lbTypePUSH.Visibility = Visibility.Visible;
                    break;
            }

            Query();
        }

        private void btnMsgSend_Click(object sender, RoutedEventArgs e)
        {
            this.mw.borBackground.Visibility = Visibility.Visible;

            DlgMsgPushSend dms = new DlgMsgPushSend(this.mw);
            dms.ShowDialog();

            Query();

            this.mw.borBackground.Visibility = Visibility.Collapsed;
        }


        private void txtSrch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { Query(); }
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            Query();
        }

        private void query_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                sfdlg.FileName = "문자발송_목록.xls";

                if (!(bool)sfdlg.ShowDialog()) { return; }

                //파일 있다면 제거
                FileInfo excelFile = new FileInfo(sfdlg.FileName);
                if (excelFile.Exists) { excelFile.Delete(); }

                // 첫번째 워크시트 가져오기
                excelApp = new Excel.Application();
                Excel.Workbook wb = excelApp.Workbooks.Add(true);
                Excel._Worksheet ws = wb.Worksheets.get_Item(1) as Excel._Worksheet;

                int i = 1;

                ws.Cells[i, 1] = "'No";
                ws.Cells[i, 2] = "'구분";
                ws.Cells[i, 3] = "'내용";
                ws.Cells[i, 4] = "'발신번호";
                ws.Cells[i, 5] = "'발신일자";
                ws.Cells[i, 6] = "'발송건수";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["msg_idx"].ToString();
                    ws.Cells[i, 2] = "'" + row["send_type"].ToString();
                    ws.Cells[i, 3] = "'" + row["msg_content"].ToString();
                    ws.Cells[i, 4] = "'" + row["send_no"].ToString();
                    ws.Cells[i, 5] = "'" + row["send_date"].ToString();
                    ws.Cells[i, 6] = "'" + row["send_cnt"].ToString();
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

        private void btnRecvUserDetail_Click(object sender, RoutedEventArgs e)
        {
            DataRowView dataRowView = (DataRowView)((Button)e.Source).DataContext;

            string data = string.Format("proc_type={0}&type={1}&code={2}", "140", dataRowView["send_type"].ToString(), dataRowView["code"].ToString());
            string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/sms", "GET", data);

            JObject jObject = JObject.Parse(strResult); //json 객체로

            if (jObject["resultCode"].ToString() == "200")
            {
                DataTable dt = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                ctlRecvUser.ItemsSource = dt.DefaultView;
            }

            bdRecvUser.Visibility = Visibility.Visible;
        }

        private void btnRecvClose_Click(object sender, RoutedEventArgs e)
        {
            bdRecvUser.Visibility = Visibility.Collapsed;
        }
    }
}
