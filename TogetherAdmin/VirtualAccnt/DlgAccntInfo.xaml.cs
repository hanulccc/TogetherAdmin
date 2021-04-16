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

namespace SangAdmin.VirtualAccnt
{
    /// <summary>
    /// DlgAccntInfo.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DlgAccntInfo : Window
    {
        BasePage _page = new BasePage();
        MainWindow mw;
        public DlgAccntInfo(MainWindow mw)
        {
            InitializeComponent();

            this.mw = mw;

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            bdMain.MouseLeftButtonDown += (o, e) => DragMove();

            List<ComboBoxPairs> ComboItemlist = new List<ComboBoxPairs>();
            ComboItemlist.Add(new ComboBoxPairs("퍼센트", "p"));
            ComboItemlist.Add(new ComboBoxPairs("원", "w"));

            cboFeeType.Items.Clear();
            cboFeeType.SelectedValuePath = "Value";
            cboFeeType.DisplayMemberPath = "Name";
            cboFeeType.ItemsSource = ComboItemlist;
            cboFeeType.SelectedIndex = 0;

            Query();
        }
        #endregion


        #region [ 조회 ]
        private bool Query()
        {
            try
            {
                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/vraccnt", "GET", "proc_type=30");

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable dt = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"]["groupArr"].ToString());
                    ctlGrid.ItemsSource = dt.DefaultView;

                    cboGroup1.DataContext = dt.DefaultView;
                    cboGroup1.SelectedIndex = 0;

                    cboGroup2.DataContext = dt.DefaultView;
                    cboGroup2.SelectedIndex = 0;

                    dt = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"]["serviceArr"].ToString());
                    ctlGrid2.ItemsSource = dt.DefaultView;

                    cboService.DataContext = dt.DefaultView;
                    cboService.SelectedIndex = 0;

                    dt = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"]["feeArr"].ToString());
                    ctlGrid3.ItemsSource = dt.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("가상계좌 상세 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                ctlGrid.Cursor = Cursors.Arrow;
            }

            return true;
        }
        #endregion


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void chkAll_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            switch (cb.Tag.ToString())
            {
                case "1":
                    foreach (DataRowView row in ctlGrid.Items)
                    {
                        row["chkYn"] = true;
                    }
                    break;
                case "2":
                    foreach (DataRowView row in ctlGrid2.Items)
                    {
                        row["chkYn"] = true;
                    }
                    break;
                case "3":
                    foreach (DataRowView row in ctlGrid3.Items)
                    {
                        row["chkYn"] = true;
                    }
                    break;
            }
        }

        private void chkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            switch (cb.Tag.ToString())
            {
                case "1":
                    foreach (DataRowView row in ctlGrid.Items)
                    {
                        row["chkYn"] = false;
                    }
                    break;
                case "2":
                    foreach (DataRowView row in ctlGrid2.Items)
                    {
                        row["chkYn"] = false;
                    }
                    break;
                case "3":
                    foreach (DataRowView row in ctlGrid3.Items)
                    {
                        row["chkYn"] = false;
                    }
                    break;
            }
        }

