using SangAdmin.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
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
using System.Windows.Shapes;
using Microsoft.Win32;

namespace SangAdmin.Other
{
    /// <summary>
    /// DlgMsgSend.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DlgMsgPushSend : Window
    {
        BasePage _page = new BasePage();
        MainWindow mw;
        public DlgMsgPushSend(MainWindow mw)
        {
            InitializeComponent();
            this.mw = mw;
            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            bdMain.MouseLeftButtonDown += (o, e) => DragMove();

            ResetTels();
        }
        #endregion

       
        private void ResetTels()
        {
            spanTels.Children.Clear();
        }
        
        #region 수신자 추가

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            plusReceive();
        }

        private void txtReceiveTel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { plusReceive(); }
        }

        private void plusReceive()
        {
            if (txtReceiveTel.Text == "") { MessageBox.Show("핸드폰 번호를 입력해주세요."); txtReceiveTel.Focus(); return; }

            CheckBox cb = new CheckBox();
            cb.Content = txtReceiveTel.Text;
            cb.Margin = new Thickness(0, 10, 0, 0);

            spanTels.Children.Add(cb);

            txtReceiveTel.Text = "";
        }

        #endregion

        private void btnTelsDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                for (int i = spanTels.Children.Count - 1; i >= 0; i--)
                {
                    CheckBox cb = spanTels.Children[i] as CheckBox;

                    if (cb.IsChecked == true)
                    {
                        spanTels.Children.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }



        private void btnReceiveUsers_Click(object sender, RoutedEventArgs e)
        {
            borBackground.Visibility = Visibility.Visible;
            this.mw.borBackground.Visibility = Visibility.Visible;
            this.mw.borBackground.Opacity = 0.3;

            DlgReceiveUsers dru = new DlgReceiveUsers(null);
            dru.ShowDialog();

            borBackground.Visibility = Visibility.Collapsed;
            this.mw.borBackground.Opacity = 0.15;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void cboMsgBox_Selected(object sender, RoutedEventArgs e)
        {
            /*if (cboMsgBox.SelectedIndex < 2) { return; }

            txtComment.Text = cboMsgBox.SelectedItem.ToString();*/
        }

        private void cboMsgBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /* if (cboMsgBox.SelectedIndex < 1) { return; }

             DataRowView row = cboMsgBox.SelectedItem as DataRowView;

             txtComment.Text = row["msg_content"].ToString();*/
        }

        private void txtComment_TextChanged(object sender, TextChangedEventArgs e)
        {
            /* CheckMsgLen();*/
        }

        private void CheckMsgLen()
        {
            /*int intTotCount = 80;

            try
            {
                char[] msg_chars = this.txtComment.Text.ToCharArray();
                int len = 0;
                int OneByteCnt = 0;
                int TwoByteCnt = 0;
                foreach (char msg_char in msg_chars)
                {
                    if (char.IsDigit(msg_char) || char.IsWhiteSpace(msg_char) || char.IsUpper(msg_char) || char.IsLower(msg_char))
                    {
                        len++;
                        OneByteCnt++;
                    }
                    else
                    {
                        len += 2;

                        TwoByteCnt++;
                    }
                }
                this.txtCount.Text = len.ToString();
                if (len > intTotCount)
                {
                    MessageBox.Show(intTotCount.ToString() + " byte를 초과하여 입력할 수 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);

                    int RemovCnt = ((TwoByteCnt * 2) - 1 + OneByteCnt) - intTotCount;

                    if (RemovCnt <= 0) RemovCnt = 1;

                    this.txtComment.Text = this.txtComment.Text.Remove(txtComment.Text.Length - RemovCnt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("입력 값 계산 오류.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }*/
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if (cbPush.IsChecked == false && cbSms.IsChecked == false)
                {
                    MessageBox.Show("메세지구분을 선택하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (txtComment.Text == "")
                {
                    MessageBox.Show("내용을 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtComment.Focus();
                    return;
                }

                if (spanTels.Children.Count <= 0)
                {
                    MessageBox.Show("수신자를 입력 또는 선택하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtReceiveTel.Focus();
                    return;
                }

                if (MessageBox.Show("전송 하시겠습니까?", "질문", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                string tel_number = "";
                string sMsg = txtComment.Text;

                if (cbSms.IsChecked == true)
                {
                    // 문자발송
                    string strSendUrl = "https://hadmok.tele-pay.kr/tgate/HaMall_SMS/KCT_SMS_HanulMall_Send.asp";
                    string hsw = "A";
                    string bNo = txtSendNo.Text;
                    string mmode = "HPP_SMS";

                    string data = string.Format("proc_type={0}&send_type={1}&send_no={2}&content={3}", "40", txtSendType.Text, bNo, sMsg);
                    string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/sms", "POST", data);

                    JObject jObject = JObject.Parse(strResult); //json 객체로

                    string msg_idx = "";
                    if (jObject["resultCode"].ToString() == "200")
                    {
                        msg_idx = jObject["resultData"]["msg_idx"].ToString();
                    }
                    else
                    {
                        MessageBox.Show("SMS메시지 저장 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }


                    for (int i = 0; i < spanTels.Children.Count; i++)
                    {
                        CheckBox cb = spanTels.Children[i] as CheckBox;

                        tel_number = cb.Content.ToString();

                        Console.WriteLine(Uri.EscapeDataString(sMsg));

                        //SMS발송
                        data = string.Format("hsw={0}&bNo={1}&mmode={2}&sMsg={3}&tel_number={4}", hsw, bNo, mmode, Uri.EscapeDataString(sMsg), tel_number);
                        strResult = _page.HttpSendData(strSendUrl, "POST", data);

                        //JObject jObject = JObject.Parse(strResult); //json 객체로

                        string data2 = "";
                        if (strResult.Contains("false") == true)
                        {
                            data2 = string.Format("proc_type={0}&msg_idx={1}&tel_number={2}&result={3}", "50", msg_idx, tel_number, "실패");
                        }
                        else
                        {
                            data2 = string.Format("proc_type={0}&msg_idx={1}&tel_number={2}&result={3}", "50", msg_idx, tel_number, "성공");
                        }
                        string strResult2 = _page.HttpSendData(_page.GetServerUrl + "/admin/sms", "POST", data2);
                    }
                }
                
                if (cbPush.IsChecked == true) 
                {
                    // 푸쉬발송
                    string token = "";
                    string userId = "";

                    for (int i = 0; i < spanTels.Children.Count; i++)
                    {
                        CheckBox cb = spanTels.Children[i] as CheckBox;

                        if (i == 0) { tel_number = cb.Content.ToString(); }
                        else { tel_number += "#" + cb.Content.ToString(); }
                    }

                    NameValueCollection nv = new NameValueCollection();
                    nv.Add("proc_type", "80");
                    nv.Add("phone", tel_number);

                    JObject jObject = Api.PostResponseJObject(Api.push_url, nv);
                    if (jObject == null) { return; }
                    JArray jArray = JArray.Parse(jObject["resultData"].ToString());

                    for (int i = 0; i < jArray.Count; i++)
                    {
                        JObject jItem = JObject.Parse(jArray[i].ToString());
                        if (i == 0)
                        {
                            token = jItem["user_token"].ToString();
                            userId = jItem["user_id"].ToString();
                        }
                        else
                        {
                            token += "#" + jItem["user_token"].ToString();
                            userId += "#" + jItem["user_id"].ToString();
                        }
                    }

                    nv = new NameValueCollection();
                    nv.Add("token", token);
                    nv.Add("receiver_id", userId);
                    nv.Add("title", txtTitle.Text);
                    nv.Add("body", sMsg);
                    nv.Add("memo", "관리자 프로그램에서 발송");

                    jObject = Api.PostResponseJObject("http://106.10.53.206:8083/together/send", nv);

                    if (jObject["code"].ToString() != "200") { MessageBox.Show("푸쉬전송 실패", "오류", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                }

                MessageBox.Show("전송완료", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("문자보내기 중 오류발생: " + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "이미지 파일|*.jpg;*.gif;*.png" };
            if (openFileDialog.ShowDialog() == true)
            {
                txtFileName.Text = openFileDialog.FileName;
            }
        }

        private void txtContent_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                char[] msg_chars = this.txtComment.Text.ToCharArray();
                int len = 0;
                int OneByteCnt = 0;
                int TwoByteCnt = 0;
                foreach (char msg_char in msg_chars)
                {
                    if (char.IsDigit(msg_char) || char.IsWhiteSpace(msg_char) || char.IsUpper(msg_char) || char.IsLower(msg_char))
                    {
                        len++;
                        OneByteCnt++;
                    }
                    else
                    {
                        len += 2;

                        TwoByteCnt++;
                    }
                }
                this.txtCount.Text = len.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("입력 값 계산 오류.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }
}
