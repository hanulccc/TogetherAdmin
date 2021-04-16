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
    /// DlgMsgSend.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DlgNotiCall : Window
    {
        BasePage _page = new BasePage();
        MainWindow mw;
        DataRowView drv;

        public DlgNotiCall(MainWindow mw, DataRowView drv)
        {
            InitializeComponent();
            this.mw = mw;
            this.drv = drv;
            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            bdMain.MouseLeftButtonDown += (o, e) => DragMove();

            List<ComboBoxPairs> ComboItemlist = new List<ComboBoxPairs>();
            ComboItemlist.Add(new ComboBoxPairs("드라이버에게문자", "드라이버에게문자"));
            ComboItemlist.Add(new ComboBoxPairs("쉘퍼에게문자", "쉘퍼"));
            ComboItemlist.Add(new ComboBoxPairs("하차사유", "쉘퍼에게문자"));
            ComboItemlist.Add(new ComboBoxPairs("본사전달", "본사전달"));

            cboSrchCategory.Items.Clear();
            cboSrchCategory.SelectedValuePath = "Value";
            cboSrchCategory.DisplayMemberPath = "Name";
            cboSrchCategory.ItemsSource = ComboItemlist;
            cboSrchCategory.SelectedIndex = 0;

            if (this.drv != null)
            {
                txtComment.Text = drv["msg"].ToString();

                int i = 0;

                foreach (ComboBoxPairs c in ComboItemlist)
                {
                    if (c.Name == drv["category"].ToString())
                    {
                        cboSrchCategory.SelectedIndex = i;
                    }
                    i++;
                }
            }

        }
        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string strCategory = cboSrchCategory.SelectedValue.ToString();

                if (strCategory == "")
                {
                    MessageBox.Show("안내콜 구분을 선택하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (txtComment.Text == "")
                {
                    MessageBox.Show("내용을 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtComment.Focus();
                    return;
                }

                Mouse.OverrideCursor = Cursors.Wait;

                if (this.drv == null)
                {
                    string data = string.Format("proc_type={0}&category={1}&msg={2}&admin_id={3}&admin_name={4}", "20", strCategory, txtComment.Text.Replace("'", "''"), this.mw.strAdId, this.mw.strAdName);
                    string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/infocall", "POST", data);

                    JObject jObject = JObject.Parse(strResult); //json 객체로

                    if (jObject["resultCode"].ToString() != "200")
                    {
                        MessageBox.Show("안내콜 저장 중 오류.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    string data = string.Format("proc_type={0}&idx={1}&category={2}&msg={3}&admin_id={4}&admin_name={5}", "30", this.drv["idx"].ToString(), strCategory, txtComment.Text.Replace("'", "''"), this.mw.strAdId, this.mw.strAdName);
                    string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/infocall", "POST", data);

                    JObject jObject = JObject.Parse(strResult); //json 객체로

                    if (jObject["resultCode"].ToString() != "200")
                    {
                        MessageBox.Show("안내콜 수정 중 오류.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("콜 메시지 데이터 저장 중 오류발생 :" + ex.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }
    }
}
