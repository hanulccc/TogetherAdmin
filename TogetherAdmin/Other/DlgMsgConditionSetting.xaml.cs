using Newtonsoft.Json.Linq;
using SangAdmin.Common;
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

namespace SangAdmin.Other
{
    /// <summary>
    /// DlgMsgConditionSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DlgMsgConditionSetting : Window
    {
        JArray jArray;

        public DlgMsgConditionSetting(JArray jArray)
        {
            InitializeComponent();

            this.jArray = jArray;

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            bdMessage.MouseLeftButtonDown += (o, e) => DragMove();

            ResetTels();

            this.jArray.RemoveAt(0);

            foreach (JObject jObject in this.jArray)
            {
                plusReceive(jObject["title"].ToString());
            }
        }

        private void ResetTels()
        {
            spanTels.Children.Clear();
        }
        #endregion

        #region 발송조건 추가

        private void btnPlus_Click(object sender, RoutedEventArgs e)
        {
            if (txtReceiveTel.Text == "") { MessageBox.Show("핸드폰 번호를 입력해주세요."); txtReceiveTel.Focus(); return; }

            plusReceive(txtReceiveTel.Text);
        }

        private void txtReceiveTel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtReceiveTel.Text == "") { MessageBox.Show("핸드폰 번호를 입력해주세요."); txtReceiveTel.Focus(); return; }

                plusReceive(txtReceiveTel.Text);
            }
        }

        private void plusReceive(string content)
        {
            CheckBox cb = new CheckBox();
            cb.Content = content;
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


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                bool success = true;

                for(int i = 0; i < spanTels.Children.Count; i++)
                {
                    CheckBox cb = spanTels.Children[i] as CheckBox;
                    bool isAlready = false;

                    foreach(JObject jObject in this.jArray)
                    {
                        if (jObject["title"].ToString() == cb.Content.ToString()) { isAlready = true; break; }
                    }

                    if (isAlready == false)
                    {
                        NameValueCollection nv = new NameValueCollection();
                        nv.Add("proc_type", "110");
                        nv.Add("method", "IN");
                        nv.Add("value", cb.Content.ToString());

                        JObject jObj = Api.PostResponseJObject(Api.sms_url, nv);
                        if (jObj["resultCode"].ToString() != "200") { success = false; }
                    }
                }

                if (success == false) { MessageBox.Show("조건저장 실패", "오류", MessageBoxButton.OK, MessageBoxImage.Error); return; }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("조건저장 중 오류발생: " + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
    }
}
