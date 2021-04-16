using SangAdmin.Common;
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
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;

namespace SangAdmin.User
{
    /// <summary>
    /// ContentShelperList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentShelperList : UserControl
    {
        BasePage _page = new BasePage();
        private static DispatcherTimer timer;
        MainWindow mw;

        public ContentShelperList(MainWindow mw)
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
            ComboItemlist.Add(new ComboBoxPairs("승인", "승인"));
            ComboItemlist.Add(new ComboBoxPairs("미승인", "미승인"));

            cboUserType.Items.Clear();
            cboUserType.SelectedValuePath = "Value";
            cboUserType.DisplayMemberPath = "Name";
            cboUserType.ItemsSource = ComboItemlist;
            cboUserType.SelectedIndex = 0;

            ComboItemlist = new List<ComboBoxPairs>();
            ComboItemlist.Add(new ComboBoxPairs("전체", ""));
            ComboItemlist.Add(new ComboBoxPairs("회원", "0"));
            ComboItemlist.Add(new ComboBoxPairs("탈퇴회원", "1"));

            cboLeaveYn.Items.Clear();
            cboLeaveYn.SelectedValuePath = "Value";
            cboLeaveYn.DisplayMemberPath = "Name";
            cboLeaveYn.ItemsSource = ComboItemlist;
            cboLeaveYn.SelectedIndex = 0;

            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;
        }

        public void Restart()
        {
            cboUserType.SelectedIndex = 0;
            cboLeaveYn.SelectedIndex = 0;
            cboSearch.SelectedIndex = 0;

            txtSrch.Text = "";
            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;
        }
        #endregion


        #region [ 조회 ]
        private bool Query()
        {
            try
            {
                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                ComboBoxPairs ComboItem = (ComboBoxPairs)cboUserType.SelectedItem;
                if (ComboItem == null) { return false; }
                
                string UserType = ComboItem.Value;

                ComboItem = (ComboBoxPairs)cboLeaveYn.SelectedItem;
                if (ComboItem == null) { return false; }

                string leaveYn = ComboItem.Value;


                string fr_date = "";
                string to_date = "";
                string SrchUser = "";
                string SrchHp = "";

                if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }
                if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }

                if (cboSearch.SelectedIndex == 0) { SrchUser = txtSrch.Text; }
                else { SrchHp = txtSrch.Text; }

                string data = string.Format("proc_type={0}&usertype={1}&frdate={2}&todate={3}&srchuser={4}&srchhp={5}&leaveyn={6}", "15", UserType, fr_date, to_date, SrchUser, SrchHp, leaveYn);

                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200") { return false; }

                JArray jArray = JArray.Parse(jObject["resultData"].ToString());
                JArray jary = new JArray();

                foreach (JObject jItem in jArray)
                {
                    if (jItem["approval_memo"].ToString() != "")
                    {
                        jItem["approval_memo"] = jItem["approval_memo"].ToString().Replace("\n", ", ");
                    }
                    jary.Add(jItem);
                }

