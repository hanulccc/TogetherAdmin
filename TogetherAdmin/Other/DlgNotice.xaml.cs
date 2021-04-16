using SangAdmin.Common;
using Microsoft.Win32;
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

namespace SangAdmin.Other
{
    /// <summary>
    /// DlgMsgSend.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DlgNotice : Window
    {
        BasePage _page = new BasePage();
        DataRowView drv = null;
        MainWindow mw;
        string idx = "";

        public DlgNotice(DataRowView drv, MainWindow mw)
        {
            InitializeComponent();

            this.drv = drv;
            this.mw = mw;

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            bdMessage.MouseLeftButtonDown += (o, e) => DragMove();

            if (drv != null)
            {
                this.idx = drv["idx"].ToString();
                txtTitle.Text = drv["title"].ToString();
                txtContent.Text = drv["content"].ToString();
                // txtFileName.Text = drv["file_name"].ToString();
                txtFrDate.Text = drv["popup_frdate"].ToString();
                txtToDate.Text = drv["popup_todate"].ToString();

                if (drv["is_popup"].ToString() == "true")
                {
                    rdoPopUpYes.IsChecked = true;
                }
            }
        }
        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                string fr_date = "";
                string to_date = "";
                string is_popup;

                if (txtTitle.Text == "")
                {
                    MessageBox.Show("제목을 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtTitle.Focus();
                    return;
                }

                if (txtContent.Text == "")
                {
                    MessageBox.Show("내용을 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtContent.Focus();
                    return;
                }

                if (rdoPopUpYes.IsChecked == true)
                {
                    if (txtFrDate.Text == "" || txtToDate.Text == "")
                    {
                        MessageBox.Show("팝업 기간을 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                    is_popup = "1";
                }
                else
                {
                    is_popup = "0";
                    txtFrDate.Text = "";
                    txtToDate.Text = "";
                }

                if (txtFrDate.Text != "") { fr_date = txtFrDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }
                if (txtToDate.Text != "") { to_date = txtToDate.SelectedDate.Value.ToString("yyyy-MM-dd"); }

                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("title", txtTitle.Text);
                nvc.Add("content", txtContent.Text);
                nvc.Add("is_popup", is_popup);
                nvc.Add("start_date", fr_date);
                nvc.Add("end_date", to_date);
                nvc.Add("admin_id", mw.strAdId);
                nvc.Add("admin_name", mw.strAdName);
                nvc.Add("idx", this.idx);

                if (this.idx == "")
                {
                    nvc.Add("proc_type", "20");
                    string strResult = _page.HttpPostFileData(_page.GetServerUrl + "/admin/notice", txtFileName.Text, "img_file", "image/jpeg", nvc);

                    JObject jObject = JObject.Parse(strResult); //json 객체로
                    if (jObject["resultCode"].ToString() != "200")
                    {
                        MessageBox.Show("공지사항 등록 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    nvc.Add("proc_type", "30");

                    string strResult = _page.HttpPostFileData(_page.GetServerUrl + "/admin/notice", txtFileName.Text, "img_file", "image/jpeg", nvc);

                    JObject jObject = JObject.Parse(strResult); //json 객체로
                    if (jObject["resultCode"].ToString() != "200")
                    {
                        MessageBox.Show("공지사항 수정 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                MessageBox.Show("저장 되었습니다.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show("공지사항 데이터 저장 중 오류발생.!!", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                this.Close();
            }

            return;
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "이미지 파일|*.jpg;*.gif;*.png" };
            if (openFileDialog.ShowDialog() == true)
            {
                txtFileName.Text = openFileDialog.FileName;
            }
        }
    }
}
