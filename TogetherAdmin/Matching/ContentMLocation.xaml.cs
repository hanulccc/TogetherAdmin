using SangAdmin.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
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
using SangAdmin.Other;
using CefSharp;
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System.IO;

namespace SangAdmin.Matching
{
    /// <summary>
    /// ContentMLocation.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentMLocation : UserControl
    {
        BasePage _page = new BasePage();
        ChromeAPI cAPI;
        MainWindow mw;
        public string data;

        public ContentMLocation(MainWindow mw)
        {
            InitializeComponent();

            this.mw = mw;

            SetDefault();
        }


        #region [ 초기값설정 ]
        private void SetDefault()
        {
            try
            {
                this.cAPI = new ChromeAPI(this);
                chromeBrowser.Address = "http://115.85.182.247:3003/navermap";

                chromeBrowser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
                chromeBrowser.JavascriptObjectRepository.Register("cAPI", cAPI, false, BindingOptions.DefaultBinder);
                txtDistance.Text = "";
            }
            catch(Exception ex)
            {
                MessageBox.Show("사용자 위치 현황 중 오류발생 :" + ex.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(ex);
            }
            

            Query(null, null, null);
        }

        public void Restart()
        {
            txtDistance.Text = "";
        }
        #endregion

        #region [ 조회 ]
        private void Query(string userId, string lat, string lon)
        {
            // 웹이서 함수 통신
            try
            {
                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string strDistance = txtDistance.Text;

                if (strDistance == "")
                {
                    lat = "36.7828003";
                    lon = "127.9942873";
                    strDistance = "5000000";
                }

                string data = string.Format("proc_type={0}&lat={1}&lon={2}&distance={3}", "60", lat, lon, strDistance);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/pickup", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable dt = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());
                    ctlGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("회원현황 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ctlGrid.Cursor = Cursors.Arrow;
            }
        }
        #endregion

        private void btnType_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            switch (btn.Content.ToString())
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

            Query(null, null, null);
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
                sfdlg.FileName = "사용자위치현황_목록.xls";

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

                ws.Cells[i, 1] = "'사용자";
                ws.Cells[i, 2] = "'상태";
                ws.Cells[i, 3] = "'감사포인트";
                ws.Cells[i, 4] = "'현위치";
                ws.Cells[i, 5] = "'도착지";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["user_name"].ToString();
                    ws.Cells[i, 2] = "'" + row["pickup_statusNm"].ToString();
                    ws.Cells[i, 3] = "'" + row["drive_fee"].ToString();
                    ws.Cells[i, 4] = "'" + row["user_addr"].ToString();
                    ws.Cells[i, 5] = "'" + row["end_addr"].ToString();

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

        private void btnMsgSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.mw.borBackground.Visibility = Visibility.Visible;

                bool bolCheck = false;

                JArray jArray = new JArray();

                // 체크가 되어있는지
                foreach (DataRowView row in ctlGrid.Items)
                {
                    if (row["chkYn"].ToString() == "True")
                    {
                        bolCheck = true;

                        JObject jObj = new JObject();
                        jObj["user_phone"] = row["user_phone"].ToString();

                        jArray.Add(jObj);
                    }
                }

                if (bolCheck == false) { MessageBox.Show("메세지를 보낼 유저를 선택하세요."); return; }

                DlgMsgSend dms = new DlgMsgSend(jArray, this.mw);
                dms.ShowDialog();
            }
            finally
            {
                this.mw.borBackground.Visibility = Visibility.Collapsed;
            }
        }

        public void webConnect(string userId, string lat, string lon)
        {
            Query(userId, lat, lon);
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


    internal class ChromeAPI
    {
        ContentMLocation cml;

        public ChromeAPI(ContentMLocation cml)
        {
            this.cml = cml;
        }

        public void showMsg(string userId, string lat, string lon)
        {
            try
            {
                // 웹이서 함수 통신

                this.cml.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate
                {
                    cml.webConnect(userId, lat, lon);
                }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
