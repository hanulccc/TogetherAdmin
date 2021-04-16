using SangAdmin.Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

namespace SangAdmin.Setting
{
    /// <summary>
    /// ContentMTaxi.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ContentUQna : UserControl
    {
        BasePage _page = new BasePage();
        MainWindow mw;

        private string gUserId = "";
        private string gUserToken = "";

        public ContentUQna(MainWindow mw)
        {
            InitializeComponent();

            this.mw = mw;

            SetDefault();
            SetTimer();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            spChat.Children.Clear();

            gUserId = "";
            gUserToken = "";

            imgSend.Tag = "";

            bdImg.Visibility = Visibility.Collapsed;
            if (bdImg.Visibility == Visibility.Collapsed)
            {
                txtContent.Width = 420;
            }

            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;
        }

        public void Restart()
        {
            cboSearch.SelectedIndex = 0;

            txtSrch.Text = "";
            txtFrDate.SelectedDate = DateTime.Now.AddDays(-30);
            txtToDate.SelectedDate = DateTime.Now;
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

        #region [ 신규 ]
        private void New()
        {
            sPanChat.Children.Clear();
            txtAnswer.Text = "";
        }
        
        private void clear()
        {
            gUserId = "";
            gUserToken = "";
        }

        private void New3()
        {
            spChat.Children.Clear();

            gUserId = "";
            gUserToken = "";
            txtContent.Text = "";
            imgSend.Source = null;
            imgSend.Tag = "";
        }
        #endregion

        #region [ 조회 ]
        private bool Query()
        {
            try
            {
                New();
                clear();

                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string fr_date = "";
                string to_date = "";

                if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }
                if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }

                string data = string.Format("proc_type={0}&start_date={1}&end_date={2}&search_text={3}", "10", fr_date, to_date, txtSrch.Text);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/qna", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable smsTable = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlGrid.ItemsSource = smsTable.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Q/A 데이터 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                ctlGrid.Cursor = Cursors.Arrow;
            }

            return true;
        }
        #endregion

