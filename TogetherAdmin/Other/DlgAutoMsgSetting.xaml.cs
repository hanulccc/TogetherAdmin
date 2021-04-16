using SangAdmin.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
using System.Collections.Specialized;

namespace SangAdmin.Other
{
    /// <summary>
    /// DlgMsgSend.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DlgAutoMsgSetting : Window
    {
        BasePage _page = new BasePage();
        DataRowView drv;
        MainWindow mw;

        public DlgAutoMsgSetting(DataRowView drv, MainWindow mw)
        {
            InitializeComponent();

            this.drv = drv;
            this.mw = mw;

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            bdMain.MouseLeftButtonDown += (o, e) => DragMove();

            if (this.drv != null)
            {
                if (this.drv["send_sms_yn"].ToString() == "Y") { cbSms.IsChecked = true; }
                if (this.drv["send_push_yn"].ToString() == "Y") { cbPush.IsChecked = true; }

                txtReceiveTel1.Text = this.drv["send_sender"].ToString();
                txtTitle.Text = this.drv["send_title"].ToString();
                txtComment.Text = this.drv["send_msg"].ToString();
            }

            Query();
        }
        #endregion

        #region [ 조회 ]
        private void Query()
        {
            try
            {
                string data = string.Format("proc_type={0}", "100");
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/sms", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    JObject jObj = new JObject();
                    jObj["title"] = "선택";
                    JArray jary = JArray.Parse(jObject["resultData"].ToString());
                    jary.AddFirst(jObj);

                    DataTable dt = JsonConvert.DeserializeObject<DataTable>(jary.ToString());

                    cboAuto.DataContext = dt.DefaultView;

                    if (this.drv != null)
                    {
                        int i = 0;
                        foreach (JObject jItem in jary)
                        {
                            if (jItem["title"].ToString() == this.drv["title"].ToString())
                            {
                                cboAuto.SelectedIndex = i;
                                break;
                            }
                            i++;
                        }
                    }
                    else { cboAuto.SelectedIndex = 0; }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("자동문자그룹 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            borBackground.Visibility = Visibility.Visible;
            this.mw.borBackground.Visibility = Visibility.Visible;
            this.mw.borBackground.Opacity = 0.3;

            DataView dataView = cboAuto.DataContext as DataView;
            JArray jArray = JArray.Parse(JsonConvert.SerializeObject(dataView.Table));

            DlgMsgConditionSetting dru = new DlgMsgConditionSetting(jArray);
            dru.ShowDialog();

            Query();

            borBackground.Visibility = Visibility.Collapsed;
            this.mw.borBackground.Opacity = 0.15;
        }
        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            /* if (txtReceiveTel.Text == "") { MessageBox.Show("핸드폰 번호를 입력해주세요."); return; }

             CheckBox cb = new CheckBox();
             cb.Content = txtReceiveTel.Text;
             cb.Margin = new Thickness(0, 10, 0, 0);

             spanTels.Children.Add(cb);*/
        }

        private void btnTelsDelete_Click(object sender, RoutedEventArgs e)
        {
            /*  try
              {
                  int i = 0;

                  foreach (CheckBox cb in spanTels.Children)
                  {
                      if (cb.IsChecked == true)
                      {
                          spanTels.Children.RemoveAt(i);
                      }
                      i++;
                  }
              }
              catch(Exception ex)
              {
                  Console.WriteLine(spanTels.Children.Count);
                  Console.WriteLine(ex);
              }*/
        }



        private void btnReceiveUsers_Click(object sender, RoutedEventArgs e)
        {
            DlgReceiveUsers dru = new DlgReceiveUsers(null);
            dru.ShowDialog();
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
                DataRowView cboData = cboAuto.SelectedItem as DataRowView;
                if (cboData["auto_seq"].ToString() == "")
                {
                    MessageBox.Show("자동 발송을 선택하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (txtComment.Text == "")
                {
                    MessageBox.Show("내용을 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtComment.Focus();
                    return;
                }

                if (txtReceiveTel1.Text == "")
                {
                    MessageBox.Show("발신번호를 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                NameValueCollection nvc = new NameValueCollection();

                if (this.drv == null)
                {
                    nvc.Add("proc_type", "90");
                }
                else
                {
                    nvc.Add("proc_type", "120");
                    nvc.Add("send_seq", this.drv["send_seq"].ToString());
                }

                nvc.Add("auto_type", cboData["auto_seq"].ToString());

                if (cbSms.IsChecked == true) { nvc.Add("send_sms", "Y"); }
                else { nvc.Add("send_sms", "N"); }

                if (cbPush.IsChecked == true) { nvc.Add("send_push", "Y"); }
                else { nvc.Add("send_push", "N"); }

                nvc.Add("sender", txtReceiveTel1.Text);
                nvc.Add("title", txtTitle.Text);
                nvc.Add("msg", txtComment.Text);

                JObject jObject = Api.PostResponseJObject(Api.sms_url, nvc);

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show("전송 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    Console.WriteLine(jObject);
                }
                else
                {
                    MessageBox.Show("저장완료", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
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
    }
}
