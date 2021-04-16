using LiveCharts;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SangAdmin.Stats.Pickup;
using Excel = Microsoft.Office.Interop.Excel;

namespace SangAdmin.Stats
{
    /// <summary>
    /// ViewMatching.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PageTaxi : UserControl
    {
        public ChartValues<double> rcnt_value { get; set; }
        public ChartValues<double> ocnt_value { get; set; }
        public ChartValues<double> ccnt_value { get; set; }
        public string[] label { get; set; }

        Style grayStyle = Application.Current.FindResource("btnGrayRectRound") as Style;
        Style greenStyle = Application.Current.FindResource("btnGreenRectRound") as Style;

        public PageTaxi()
        {
            InitializeComponent();

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;

            Query();
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

            txtSrch.Text = "";

            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;
        }
        #endregion

        private bool Query()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

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

                // 일/주/월별 통계
                JObject jObj = Api.GetResponseJObject(Api.statTaxi_url + "?proc_type=10&start_date=" + fr_date + "&end_date=" + to_date + "&yoil=" + yoil);

                if (jObj == null) return false;

                if (jObj["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObj["resultMsg"].ToString());
                    return false;
                }
                if (jObj["resultData"].ToString() == "") return false;

                DataTable t = JsonConvert.DeserializeObject<DataTable>(jObj["resultData"].ToString());
                dayGrid.ItemsSource = t.DefaultView;



                // 시간대별 통계
                jObj = Api.GetResponseJObject(Api.statTaxi_url + "?proc_type=20&start_date=" + fr_date + "&end_date=" + to_date + "&yoil=" + yoil);

                if (jObj == null) return false;

                if (jObj["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObj["resultMsg"].ToString());
                    return false;
                }
                if (jObj["resultData"].ToString() == "") return false;

                t = JsonConvert.DeserializeObject<DataTable>(jObj["resultData"].ToString());
                timeGrid.ItemsSource = t.DefaultView;



                // 지역별 통계
                jObj = Api.GetResponseJObject(Api.statTaxi_url + "?proc_type=30&start_date=" + fr_date + "&end_date=" + to_date + "&srch_text=" + txtSrch.Text);

                if (jObj == null) return false;

                if (jObj["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObj["resultMsg"].ToString());
                    return false;
                }
                if (jObj["resultData"].ToString() == "") return false;

                t = JsonConvert.DeserializeObject<DataTable>(jObj["resultData"].ToString());
                areaGrid.ItemsSource = t.DefaultView;
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

        private void btnSrch_Click(object sender, RoutedEventArgs e)
        {
            Query();
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
                sfdlg.FileName = "택시동승통계_목록.xls";

                if (!(bool)sfdlg.ShowDialog()) { return; }

                Mouse.OverrideCursor = Cursors.Wait;

                //파일 있다면 제거
                FileInfo excelFile = new FileInfo(sfdlg.FileName);
                if (excelFile.Exists) { excelFile.Delete(); }

                // 첫번째 워크시트 가져오기
                excelApp = new Excel.Application();
                Excel.Workbook wb = excelApp.Workbooks.Add(true);
                Excel._Worksheet ws1 = wb.Worksheets.get_Item(1) as Excel._Worksheet;
                Excel._Worksheet ws2 = (Excel._Worksheet)excelApp.Worksheets.Add();
                Excel._Worksheet ws3 = (Excel._Worksheet)excelApp.Worksheets.Add();

                ws1.Name = "지역별 통계";
                ws2.Name = "시간대별 통계";
                ws3.Name = "일자별 통계";

                int i = 1;

                ws1.Cells[i, 1] = "'지역";
                ws1.Cells[i, 2] = "'채팅방 개수";

                ws2.Cells[i, 1] = "'시간대";
                ws2.Cells[i, 2] = "'채팅방 개수";

                ws3.Cells[i, 1] = "'일자";
                ws3.Cells[i, 2] = "'채팅방 개수";

                i++;

                //엑셀 작성
                foreach (DataRowView row in areaGrid.Items)
                {
                    ws1.Cells[i, 1] = "'" + row["stat_area"].ToString();
                    ws1.Cells[i, 2] = "'" + row["stat_chatcnt"].ToString();
                    i++;
                }

                i = 2;
                foreach (DataRowView row in timeGrid.Items)
                {
                    ws2.Cells[i, 1] = "'" + row["stat_time"].ToString();
                    ws2.Cells[i, 2] = "'" + row["stat_chatcnt"].ToString();
                    i++;
                }

                i = 2;
                foreach (DataRowView row in dayGrid.Items)
                {
                    ws3.Cells[i, 1] = "'" + row["stat_date"].ToString();
                    ws3.Cells[i, 2] = "'" + row["stat_chatcnt"].ToString();
                    i++;
                }

                // 엑셀파일 저장
                wb.SaveAs(sfdlg.FileName, Excel.XlFileFormat.xlWorkbookNormal, Type.Missing, Type.Missing, Type.Missing
                    , Type.Missing, Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing
                    , Type.Missing, Type.Missing);

                wb.Close(true);

                System.Runtime.InteropServices.Marshal.ReleaseComObject(excelApp);
                excelApp = null;
                System.Runtime.InteropServices.Marshal.ReleaseComObject(ws1);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(ws2);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(ws3);
                ws1 = null;
                ws2 = null;
                ws3 = null;
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
