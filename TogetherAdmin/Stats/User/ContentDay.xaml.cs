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

namespace SangAdmin.Stats.User
{
    /// <summary>
    /// ContentMPickup.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentDay : UserControl
    {
        BasePage _page = new BasePage();
        Style grayStyle = Application.Current.FindResource("btnGrayRectRound") as Style;
        Style greenStyle = Application.Current.FindResource("btnGreenRectRound") as Style;

        public ContentDay()
        {
            InitializeComponent();
            
            SetDefault();

            if (Query() == false) return;
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;
        }

        public void Restart()
        {
            foreach (Button btn in dpanYoil.Children)
            {
                if (btn.Style == this.greenStyle)
                {
                    btn.Style = this.grayStyle;
                }
            }

            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;
        }
        #endregion

        private bool Query()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string yoil = "";
                string fr_date = "";
                string to_date = "";

                if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }
                if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }

                foreach (Button btn in dpanYoil.Children)
                {
                    if (btn.Style == this.greenStyle)
                    {
                        if (yoil == "") { yoil = btn.Content.ToString(); }
                        else { yoil += "," + btn.Content.ToString(); }
                    }
                }

                JObject jObj = Api.GetResponseJObject(Api.statUser_url + "?proc_type=10&start_date=" + fr_date + "&end_date=" + to_date + "&yoil=" + yoil);

                if (jObj == null) return false;

                if (jObj["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObj["resultMsg"].ToString());
                    return false;
                }

                if (jObj["resultData"].ToString() == "") return false;

                DataTable userTable = JsonConvert.DeserializeObject<DataTable>(jObj["resultData"].ToString());
                ctlGrid.ItemsSource = userTable.DefaultView;

                this.DataContext = this;
            }
            catch (Exception ex)
            {
                MessageBox.Show("통계 데이터 조회 중 오류 : " + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine("통계 데이터 조회 중 오류 : " + ex);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }

            return true;
        }

        private void cbYoil_Checked(object sender, RoutedEventArgs e)
        {
            Query();
        }

        private void txtDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtFrDate.Text != "" && txtToDate.Text != "")
            {
                Query();
            }
        }

        private void btnExcelSave_Click(object sender, RoutedEventArgs e)
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
                sfdlg.FileName = "사용자_일주월간_통계_목록.xls";

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
                ws.Cells[i, 2] = "'총 드라이버 수";
                ws.Cells[i, 3] = "'총 쉘퍼수";
                ws.Cells[i, 4] = "'쉘퍼 심사 요청자 수";
                ws.Cells[i, 5] = "'신규 가입자 수";
                ws.Cells[i, 6] = "'총 가입자 수";
                ws.Cells[i, 7] = "'활성 사용자 수";
                ws.Cells[i, 8] = "'앱 다운로드 수";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["stat_date"].ToString();
                    ws.Cells[i, 2] = "'" + row["stat_dtot"].ToString();
                    ws.Cells[i, 3] = "'" + row["stat_htot"].ToString();
                    ws.Cells[i, 4] = "'" + row["stat_hreq"].ToString();
                    ws.Cells[i, 5] = "'" + row["stat_signup"].ToString();
                    ws.Cells[i, 6] = "'" + row["stat_tot"].ToString();
                    ws.Cells[i, 7] = "'" + row["stat_active"].ToString();
                    ws.Cells[i, 8] = "'" + row["stat_download"].ToString();
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

        private void btnYoil_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn.Style == this.greenStyle) { btn.Style = this.grayStyle; }
            else { btn.Style = this.greenStyle; }

            Query();
        }
    }
}
