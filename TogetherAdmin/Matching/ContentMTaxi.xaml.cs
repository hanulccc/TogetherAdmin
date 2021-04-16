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
using System.Windows.Threading;
using Excel = Microsoft.Office.Interop.Excel;

namespace SangAdmin.Matching
{
    /// <summary>
    /// ContentMTaxi.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentMTaxi : UserControl
    {
        BasePage _page = new BasePage();

        public ContentMTaxi()
        {
            InitializeComponent();

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            spChat.Children.Clear();

            txtFrDate.SelectedDate = DateTime.Now.AddDays(-21);
            txtToDate.SelectedDate = DateTime.Now;

            cboSearch.SelectedIndex = 0;
            txtSrch.Text = "";

            Sign();
            Query();
            SetTimer();
            newChat();
        }

        public void Restart()
        {
            spChat.Children.Clear();

            txtFrDate.SelectedDate = DateTime.Now.AddDays(-21);
            txtToDate.SelectedDate = DateTime.Now;

            cboSearch.SelectedIndex = 0;
            txtSrch.Text = "";

            Sign();
            Query();
            newChat();
        }

        private void Sign()
        {
            try
            {
                string data = string.Format("proc_type={0}", "30");
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/chat", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show("채팅방현황판 조회 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                txtTodayCnt.Text = jObject["resultData"][0]["today_cnt"].ToString();
                txtNowRoomCnt.Text = jObject["resultData"][0]["now_room_cnt"].ToString();
                txtNowUserCnt.Text = jObject["resultData"][0]["now_user_cnt"].ToString();
            }
            catch (Exception e)
            {
                MessageBox.Show("채팅방현황판 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region [ 타이머설정  ]
        private void SetTimer()
        {
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 60);
            dispatcherTimer.Start();
        }

        protected void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            Query();
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

                string data = string.Format("proc_type={0}&start_date={1}&end_date={2}&search_text={3}", "10", fr_date, to_date, txtSrch.Text);
                string strResult = _page.HttpSendData(Api.serverURL + "/admin/chat", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable smsTable = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlGrid.ItemsSource = smsTable.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Chat 데이터 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                newChat();
                ctlGrid.Cursor = Cursors.Arrow;
            }
        }
        #endregion

        #region 채팅방 상세정보
        private void GetChatData(string strChatId)
        {
            try
            {
                newChat();

                string data = string.Format("proc_type={0}&chat_id={1}", "20", strChatId);
                string strResult = _page.HttpSendData(Api.serverURL + "/admin/chat", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200") { return; }

                DataTable dt = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());
                lboxChatting.ItemsSource = dt.DefaultView;

                JArray jArray = JArray.Parse(jObject["resultData2"].ToString());
                int i = 0;


                foreach (JObject jItem in jArray)
                {
                    Grid g = new Grid();
                    Border b = new Border();
                    Ellipse e = new Ellipse();
                    ImageBrush ib = new ImageBrush();

                    g.HorizontalAlignment = HorizontalAlignment.Right;
                    b.Width = 50;
                    b.Background = Brushes.Transparent;
                    b.BorderBrush = Brushes.White;
                    b.BorderThickness = new Thickness(4);
                    b.CornerRadius = new CornerRadius(40);
                    e.Margin = new Thickness(4);

                    ib.ImageSource = new BitmapImage(new Uri(jItem["profile_img"].ToString()));

                    b.BorderThickness = new Thickness(0);
                    b.Background = Brushes.White;
                    e.Fill = ib;
                    g.Margin = new Thickness(0, 0, 36 * i, 0);
                    g.Children.Add(b);
                    g.Children.Add(e);

                    if (i == 0)
                    {
                        txtChatName.Content = jItem["user_name"].ToString();
                    }
                    else
                    {
                        txtChatName.Content += ", " + jItem["user_name"].ToString();
                    }

                    gridProfile.Children.Insert(0, g);

                    i++;
                }

                txtChatCount.Text = "참여인원 " + i + "명";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void newChat()
        {
            txtChatName.Content = "";
            txtChatCount.Text = "참여인원 0명";
            gridProfile.Children.Clear();
            lboxChatting.ItemsSource = null;
        }

        private void GetChatData2(string strChatId)
        {
            string data = string.Format("proc_type={0}&chat_id={1}", "20", strChatId);
            string strResult = _page.HttpSendData(Api.serverURL + "/admin/chat", "GET", data);

            JObject jObject = JObject.Parse(strResult); //json 객체로

            if (jObject["resultCode"].ToString() == "200")
            {
                spChat.Children.Clear();

                string memberlist = jObject["resultData2"].ToString();
                memberlist = memberlist.Replace("[\r\n", "");
                memberlist = memberlist.Replace("\r\n]", "");
                txtMemberList.Text = "현재 참여자:\n" + memberlist;

                for (int i = 0; i < jObject["resultData"].Count(); i++)
                {
                    if (jObject["resultData"][i]["leader_yn"].ToString() == "Y")
                    {
                        DockPanel dpChatLeader = new DockPanel();
                        dpChatLeader.HorizontalAlignment = HorizontalAlignment.Left;

                        Label lblUser = new Label();
                        lblUser.Margin = new Thickness(12, 0, 0, 0);
                        lblUser.Content = jObject["resultData"][i]["user_name"].ToString();

                        Label lblTime = new Label();
                        lblTime.Foreground = new SolidColorBrush(Color.FromArgb(255, 31, 107, 209));
                        lblTime.Content = jObject["resultData"][i]["chat_time"].ToString();

                        dpChatLeader.Children.Add(lblUser);
                        dpChatLeader.Children.Add(lblTime);

                        Border ChatBorder = new Border();
                        ChatBorder.CornerRadius = new CornerRadius(10);
                        ChatBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 203, 239, 255));
                        ChatBorder.BorderThickness = new Thickness(1, 1, 1, 1);
                        ChatBorder.Background = new SolidColorBrush(Color.FromArgb(255, 203, 239, 255));
                        ChatBorder.Padding = new Thickness(10);
                        ChatBorder.Margin = new Thickness(10, 0, 0, 20);
                        ChatBorder.Padding = new Thickness(10);
                        ChatBorder.HorizontalAlignment = HorizontalAlignment.Left;

                        TextBlock textBlock = new TextBlock();//Text 생성
                        textBlock.Text = jObject["resultData"][i]["content"].ToString();
                        textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                        ChatBorder.Child = textBlock;

                        spChat.Children.Add(dpChatLeader);
                        spChat.Children.Add(ChatBorder);
                    }
                    else
                    {
                        DockPanel dpChatUser = new DockPanel();
                        dpChatUser.HorizontalAlignment = HorizontalAlignment.Right;

                        Label lblUser = new Label();
                        lblUser.Margin = new Thickness(0, 0, 5, 0);
                        lblUser.Content = jObject["resultData"][i]["user_name"].ToString();

                        Label lblReplyTime = new Label();
                        lblReplyTime.Foreground = new SolidColorBrush(Color.FromArgb(255, 31, 107, 209));
                        lblReplyTime.Content = jObject["resultData"][i]["chat_time"].ToString();

                        dpChatUser.Children.Add(lblReplyTime);
                        dpChatUser.Children.Add(lblUser);

                        Border ChatBorder = new Border();
                        ChatBorder.CornerRadius = new CornerRadius(10);
                        ChatBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                        ChatBorder.BorderThickness = new Thickness(1, 1, 1, 1);
                        ChatBorder.Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                        ChatBorder.Padding = new Thickness(10);
                        ChatBorder.Margin = new Thickness(10, 0, 0, 20);
                        ChatBorder.Padding = new Thickness(10);
                        ChatBorder.HorizontalAlignment = HorizontalAlignment.Right;

                        TextBlock textBlock = new TextBlock();//Text 생성
                        textBlock.Text = jObject["resultData"][i]["content"].ToString();
                        textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                        textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                        ChatBorder.Child = textBlock;

                        spChat.Children.Add(dpChatUser);
                        spChat.Children.Add(ChatBorder);
                    }
                }
                ChatView.ScrollToBottom();
            }
        }
        #endregion

        private void txtSrch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { Query(); }
        }

        private void ctlGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = (DataGrid)sender;

            DataRowView row = dg.SelectedItem as DataRowView;
            if (row != null)
            {
                GetChatData(row["chat_id"].ToString());
            }
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
                sfdlg.FileName = "택시동승현황_목록.xls";

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

                ws.Cells[i, 1] = "'개설자 이름";
                ws.Cells[i, 2] = "'개설자 ID";
                ws.Cells[i, 3] = "'연락처";
                ws.Cells[i, 4] = "'채팅방 생성일";
                ws.Cells[i, 5] = "'참여인원";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["user_name"].ToString();
                    ws.Cells[i, 2] = "'" + row["user_id"].ToString();
                    ws.Cells[i, 3] = "'" + row["user_ph"].ToString();
                    ws.Cells[i, 4] = "'" + row["publish_date"].ToString();
                    ws.Cells[i, 5] = "'" + row["cnt"].ToString();
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

        private void BorMember_MouseEnter(object sender, MouseEventArgs e)
        {
            txtFront.Visibility = Visibility.Collapsed;
            txtMemberList.Visibility = Visibility.Visible;
        }

        private void BorMember_MouseLeave(object sender, MouseEventArgs e)
        {
            txtFront.Visibility = Visibility.Visible;
            txtMemberList.Visibility = Visibility.Collapsed;
        }

    }
}
