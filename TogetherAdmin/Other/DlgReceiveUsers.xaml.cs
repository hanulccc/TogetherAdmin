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

namespace SangAdmin.Other
{
    /// <summary>
    /// DlgReceiveUsers.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DlgReceiveUsers : Window
    {
        BasePage _page = new BasePage();
        DlgMsgSend dlgMsgSend;

        public DlgReceiveUsers(DlgMsgSend dms)
        {
            InitializeComponent();

            this.dlgMsgSend = dms;

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            bdMessage.MouseLeftButtonDown += (o, e) => DragMove();

            Query();
        }
        #endregion

        #region [ 조회 ]
        private void Query()
        {
            DataSet dsData = null;
            try
            {
                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string userType = "";

                if (lbTypeDriver.Visibility  == Visibility.Visible) { userType = "드라이버"; }
                else if (lbTypeShelper.Visibility == Visibility.Visible) { userType = "쉘퍼"; }

                //SMS 수신자 선택 목록 조회
                string data = string.Format("proc_type={0}&user_type={1}&srch_user={2}", "70", userType, txtSrch.Text);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/sms", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable dt = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());
                    ctlGrid.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show("회원 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (dsData != null) dsData.Dispose();
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
            Query();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnUserSend_Click(object sender, RoutedEventArgs e)
        {
            foreach (DataRowView row in ctlGrid.Items)
            {
                if (row["chkYn"].ToString() == "True")
                {
                    string user_name = row["user_name"].ToString();
                    string user_ph = row["user_ph"].ToString();

                    for (int i = 0; i < dlgMsgSend.spanTels.Children.Count; i++)
                    {
                        CheckBox cb = dlgMsgSend.spanTels.Children[i] as CheckBox;

                        if (cb.Content.ToString() == user_ph)
                        {
                            MessageBox.Show(user_name + "님은 이미 선택 하셨습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }
                    }

                    CheckBox checkBox = new CheckBox();
                    checkBox.Content = user_ph;
                    checkBox.Margin = new Thickness(0, 10, 0, 0);

                    dlgMsgSend.spanTels.Children.Add(checkBox);
                }
            }
            this.Close();
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
           Query();
        }

        private void txtSrch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { Query(); }
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
