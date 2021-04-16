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
    public partial class ContentMNotiCall : UserControl
    {
        BasePage _page = new BasePage();
        MainWindow mw;

        public ContentMNotiCall(MainWindow mw)
        {
            InitializeComponent();

            this.mw = mw;

            SetDefault();
            Query();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            List<ComboBoxPairs> ComboItemlist = new List<ComboBoxPairs>();
            ComboItemlist.Add(new ComboBoxPairs("전체", ""));
            ComboItemlist.Add(new ComboBoxPairs("드라이버에게문자", "드라이버에게문자"));
            ComboItemlist.Add(new ComboBoxPairs("쉘퍼에게문자", "쉘퍼에게문자"));
            ComboItemlist.Add(new ComboBoxPairs("하차사유", "하차사유"));
            ComboItemlist.Add(new ComboBoxPairs("본사전달", "본사전달"));

            cboSrchCategory.Items.Clear();
            cboSrchCategory.SelectedValuePath = "Value";
            cboSrchCategory.DisplayMemberPath = "Name";
            cboSrchCategory.ItemsSource = ComboItemlist;
            cboSrchCategory.SelectedIndex = 0;
        }

        #endregion

        #region [ 조회 ]
        private void Query()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string data = string.Format("proc_type={0}&category={1}&search_text={2}", "10", cboSrchCategory.SelectedValue.ToString(), txtSrch.Text);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/infocall", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable dt = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("공지사항 데이터 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
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
                sfdlg.FileName = "안내콜관리_목록.xls";

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
                ws.Cells[i, 3] = "'메시지";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["idx"].ToString();
                    ws.Cells[i, 2] = "'" + row["category"].ToString();
                    ws.Cells[i, 3] = "'" + row["msg"].ToString();
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

            Window child = new DlgNotiCall(this.mw, null);
            child.Owner = Application.Current.MainWindow;
            child.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            child.ShowDialog();

            Query();

            this.mw.borBackground.Visibility = Visibility.Collapsed;
        }

        private void ctlGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.mw.borBackground.Visibility = Visibility.Visible;

            DataRowView drv = ctlGrid.SelectedItem as DataRowView;

            if (drv != null)
            {
                Window child = new DlgNotiCall(this.mw, drv);
                child.Owner = Application.Current.MainWindow;
                child.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                child.ShowDialog();

                Query();
            }

            this.mw.borBackground.Visibility = Visibility.Collapsed;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("선택한 항목을 삭제 하시겠습니까?", "알림", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

            try
            {
                ctlGrid.Cursor = Cursors.Wait;

                foreach (DataRowView row in ctlGrid.Items)
                {
                    if (row["chkYn"].ToString() == "True")
                    {
                        string data = string.Format("proc_type={0}&idx={1}&admin_id={2}&admin_name={3}", "40", row["idx"].ToString(), this.mw.strAdId, this.mw.strAdName);
                        string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/infocall", "POST", data);

                        JObject jObject = JObject.Parse(strResult); //json 객체로

                        if (jObject["resultCode"].ToString() != "200")
                        {
                            MessageBox.Show("안내콜 삭제 중 오류.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                Query();
            }
            catch (Exception ex)
            {
                MessageBox.Show("FAQ 데이터 삭제 중 오류발생 :" + ex.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ctlGrid.Cursor = Cursors.Arrow;
            }
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            foreach (DataRowView row in ctlGrid.Items)
            {
                row["chkYn"] = true;
            }
        }

        private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            foreach (DataRowView row in ctlGrid.Items)
            {
                row["chkYn"] = false;
            }
        }
    }
}