        #region [ 저장 ]
        private bool Save(string strImgFilePath)
        {
            try
            {
                if (gUserId == "")
                {
                    MessageBox.Show("답변하실 드라이버님을 우측 리스트에서 선택하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                if (txtAnswer.Text == "" && strImgFilePath == "")
                {
                    MessageBox.Show("답변 메시지를 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtContent.Focus();
                    return false;
                }

                ctlGrid.Cursor = Cursors.Wait;

                //string strImgFilePath = imgSend.Tag.ToString();
                string strContent = txtAnswer.Text;

                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("proc_type", "20");
                nvc.Add("user_id", gUserId);
                nvc.Add("content", strContent);
                nvc.Add("admin_id", mw.strAdId);
                nvc.Add("admin_name", mw.strAdName);

                string strResult = _page.HttpPostFileData(_page.GetServerUrl + "/admin/qna", strImgFilePath, "img_file", "image/jpeg", nvc);

                JObject jObject = JObject.Parse(strResult); //json 객체로
                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObject["resultMsg"].ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                //리스트 상태를 읽음으로 변경
                for (int row = 0; row < ctlGrid.SelectedItems.Count; row++)
                {
                    DataRowView selItem = ctlGrid.SelectedItems[row] as DataRowView;
                    selItem["is_read"] = "1";
                }

                GetQnAData();
            }
            catch (Exception e)
            {
                MessageBox.Show("QnA 데이터 저장 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                ctlGrid.Cursor = Cursors.Arrow;
            }
            return true;
        }
        #endregion

        #region [ 삭제 ]
        private bool Delete()
        {
            if (ctlGrid.SelectedItems.Count <= 0)
            {
                MessageBox.Show("삭제할 항목을 선택하세요", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            if (MessageBox.Show("선택한 항목을 삭제 하시겠습니까?\n해당 드라이버님과 대화한 모든 글이 삭제됩니다.", "알림", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return false;

            DataSet dsData = null;
            try
            {
                ctlGrid.Cursor = Cursors.Wait;

                for (int row = 0; row < ctlGrid.SelectedItems.Count; row++)
                {
                    DataRowView selItem = ctlGrid.SelectedItems[row] as DataRowView;

                    string strUserId = selItem["user_id"].ToString();

                    string data = string.Format("proc_type={0}&user_id={1}", "30", strUserId);
                    string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/qna", "POST", data);

                    JObject jObject = JObject.Parse(strResult); //json 객체로

                    if (jObject["resultCode"].ToString() != "200")
                    {
                        MessageBox.Show("Q/A 삭제 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

                if (Query() == false) return false;

                New();
                clear();
            }
            catch (Exception e)
            {
                MessageBox.Show("QnA 데이터 삭제 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                if (dsData != null) dsData.Dispose();

                ctlGrid.Cursor = Cursors.Arrow;
            }

            return true;
        }
        #endregion

        #region 채팅방 상세정보
        private void GetQnAData()
        {
            New();
            string data = string.Format("proc_type={0}&user_id={1}", "40", gUserId);
            string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/qna", "GET", data);

            JObject jObject = JObject.Parse(strResult); //json 객체로

            if (jObject["resultCode"].ToString() != "200") { return; }

            JArray jArray = JArray.Parse(jObject["resultData"].ToString());

            foreach(JObject jItem in jArray)
            {
                StackPanel span = new StackPanel();
                Label lb = new Label();
                DockPanel dpan = new DockPanel();
                Border b = new Border();
                TextBlock tbContent = new TextBlock();
                TextBlock tbDate = new TextBlock();

                span.Margin = new Thickness(0,0,0,30);
                lb.Margin = new Thickness(0, 0, 0, 10);
                b.BorderThickness = new Thickness(0);
                tbContent.Margin = new Thickness(15, 10, 15, 10);
                tbContent.Text = jItem["contents"].ToString();
                tbContent.MaxWidth = 345;
                tbContent.TextWrapping = TextWrapping.Wrap;
                tbDate.Text = jItem["insert_dt"].ToString();
                tbDate.VerticalAlignment = VerticalAlignment.Bottom;
                tbDate.FontSize = 11;
                tbDate.Foreground = new SolidColorBrush(Color.FromRgb(182, 184, 184));

                if (jItem["is_reply"].ToString() == "0")
                {
                    // 사용자
                    lb.Content = jItem["user_name"].ToString();
                    b.CornerRadius = new CornerRadius(0,10,10,10);
                    b.Background = new SolidColorBrush(Color.FromRgb(244, 246, 246));
                    b.Child = tbContent;
                    tbDate.Margin = new Thickness(10,0,0,0);

                    dpan.Children.Add(b);
                    dpan.Children.Add(tbDate);
                    span.Children.Add(lb);
                    span.Children.Add(dpan);

                    sPanChat.Children.Add(span);
                }
                else
                {
                    // 관리자
                    span.HorizontalAlignment = HorizontalAlignment.Right;
                    lb.Content = jItem["admin_name"].ToString();
                    lb.HorizontalAlignment = HorizontalAlignment.Right;
                    tbDate.Margin = new Thickness(0, 0, 10, 0);
                    b.CornerRadius = new CornerRadius(10,0,10,10);
                    b.Background = new SolidColorBrush(Color.FromRgb(141, 159, 157));
                    tbContent.Foreground = new SolidColorBrush(Colors.White);
                    b.Child = tbContent;

                    dpan.Children.Add(tbDate);
                    dpan.Children.Add(b);
                    span.Children.Add(lb);
                    span.Children.Add(dpan);

                    sPanChat.Children.Add(span);
                }
            }
        }

        private void GetQnAData2()
        {
            string data = string.Format("proc_type={0}&user_id={1}", "40", gUserId);
            string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/qna", "GET", data);

            JObject jObject = JObject.Parse(strResult); //json 객체로

            if (jObject["resultCode"].ToString() == "200")
            {
                spChat.Children.Clear();
                for (int i = 0; i < jObject["resultData"].Count(); i++)
                {
                    gUserToken = jObject["resultData"][i]["user_token"].ToString();
                    if (jObject["resultData"][i]["is_reply"].ToString() == "0")
                    {
                        DockPanel dpQuestUser = new DockPanel();
                        dpQuestUser.HorizontalAlignment = HorizontalAlignment.Left;

                        Label lblUser = new Label();
                        lblUser.Margin = new Thickness(12, 0, 0, 0);
                        lblUser.Content = jObject["resultData"][i]["user_name"].ToString();

                        Label lblTime = new Label();
                        lblTime.Foreground = new SolidColorBrush(Color.FromArgb(255, 31, 107, 209));
                        lblTime.Content = jObject["resultData"][i]["insert_dt"].ToString();

                        dpQuestUser.Children.Add(lblUser);
                        dpQuestUser.Children.Add(lblTime);

                        Border Quest = new Border();
                        Quest.CornerRadius = new CornerRadius(10);
                        Quest.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 203, 239, 255));
                        Quest.BorderThickness = new Thickness(1, 1, 1, 1);
                        Quest.Background = new SolidColorBrush(Color.FromArgb(255, 203, 239, 255));
                        Quest.Padding = new Thickness(10);
                        Quest.Margin = new Thickness(10, 0, 0, 20);
                        Quest.Padding = new Thickness(10);
                        Quest.HorizontalAlignment = HorizontalAlignment.Left;

                        if (jObject["resultData"][i]["thumb_url"].ToString() == "")
                        {
                            TextBlock textBlock = new TextBlock();//Text 생성
                            textBlock.Text = jObject["resultData"][i]["contents"].ToString();
                            textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                            textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                            Quest.Child = textBlock;
                        }
                        else
                        {
                            string strThumbImage = _page.GetServerUrl + _page.CommAttachFileUrl + jObject["resultData"][i]["thumb_url"].ToString();
                            string strOrgImage = _page.GetServerUrl + _page.CommAttachFileUrl + jObject["resultData"][i]["img_url"].ToString();

                            Image imgQuest = new Image();
                            imgQuest.Width = 150;
                            imgQuest.HorizontalAlignment = HorizontalAlignment.Left;
                            imgQuest.VerticalAlignment = VerticalAlignment.Top;

                            imgQuest.Source = new BitmapImage(new Uri(strThumbImage, UriKind.RelativeOrAbsolute));
                            imgQuest.Cursor = Cursors.Hand;
                            imgQuest.MouseLeftButtonDown += OnImageViewClick;
                            imgQuest.Tag = strOrgImage;

                            Quest.Child = imgQuest;
                        }
                        spChat.Children.Add(dpQuestUser);
                        spChat.Children.Add(Quest);
                    }
                    else
                    {
                        DockPanel dpAdminUser = new DockPanel();
                        dpAdminUser.HorizontalAlignment = HorizontalAlignment.Right;

                        Label lblAdmin = new Label();
                        lblAdmin.Margin = new Thickness(0, 0, 5, 0);
                        lblAdmin.Content = jObject["resultData"][i]["insert_dt"].ToString();

                        Label lblReplyTime = new Label();
                        lblReplyTime.Foreground = new SolidColorBrush(Color.FromArgb(255, 31, 107, 209));
                        lblReplyTime.Content = jObject["resultData"][i]["admin_name"].ToString() + " [ " + jObject["resultData"][i]["admin_id"].ToString() + " ] ";
                        lblReplyTime.ToolTip = "※ 예시) 관리자이름 [ 관리자 ID ]";

                        dpAdminUser.Children.Add(lblReplyTime);
                        dpAdminUser.Children.Add(lblAdmin);

                        Border Reply = new Border();
                        Reply.CornerRadius = new CornerRadius(10);
                        Reply.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                        Reply.BorderThickness = new Thickness(1, 1, 1, 1);
                        Reply.Background = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245));
                        Reply.Padding = new Thickness(10);
                        Reply.Margin = new Thickness(10, 0, 0, 20);
                        Reply.Padding = new Thickness(10);
                        Reply.HorizontalAlignment = HorizontalAlignment.Right;

                        if (jObject["resultData"][i]["thumb_url"].ToString() == "")
                        {

                            TextBlock textBlock = new TextBlock();//Text 생성
                            textBlock.Text = jObject["resultData"][i]["contents"].ToString();
                            textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                            textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Center;

                            Reply.Child = textBlock;

                        }
                        else
                        {

                            string strThumbImage = _page.GetServerUrl + _page.CommAttachFileUrl + jObject["resultData"][i]["thumb_url"].ToString();
                            string strOrgImage = _page.GetServerUrl + _page.CommAttachFileUrl + jObject["resultData"][i]["img_url"].ToString();

                            Image imgReply = new Image();
                            imgReply.Width = 150;
                            imgReply.HorizontalAlignment = HorizontalAlignment.Left;
                            imgReply.VerticalAlignment = VerticalAlignment.Top;
                            imgReply.Source = new BitmapImage(new Uri(strThumbImage, UriKind.RelativeOrAbsolute));
                            imgReply.Cursor = Cursors.Hand;
                            imgReply.MouseLeftButtonDown += OnImageViewClick;
                            imgReply.Tag = strOrgImage;

                            Reply.Child = imgReply;
                        }
                        spChat.Children.Add(dpAdminUser);
                        spChat.Children.Add(Reply);
                    }

                }
                ChatView.ScrollToBottom();

                txtContent.Text = "";
                txtContent.Focus();
                imgSend.Tag = "";
                bdImg.Visibility = Visibility.Collapsed;
                txtContent.Width = 420;
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
                gUserId = row["user_id"].ToString();
                //row["is_read"] = "1";
                GetQnAData();
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
                sfdlg.FileName = "묻고답하기_목록.xls";

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

                ws.Cells[i, 1] = "'드라이버 이름";
                ws.Cells[i, 2] = "'핸드폰 번호";
                ws.Cells[i, 3] = "'묻고답하기";
                ws.Cells[i, 4] = "'마지막작성일";

                i++;

                //엑셀 작성
                foreach (DataRowView row in ctlGrid.Items)
                {
                    ws.Cells[i, 1] = "'" + row["user_name"].ToString();
                    ws.Cells[i, 2] = "'" + row["user_ph"].ToString();
                    ws.Cells[i, 3] = "'" + row["contents"].ToString();
                    ws.Cells[i, 4] = "'" + row["insert_dt"].ToString();
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


        private void txtContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                txtContent.AcceptsReturn = true;
            }

            if (e.Key == Key.Return && !Keyboard.IsKeyDown(Key.LeftShift))
            {
                if (Save("") == false) return;
            }
        }

        private void txtContent_KeyUp(object sender, KeyEventArgs e)
        {
            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                txtContent.AcceptsReturn = false;
            }
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "이미지 파일|*.jpg;*.gif;*.png" };
            if (openFileDialog.ShowDialog() == true)
            {
                /*BitmapImage img = new BitmapImage();
                img.BeginInit();
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                img.DecodePixelWidth = 84;
                img.UriSource = new Uri(openFileDialog.FileName, UriKind.Absolute);
                img.EndInit();

                bdImg.Visibility = Visibility.Visible;
                imgSend.Source = img;
                imgSend.Tag = openFileDialog.FileName;

                txtContent.Width = 325;*/

                Save(openFileDialog.FileName);
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (Save("") == false) return;
        }

        

        private void OnImageViewClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Image img = (Image)sender;

                string Url = img.Tag.ToString();

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

        private void query_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Query();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("선택한 항목을 삭제 하시겠습니까?\n해당 드라이버님과 대화한 모든 글이 삭제됩니다.", "알림", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

            DataSet dsData = null;
            try
            {
                ctlGrid.Cursor = Cursors.Wait;

                foreach (DataRowView row in ctlGrid.Items)
                {
                    if (row["chkYn"].ToString() == "True")
                    {
                        string strUserId = row["user_id"].ToString();

                        string data = string.Format("proc_type={0}&user_id={1}", "30", strUserId);
                        string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/qna", "POST", data);

                        JObject jObject = JObject.Parse(strResult); //json 객체로

                        if (jObject["resultCode"].ToString() != "200")
                        {
                            MessageBox.Show("Q/A 삭제 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                Query();

                New();
                clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("QnA 데이터 삭제 중 오류발생 :" + ex.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (dsData != null) dsData.Dispose();

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
