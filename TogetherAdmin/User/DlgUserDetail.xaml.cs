using SangAdmin.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
using SangAdmin.VirtualAccnt;

namespace SangAdmin.User
{
    /// <summary>
    /// DlgUserDetail.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DlgUserDetail : Window
    {
        BasePage _page = new BasePage();
        MainWindow mw;

        private string gUserId;

        public DlgUserDetail(string strUserId, MainWindow mw)
        {
            InitializeComponent();
            bdMain.MouseLeftButtonDown += (o, e) => DragMove();

            this.gUserId = strUserId;
            this.mw = mw;

            SetDefault();
            Query();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            WritePenalty.Visibility = Visibility.Hidden;

            List<ComboBoxPairs> ComboItemlist = new List<ComboBoxPairs>();
            ComboItemlist.Add(new ComboBoxPairs("승인", "1"));
            ComboItemlist.Add(new ComboBoxPairs("미승인", "0"));

            cboProfileYn.Items.Clear();
            cboProfileYn.SelectedValuePath = "Value";
            cboProfileYn.DisplayMemberPath = "Name";
            cboProfileYn.ItemsSource = ComboItemlist;
            cboProfileYn.SelectedIndex = 1;

            cboLicenseYn.Items.Clear();
            cboLicenseYn.SelectedValuePath = "Value";
            cboLicenseYn.DisplayMemberPath = "Name";
            cboLicenseYn.ItemsSource = ComboItemlist;
            cboLicenseYn.SelectedIndex = 1;

            ComboItemlist = new List<ComboBoxPairs>();
            ComboItemlist.Add(new ComboBoxPairs("가입", "0"));
            ComboItemlist.Add(new ComboBoxPairs("탈퇴", "1"));

            cboLeaveYn.Items.Clear();
            cboLeaveYn.SelectedValuePath = "Value";
            cboLeaveYn.DisplayMemberPath = "Name";
            cboLeaveYn.ItemsSource = ComboItemlist;
            cboLeaveYn.SelectedIndex = 1;

            cboBank.SelectedIndex = 0;
        }
        #endregion

        #region [ 조회 ]
        private bool Query()
        {
            try
            {
                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string data = string.Format("proc_type={0}&user_id={1}", "20", gUserId);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "GET", data);
                int i = 0;

                JObject jObject = JObject.Parse(strResult); //json 객체로
                JObject resultData = new JObject();
                if (jObject["resultCode"].ToString() == "200")
                {
                    resultData = JObject.Parse(jObject["resultData"].ToString());

                    lbUserId.Content = resultData["user_id"].ToString();
                    txtUserId.Text = resultData["user_id"].ToString();
                    lbUserName.Content = resultData["user_name"].ToString();
                    txtUserName.Text = resultData["user_name"].ToString();
                    txtResidentNo.Text = resultData["resident_no"].ToString();
                    txtUserPh.Text = resultData["user_ph"].ToString();
                    txtUserVPh.Text = resultData["user_vph"].ToString();
                    if (txtUserVPh.Text != "")
                        btnVirtNo.Content = "회수";
                    else
                        btnVirtNo.Content = "발급";

                    txtAgent.Text = resultData["agent"].ToString();
                    txtActiveArea1.Text = resultData["active_area1"].ToString();
                    txtActiveArea2.Text = resultData["active_area2"].ToString();
                    txtCareer.Text = resultData["career"].ToString();
                    txtTermsDate.Text = Convert.ToDateTime(resultData["terms_auth_dt"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");

                    foreach(ComboBoxItem item in cboBank.Items)
                    {
                        if (item.Content.ToString() == resultData["account_bank"].ToString())
                        {
                            cboBank.SelectedIndex = i;
                        }
                        i++;
                    }

                    txtAccount.Text = resultData["account"].ToString();
                    cboLeaveYn.SelectedValue = resultData["leave_yn"].ToString();
                    if (resultData["leave_date"].ToString() != "")
                        txtLeaveDate.Text = Convert.ToDateTime(resultData["leave_date"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                    txtLeaveMemo.Text = resultData["leave_memo"].ToString();
                    txtAdminMemo.Text = resultData["admin_memo"].ToString();
                    txtBoNo.Text = resultData["bohum_no"].ToString();
                    txtBoFrDate.Text = resultData["bohum_frdate"].ToString();
                    txtBoToDate.Text = resultData["bohum_todate"].ToString();

                    if (txtAccount.Text != "")
                        btnAccCtl.Content = "회수";
                    else
                        btnAccCtl.Content = "발급";

                    txtLicenseNum.Text = resultData["license_num"].ToString();
                    if (resultData["license_dt"].ToString() != "") { txtLicenseDt.Text = Convert.ToDateTime(resultData["license_dt"].ToString()).ToString("yyy-MM-dd"); }
                    if (resultData["reg_date"].ToString() != "")
                        txtRegDate.Text = Convert.ToDateTime(resultData["reg_date"].ToString()).ToString("yyyy-MM-dd HH:mm:ss");

                    cboProfileYn.SelectedValue = (resultData["profile_yn"].ToString() == "1") ? "1" : "0";
                    cboLicenseYn.SelectedValue = (resultData["license_yn"].ToString() == "1") ? "1" : "0";

                    if (resultData["profile_img"].ToString() != "")
                    {
                        imgProfile.ImageSource = new BitmapImage(new Uri(resultData["profile_img"].ToString(), UriKind.RelativeOrAbsolute));
                        imgProfile.Stretch = Stretch.Fill;
                    }

                    if (resultData["license_img"].ToString() != "")
                    {
                        imgLicense.Source = new BitmapImage(new Uri(resultData["license_img"].ToString(), UriKind.RelativeOrAbsolute));
                        imgLicense.Stretch = Stretch.Fill;
                    }
                }

                //회원 패널티정보 조회
                data = string.Format("proc_type={0}&user_id={1}", "30", gUserId);
                strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "GET", data);

                jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable userPenalty = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlPenalty.ItemsSource = userPenalty.DefaultView;

                }

                //회원 차량정보 조회
                data = string.Format("proc_type={0}&user_id={1}", "40", gUserId);
                strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "GET", data);

                jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200" && JArray.Parse(jObject["resultData"].ToString()).Count > 0)
                {
                    JObject reData = JObject.Parse(jObject["resultData"][0].ToString());

                    reData["bohum_no"] = resultData["bohum_no"];
                    reData["bohum_frdate"] = resultData["bohum_frdate"];
                    reData["bohum_todate"] = resultData["bohum_todate"];

                    DataTable userCarInfo = JsonConvert.DeserializeObject<DataTable>("[" + reData.ToString() + "]");

                    ctlGrid.ItemsSource = userCarInfo.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("회원상세 데이터 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private bool Save()
        {
            try
            {
                ctlGrid.Cursor = Cursors.Wait;

                ComboBoxItem bankItem = cboBank.SelectedItem as ComboBoxItem;
                string bank = bankItem.Content.ToString();


                string data = string.Format("proc_type={0}&user_id={1}&user_name={2}&resident_no={3}&user_ph={4}&user_vph={5}&user_email={6}"
                                        + "&agent={7}&active_area1={8}&active_area2={9}&career={10}&account_bank={11}&account={12}&leave_yn={13}"
                                        + "&leave_memo={14}&admin_memo={15}&license_num={16}&license_dt={17}&profile_yn={18}&license_yn={19}&bohum_no={20}"
                                        + "&bohum_frdate={21}&bohum_todate={22}&admin_id={23}&admin_name={24}"
                                        , "50", gUserId, txtUserName.Text, txtResidentNo.Text, txtUserPh.Text, txtUserVPh.Text, ""
                                        , txtAgent.Text, txtActiveArea1.Text, txtActiveArea2.Text, txtCareer.Text, bank
                                        , txtAccount.Text, cboLeaveYn.SelectedValue.ToString(), txtLeaveMemo.Text, txtAdminMemo.Text
                                        , txtLicenseNum.Text, txtLicenseDt.Text.Replace("-", ""), cboProfileYn.SelectedValue, cboLicenseYn.SelectedValue
                                        , txtBoNo.Text, txtBoFrDate.Text, txtBoToDate.Text, mw.strAdId, mw.strAdName);

                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "POST", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show("회원정보 데이터 저장 중 오류발생!!", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                string strCarOkYn = "";
                string strDocuInfoYn = "";

                //차량정보 업데이트
                foreach (DataRowView row in ctlGrid.Items)
                {
                    string run_yn = row["run_yn"].ToString();
                    string car_num = row["car_num"].ToString();
                    string carok_yn = row["carok_yn"].ToString() == "True" ? "1" : "0";
                    string docu_info_yn = row["docu_info_yn"].ToString() == "True" ? "1" : "0";

                    data = string.Format("proc_type={0}&user_id={1}&carok_yn={2}&docu_info_yn={3}&car_num={4}&admin_id={5}&admin_name={6}", "60", gUserId, carok_yn, docu_info_yn, car_num, mw.strAdId, mw.strAdName);
                    strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "POST", data);

                    jObject = JObject.Parse(strResult); //json 객체로

                    if (jObject["resultCode"].ToString() != "200")
                    {
                        MessageBox.Show("차량정보 데이터 저장 중 오류발생!!", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }

                    if (run_yn == "1" && carok_yn == "1" && docu_info_yn == "1")
                    {
                        strCarOkYn = "1";
                        strDocuInfoYn = "1";
                    }
                }

                //프로필사진,운전면허증사진,차량사진,보험증권,차동차등록증사진 모두 승인경우 최종승인일(ok_date) 업데이트
                string strSQL = "";
                string msg = "";
                if (cboProfileYn.SelectedValue.ToString() == "1" && cboLicenseYn.SelectedValue.ToString() == "1" && strCarOkYn == "1" && strDocuInfoYn == "1")
                {
                    strSQL = "UPDATE helper_info SET ok_date = sysdate() ";
                    strSQL = strSQL + " WHERE user_id = '" + gUserId + "'";
                    strSQL = strSQL + "   AND ok_date is null ";
                    msg = "쉘퍼 승인완료(최종승인일 업데이트)";
                }
                else
                {
                    strSQL = "UPDATE helper_info SET ok_date = null ";
                    strSQL = strSQL + " WHERE user_id = '" + gUserId + "'";
                    msg = "쉘퍼 미승인";
                }

                data = string.Format("proc_type={0}&sql_string={1}&user_id={2}&admin_id={3}&admin_name={4}&msg={5}", "90", strSQL, gUserId, mw.strAdId, mw.strAdName, msg);
                strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "POST", data);

                jObject = JObject.Parse(strResult);

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show("최종승인 데이터 저장 중 오류발생!!", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                MessageBox.Show("저장 되었습니다.", "확인", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
            }
            catch (Exception e)
            {
                MessageBox.Show("회원정보 데이터 저장 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            finally
            {
                ctlGrid.Cursor = Cursors.Arrow;
            }
            return true;
        }
        #endregion

        private void btnChangePage_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn.Tag.ToString() == "choice") { return; }

            setPage(btn.Content.ToString());
        }


        public void setPage(string pageName)
        {
            switch (pageName)
            {
                case "기본 정보":
                    btnDefaultInfo.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnVirtualAccnt.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnShelperInfo.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnDefaultInfo.Tag = "choice";
                    btnVirtualAccnt.Tag = "unchoice";
                    btnShelperInfo.Tag = "unchoice";

                    gridDefaultInfo.Visibility = Visibility.Visible;
                    gridVirtualAccnt.Visibility = Visibility.Collapsed;
                    gridShelperInfo.Visibility = Visibility.Collapsed;
                    break;

                case "계좌 및 보험 정보":
                    btnDefaultInfo.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnVirtualAccnt.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnShelperInfo.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnDefaultInfo.Tag = "unchoice";
                    btnVirtualAccnt.Tag = "choice";
                    btnShelperInfo.Tag = "unchoice";

                    gridDefaultInfo.Visibility = Visibility.Collapsed;
                    gridVirtualAccnt.Visibility = Visibility.Visible;
                    gridShelperInfo.Visibility = Visibility.Collapsed;
                    break;

                case "쉘퍼 정보":
                    btnDefaultInfo.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnVirtualAccnt.Style = Application.Current.FindResource("UnChoiceTab") as Style;
                    btnShelperInfo.Style = Application.Current.FindResource("ChoiceTab") as Style;
                    btnDefaultInfo.Tag = "unchoice";
                    btnVirtualAccnt.Tag = "unchoice";
                    btnShelperInfo.Tag = "choice";

                    gridDefaultInfo.Visibility = Visibility.Collapsed;
                    gridVirtualAccnt.Visibility = Visibility.Collapsed;
                    gridShelperInfo.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string Url = imgProfile.ImageSource.ToString().Replace("TH", "");

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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void btnPenaltySave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtFrDate.Text == "")
                {
                    MessageBox.Show("패널티 시작일을 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (txtToDate.Text == "")
                {
                    MessageBox.Show("패널티 종료일을 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (Convert.ToDateTime(txtFrDate.Text) > Convert.ToDateTime(txtToDate.Text))
                {
                    MessageBox.Show("패널티 시작일이 종료일 보다 큽니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (txtReason.Text == "")
                {
                    MessageBox.Show("사유를 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtReason.Focus();
                    return;
                }

                if (MessageBox.Show("저장 하시겠습니까?", "확인", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                string fr_date = "";
                string to_date = "";

                if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }
                if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }

                //회원 패널티정보 저장
                string data = string.Format("proc_type={0}&user_id={1}&fr_date={2}&to_date={3}&reason={4}&admin_id={5}&admin_name={6}", "70", gUserId, fr_date, to_date, txtReason.Text.Replace("'", "''"), mw.strAdId, mw.strAdName);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "POST", data);

                JObject jObject = JObject.Parse(strResult);

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObject["resultMsg"].ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //회원 패널티정보 조회
                data = string.Format("proc_type={0}&user_id={1}", "30", gUserId);
                strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "GET", data);

                jObject = JObject.Parse(strResult);

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable userPenalty = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlPenalty.ItemsSource = userPenalty.DefaultView;
                }

                WritePenalty.Visibility = Visibility.Collapsed;
                ctlPenalty.Visibility = Visibility.Visible;
                txtReason.Text = "";
            }
            catch
            {
                MessageBox.Show("패널티 입력시 오류발생!!!", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSave4_Click(object sender, RoutedEventArgs e)
        {
            WritePenalty.Visibility = Visibility.Visible;

            ctlPenalty.Visibility = Visibility.Collapsed;
        }

        private void btnPenaltyDel_Click(object sender, RoutedEventArgs e)
        {
            if (ctlPenalty.SelectedItems.Count <= 0)
            {
                MessageBox.Show("삭제할 항목을 선택하세요", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("선택한 항목을 삭제 하시겠습니까?", "알림", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

            try
            {
                string data = "";
                string strResult = "";
                string strSQL = "";
                JObject jObject;
                for (int row = 0; row < ctlPenalty.SelectedItems.Count; row++)
                {
                    DataRowView selItem = ctlPenalty.SelectedItems[row] as DataRowView;

                    string strFrDate = selItem["fr_date"].ToString();
                    string strToDate = selItem["to_date"].ToString();

                    strSQL = "  DELETE FROM user_penalty ";
                    strSQL = strSQL + " WHERE user_id = '" + gUserId + "'";
                    strSQL = strSQL + "   AND fr_date = '" + strFrDate + "'";
                    strSQL = strSQL + "   AND to_date = '" + strToDate + "'";

                    data = string.Format("proc_type={0}&user_id={1}&fr_date={2}&to_date={3}&admin_id={4}&admin_name={5}", "100", gUserId, strFrDate, strToDate, mw.strAdId, mw.strAdName);
                    strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "POST", data);

                    jObject = JObject.Parse(strResult);

                    if (jObject["resultCode"].ToString() != "200")
                    {
                        MessageBox.Show("회원패널티 데이터 삭제 중 오류발생!!", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                //회원 패널티정보 조회
                data = string.Format("proc_type={0}&user_id={1}", "30", gUserId);
                strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "GET", data);

                jObject = JObject.Parse(strResult);

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable userPenalty = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlPenalty.ItemsSource = userPenalty.DefaultView;
                }
            }
            catch
            {
                MessageBox.Show("패널티 데이터 삭제 중 오류발생 :", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDriver_Click(object sender, RoutedEventArgs e)
        {
            borBackground.Visibility = Visibility.Visible;
            this.mw.borBackground.Visibility = Visibility.Visible;
            this.mw.borBackground.Opacity = 0.3;

            Button btn = sender as Button;
            Window child = null;

            if (btn.Name == "btnChange")
            {
                // 드라이버 수정현황
                child = new DlgChangeList(txtUserId.Text, txtUserName.Text, "user", "수정 내역");
            }
            else
            {
                // 드라이버 이용현황
                child = new DlgDriverSrvList(txtUserId.Text, txtUserName.Text);
            }

            child.Owner = Application.Current.MainWindow;
            child.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            child.ShowDialog();

            borBackground.Visibility = Visibility.Collapsed;
            this.mw.borBackground.Opacity = 0.15;
        }

        private void btnAccTrade_Click(object sender, RoutedEventArgs e)
        {
            if (txtAccount.Text == "")
            {
                MessageBox.Show("가상계좌가 없습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            //Window child = new DlgUserAccTrans(gUserId, txtUserName.Text, txtAccount.Text);
            Window child = new DlgUserAccTrans();

            child.Owner = Application.Current.MainWindow;

            child.WindowStartupLocation = WindowStartupLocation.CenterOwner;

            child.ShowDialog();
        }

        private void btnAccCtl_Click(object sender, RoutedEventArgs e)
        {
            string user_id = txtUserId.Text;
            string user_name = txtUserName.Text;
            string user_ph = txtUserPh.Text;
            string account = txtAccount.Text;

            ComboBoxItem bankItem = cboBank.SelectedItem as ComboBoxItem;

            if (btnAccCtl.Content.ToString() == "발급")
            {
                if (MessageBox.Show("가상계좌를 발급 하시겠습니까?", "질문", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                string memo = "재발급계좌";

                string data = string.Format("proc_type={0}&user_id={1}&user_name={2}&user_ph={3}&bank_name={4}&memo={5}", "80", user_id, user_name, user_ph, bankItem.Content.ToString(), memo);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/vraccnt", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show("가상계좌 발급 오류: " + strResult, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("가상계좌를 발급 하였습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (MessageBox.Show("가상계좌를 회수 하시겠습니까?", "질문", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                string memo = "장기미사용으로 인해 회수";

                string data = string.Format("proc_type={0}&user_id={1}&account={2}&memo={3}", "90", user_id, account, memo);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/vraccnt", "POST", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show("가상계좌 회수 오류: " + strResult, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("가상계좌를 회수 하였습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            txtAccount.Text = "";
            Query();
        }

        private void btnShelper_Click(object sender, RoutedEventArgs e)
        {
            Window child = new DlgShelperSrvList(txtUserId.Text, txtUserName.Text);

            child.Owner = Application.Current.MainWindow;
            child.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            child.ShowDialog();
        }

        private void btnVirtNo_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string user_id = txtUserId.Text;
            string user_ph = txtUserPh.Text;
            string user_vph = txtUserVPh.Text;

            if (btn.Content.ToString() == "발급")
            {
                if (MessageBox.Show("가상번호를 발급 하시겠습니까?", "질문", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                string data = string.Format("proc_type={0}&user_id={1}&user_ph={2}", "40", user_id, user_ph);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/vrnum", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show("가상번호 발급 오류: " + strResult, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("가상번호를 발급 하였습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                if (MessageBox.Show("가상번호를 회수 하시겠습니까?", "질문", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No) return;

                string data = string.Format("proc_type={0}&user_id={1}&user_vph={2}", "20", user_id, user_vph);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/vrnum", "POST", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show("가상번호 회수 오류: " + strResult, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("가상번호를 회수 하였습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            txtUserVPh.Text = "";
            Query();
        }

        private void btnCamera_Click(object sender, RoutedEventArgs e)
        {
            // 프로필 사진을 우리가 바꾸면 안될거같음
        }
    }
}
