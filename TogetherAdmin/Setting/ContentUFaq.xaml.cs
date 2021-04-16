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
    public partial class ContentUFaq : UserControl
    {
        BasePage _page = new BasePage();
        MainWindow mw;

        public ContentUFaq(MainWindow mw)
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
            ComboItemlist.Add(new ComboBoxPairs("가입", "가입"));
            ComboItemlist.Add(new ComboBoxPairs("매칭", "매칭"));
            ComboItemlist.Add(new ComboBoxPairs("취소/지연", "취소/지연"));
            ComboItemlist.Add(new ComboBoxPairs("기타", "기타"));

            cboCategory.Items.Clear();
            cboCategory.SelectedValuePath = "Value";
            cboCategory.DisplayMemberPath = "Name";
            cboCategory.ItemsSource = ComboItemlist;
            cboCategory.SelectedIndex = 0;

            ComboItemlist = new List<ComboBoxPairs>();
            ComboItemlist.Add(new ComboBoxPairs("전체", ""));
            ComboItemlist.Add(new ComboBoxPairs("제목", "제목"));
            ComboItemlist.Add(new ComboBoxPairs("내용", "내용"));

            cboSearch.Items.Clear();
            cboSearch.SelectedValuePath = "Value";
            cboSearch.DisplayMemberPath = "Name";
            cboSearch.ItemsSource = ComboItemlist;
            cboSearch.SelectedIndex = 0;
        }

        public void Restart()
        {
            cboCategory.SelectedIndex = 0;
            cboSearch.SelectedIndex = 0;

            txtSrch.Text = "";
        }
        #endregion

        #region [ 조회 ]
        private void Query()
        {
            try
            {
                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string data = string.Format("proc_type={0}&category={1}&search_text={2}", "10", cboCategory.SelectedValue.ToString(), txtSrch.Text);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/faq", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable smsTable = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlGrid.ItemsSource = smsTable.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("FAQ 데이터 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                ctlGrid.Cursor = Cursors.Arrow;
            }

            return;
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
                sfdlg.FileName = "자주묻는질문_목록.xls";

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

                ws.Cells[i, 1] = "'No";
                ws.Cells[i, 2] = "'구분";
                ws.Cells[i, 3] = "'제목";
                ws.Cells[i, 4] = "'내용";
                ws.Cells[i, 5] = "'등록일";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["idx"].ToString();
                    ws.Cells[i, 2] = "'" + row["category"].ToString();
                    ws.Cells[i, 3] = "'" + row["title"].ToString();
                    ws.Cells[i, 4] = "'" + row["content"].ToString();
                    ws.Cells[i, 5] = "'" + row["reg_date"].ToString();
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

            DlgFaq dms = new DlgFaq(mw, null);
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

                Window child = new DlgFaq(mw, drv);

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
                        string strIdx = row["idx"].ToString();

                        string data = string.Format("proc_type={0}&idx={1}&admin_id={2}&admin_name={3}", "40", strIdx, mw.strAdId, mw.strAdName);
                        string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/faq", "POST", data);

                        JObject jObject = JObject.Parse(strResult); //json 객체로

                        if (jObject["resultCode"].ToString() != "200")
                        {
                            MessageBox.Show("FAQ 삭제 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
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