        private void btnInsertPop_Click(object sender, RoutedEventArgs e)
        {
            borBackground.Visibility = Visibility.Visible;
            this.mw.borBackground.Visibility = Visibility.Visible;
            this.mw.borBackground.Opacity = 0.3;

            Button btn = sender as Button;

            switch (btn.Tag.ToString())
            {
                case "사용자":
                    lbTitle.Content = "사용자 그룹 추가";
                    lbName.Content = "그룹명";

                    bdAddPop1.Visibility = Visibility.Visible;
                    break;
                case "서비스":
                    lbTitle.Content = "서비스 추가";
                    lbName.Content = "서비스명";

                    bdAddPop1.Visibility = Visibility.Visible;
                    break;
                case "가상계좌":
                    bdAddPop2.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void btnAddPop1Close_Click(object sender, RoutedEventArgs e)
        {
            bdAddPop1.Visibility = Visibility.Collapsed;
            bdAddPop2.Visibility = Visibility.Collapsed;

            borBackground.Visibility = Visibility.Collapsed;
            this.mw.borBackground.Opacity = 0.15;
        }

        private void btnAddPop1Insert_Click(object sender, RoutedEventArgs e)
        {
            NameValueCollection nvc = new NameValueCollection();

            if (lbTitle.Content.ToString() == "사용자 그룹 추가") { nvc.Add("g_type", "그룹"); }
            else { nvc.Add("g_type", "서비스"); }

            nvc.Add("proc_type", "40");
            nvc.Add("g_name", txtGName.Text);
            nvc.Add("g_memo", txtGMemo.Text);

            JObject jObject = Api.PostResponseJObject(Api.vraccnt_url, nvc);

            if (jObject["resultCode"].ToString() != "200")
            {
                MessageBox.Show(lbTitle.Content + " 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            txtGName.Text = "";
            txtGMemo.Text = "";
            bdAddPop1.Visibility = Visibility.Collapsed;

            Query();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            NameValueCollection nvc = new NameValueCollection();
            JObject jObject = new JObject();

            string seq = "";

            switch (btn.Tag.ToString())
            {
                case "사용자":
                    foreach (DataRowView row in ctlGrid.Items)
                    {
                        if (row["chkYn"].ToString() == "True")
                        {
                            if (seq == "") { seq = row["g_seq"].ToString(); }
                            else { seq += "," + row["g_seq"].ToString(); }
                        }
                    }

                    nvc.Add("proc_type", "50");
                    nvc.Add("g_seq", seq);

                    jObject = Api.PostResponseJObject(Api.vraccnt_url, nvc);

                    if (jObject["resultCode"].ToString() != "200")
                    {
                        MessageBox.Show("삭제 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case "서비스":
                    foreach (DataRowView row in ctlGrid2.Items)
                    {
                        if (row["chkYn"].ToString() == "True")
                        {
                            if (seq == "") { seq = row["g_seq"].ToString(); }
                            else { seq += "," + row["g_seq"].ToString(); }
                        }
                    }

                    nvc.Add("proc_type", "50");
                    nvc.Add("g_seq", seq);

                    jObject = Api.PostResponseJObject(Api.vraccnt_url, nvc);

                    if (jObject["resultCode"].ToString() != "200")
                    {
                        MessageBox.Show("삭제 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
                case "가상계좌":
                    foreach (DataRowView row in ctlGrid3.Items)
                    {
                        if (row["chkYn"].ToString() == "True")
                        {
                            if (seq == "") { seq = row["f_seq"].ToString(); }
                            else { seq += "," + row["f_seq"].ToString(); }
                        }
                    }

                    nvc.Add("proc_type", "70");
                    nvc.Add("f_seq", seq);

                    jObject = Api.PostResponseJObject(Api.vraccnt_url, nvc);

                    if (jObject["resultCode"].ToString() != "200")
                    {
                        MessageBox.Show("삭제 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    break;
            }

            Query();
        }

        private void btnAddPop2Insert_Click(object sender, RoutedEventArgs e)
        {
            NameValueCollection nvc = new NameValueCollection();
            DataRowView row = cboGroup1.SelectedItem as DataRowView;
            DataRowView row2 = cboGroup2.SelectedItem as DataRowView;
            DataRowView row3 = cboService.SelectedItem as DataRowView;
            ComboBoxPairs ComboItem = (ComboBoxPairs)cboFeeType.SelectedItem;

            nvc.Add("proc_type", "60");
            nvc.Add("f_group1", row["g_seq"].ToString());
            nvc.Add("f_group2", row2["g_seq"].ToString());
            nvc.Add("f_service", row3["g_seq"].ToString());
            nvc.Add("f_feetype", ComboItem.Value);
            nvc.Add("f_fee", txtFee.Text);
            nvc.Add("f_min", txtMin.Text);
            nvc.Add("f_max", txtMax.Text);
            nvc.Add("f_memo", txtMemo.Text);

            JObject jObject = Api.PostResponseJObject(Api.vraccnt_url, nvc);

            if (jObject["resultCode"].ToString() != "200")
            {
                MessageBox.Show(lbTitle.Content + " 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            txtFee.Text = "";
            txtMin.Text = "";
            txtMax.Text = "";
            txtMemo.Text = "";
            bdAddPop2.Visibility = Visibility.Collapsed;

            Query();
        }
    }
}