                DataTable userTable = JsonConvert.DeserializeObject<DataTable>(jary.ToString());
                ctlGrid.ItemsSource = userTable.DefaultView;
            }
            catch (Exception e)
            {
                MessageBox.Show("쉘퍼가입정보 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                ctlGrid.Cursor = Cursors.Arrow;
            }

            return true;
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
                sfdlg.FileName = "쉘퍼가입관리_목록.xls";

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

                ws.Cells[i, 1] = "'승인여부";
                ws.Cells[i, 2] = "'회원ID";
                ws.Cells[i, 3] = "'성명";
                ws.Cells[i, 4] = "'핸드폰";
                ws.Cells[i, 5] = "'프로필사진";
                ws.Cells[i, 6] = "'면허증사진";
                ws.Cells[i, 7] = "'운전면허번호";
                ws.Cells[i, 8] = "'운전면허만료";
                ws.Cells[i, 9] = "'프로필심사";
                ws.Cells[i, 10] = "'면허증심사";
                ws.Cells[i, 11] = "'가입일";
                ws.Cells[i, 12] = "'승인일";
                ws.Cells[i, 13] = "'자동승인여부";
                ws.Cells[i, 14] = "'승인거절 사유";
                ws.Cells[i, 15] = "'탈퇴여부";
                ws.Cells[i, 16] = "'탈퇴일";
                ws.Cells[i, 17] = "'탈퇴사유";
                ws.Cells[i, 18] = "'차량번호";
                ws.Cells[i, 19] = "'보험번호";
                ws.Cells[i, 20] = "'보험시작일";
                ws.Cells[i, 21] = "'보험종료일";
                ws.Cells[i, 22] = "'제조사";
                ws.Cells[i, 23] = "'모델명";
                ws.Cells[i, 24] = "'탑승인원";
                ws.Cells[i, 25] = "'차량명의";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["user_status"].ToString();
                    ws.Cells[i, 2] = "'" + row["user_id"].ToString();
                    ws.Cells[i, 3] = "'" + row["user_name"].ToString();
                    ws.Cells[i, 4] = "'" + row["user_ph"].ToString();
                    ws.Cells[i, 5] = "'" + row["profile_img"].ToString();
                    ws.Cells[i, 6] = "'" + row["license_img"].ToString();
                    ws.Cells[i, 7] = "'" + row["license_num"].ToString();
                    ws.Cells[i, 8] = "'" + row["license_dt"].ToString();
                    ws.Cells[i, 9] = "'" + row["profile_yn"].ToString();
                    ws.Cells[i, 10] = "'" + row["license_yn"].ToString();
                    ws.Cells[i, 11] = "'" + row["reg_date"].ToString();
                    ws.Cells[i, 12] = "'" + row["ok_date"].ToString();
                    ws.Cells[i, 13] = "'" + row["auto_approval"].ToString();
                    ws.Cells[i, 14] = "'" + row["approval_memo"].ToString();
                    ws.Cells[i, 15] = "'" + row["leave_yn"].ToString();
                    ws.Cells[i, 16] = "'" + row["leave_date"].ToString();
                    ws.Cells[i, 17] = "'" + row["leave_memo"].ToString();
                    ws.Cells[i, 18] = "'" + row["car_num"].ToString();
                    ws.Cells[i, 19] = "'" + row["bohum_no"].ToString();
                    ws.Cells[i, 20] = "'" + row["bohum_frdate"].ToString();
                    ws.Cells[i, 21] = "'" + row["bohum_todate"].ToString();
                    ws.Cells[i, 22] = "'" + row["car_brand"].ToString();
                    ws.Cells[i, 23] = "'" + row["car_model"].ToString();
                    ws.Cells[i, 24] = "'" + row["car_max"].ToString();
                    ws.Cells[i, 25] = "'" + row["car_owner"].ToString();
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

        private void btnAutoSelect_Click(object sender, RoutedEventArgs e)
        {
            if (txtAutoSelect.Text == "자동 조회")
            {
                timer = new DispatcherTimer();

                timer.Interval = TimeSpan.FromSeconds(5);
                timer.Tick += new EventHandler(OnTimedEvent);
                timer.Start();

                txtAutoSelect.Text = "수동 조회";
            }
            else
            {
                timer.Stop();
                txtAutoSelect.Text = "자동 조회";
            }
        }

        private void OnTimedEvent(Object source, EventArgs e)
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

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            Query();
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
                    string strUserId = row["user_id"].ToString();

                    Window child = new DlgUserDetail(strUserId, mw);
                    child.Owner = Application.Current.MainWindow;
                    child.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                    child.ShowDialog();

                    //Dialog화면에서 데이터 변경이 있는경우 다시 조회처리
                    if (child.DialogResult.HasValue && child.DialogResult.Value)
                    {
                        Query();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.mw.borBackground.Visibility = Visibility.Collapsed;
            }
        }
    }
}
