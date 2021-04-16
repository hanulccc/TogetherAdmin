﻿using SangAdmin.Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
    public partial class ContentUNotice : UserControl
    {
        BasePage _page = new BasePage();
        MainWindow mw;

        public ContentUNotice(MainWindow mw)
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
            ComboItemlist.Add(new ComboBoxPairs("예", "1"));
            ComboItemlist.Add(new ComboBoxPairs("아니오", "0"));
            ComboItemlist.Add(new ComboBoxPairs("진행중", "2"));

            cboPopUpYn.Items.Clear();
            cboPopUpYn.SelectedValuePath = "Value";
            cboPopUpYn.DisplayMemberPath = "Name";
            cboPopUpYn.ItemsSource = ComboItemlist;
            cboPopUpYn.SelectedIndex = 0;

            txtFrDate.SelectedDate = DateTime.Now.AddDays(-60);
            txtToDate.SelectedDate = DateTime.Now;
        }

        public void Restart()
        {
            cboPopUpYn.SelectedIndex = 0;

            txtSrch.Text = "";
            txtFrDate.SelectedDate = DateTime.Now.AddDays(-60);
            txtToDate.SelectedDate = DateTime.Now;
        }
        #endregion

        #region [ 조회 ]
        private void Query()
        {
            try
            {
                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string fr_date = "";
                string to_date = "";

                if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }
                if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }

                string data = string.Format("proc_type={0}&start_date={1}&end_date={2}&search_text={3}&is_popup={4}", "10", fr_date, to_date, txtSrch.Text, cboPopUpYn.SelectedValue.ToString());
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/notice", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable smsTable = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlGrid.ItemsSource = smsTable.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("공지사항 데이터 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
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
                sfdlg.FileName = "공지사항_목록.xls";

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
                ws.Cells[i, 2] = "'팝업여부";
                ws.Cells[i, 3] = "'제목";
                ws.Cells[i, 4] = "'내용";
                ws.Cells[i, 5] = "'마지막 작성일";
                ws.Cells[i, 6] = "'이미지";
                ws.Cells[i, 7] = "'팝업기간(From)";
                ws.Cells[i, 8] = "'팝업기간(To)";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["idx"].ToString();
                    ws.Cells[i, 2] = "'" + row["is_popup"].ToString();
                    ws.Cells[i, 3] = "'" + row["title"].ToString();
                    ws.Cells[i, 4] = "'" + row["content"].ToString();
                    ws.Cells[i, 5] = "'" + row["reg_date"].ToString();
                    ws.Cells[i, 6] = "'" + row["file_name"].ToString();
                    ws.Cells[i, 7] = "'" + row["popup_frdate"].ToString();
                    ws.Cells[i, 8] = "'" + row["popup_todate"].ToString();
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

            Window child = new DlgNotice(null, this.mw);

            child.Owner = Application.Current.MainWindow;
            child.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            child.ShowDialog();

            Query();

            this.mw.borBackground.Visibility = Visibility.Collapsed;
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Image img = (Image)sender;

                string Url = img.Source.ToString().Replace("TH", "");

                Process picture = new Process();
                picture.StartInfo.FileName = "rundll32.exe";
                picture.StartInfo.Arguments = " shimgvw.dll ImageView_Fullscreen " + Url;
                picture.StartInfo.UseShellExecute = false;
                picture.Start();
                picture.WaitForExit();
            }
            catch
            {

            }
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

                    if (row == null) { MessageBox.Show("오류발생, 다시 시도해주세요."); return; }

                    Window child = new DlgNotice(row, this.mw);

                    child.Owner = Application.Current.MainWindow;

                    child.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                    child.ShowDialog();

                    Query();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show(ex.Message);
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
                        string data = string.Format("proc_type={0}&idx={1}&admin_id={2}&admin_name={3}", "40", row["idx"].ToString(), mw.strAdId, mw.strAdName);
                        string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/notice", "POST", data);

                        JObject jObject = JObject.Parse(strResult); //json 객체로

                        if (jObject["resultCode"].ToString() != "200")
                        {
                            MessageBox.Show("공지사항 삭제 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                            continue;
                        }
                    }
                }

                Query();
            }
            catch (Exception ex)
            {
                MessageBox.Show("공지사항 데이터 삭제 중 오류발생 :" + ex.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
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
