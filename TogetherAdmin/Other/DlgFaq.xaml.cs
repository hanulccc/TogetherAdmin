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
    public partial class DlgFaq : Window
    {
        BasePage _page = new BasePage();
        MainWindow mw;
        DataRowView data;

        string idx = "";


        public DlgFaq(MainWindow mw, DataRowView data)
        {
            InitializeComponent();

            this.mw = mw;
            this.data = data;

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            bdMessage.MouseLeftButtonDown += (o, e) => DragMove();

            List<ComboBoxPairs> ComboItemlist = new List<ComboBoxPairs>();
            ComboItemlist.Add(new ComboBoxPairs("가입", "가입"));
            ComboItemlist.Add(new ComboBoxPairs("매칭", "매칭"));
            ComboItemlist.Add(new ComboBoxPairs("취소/지연", "취소/지연"));
            ComboItemlist.Add(new ComboBoxPairs("기타", "기타"));

            cboCategory.Items.Clear();
            cboCategory.SelectedValuePath = "Value";
            cboCategory.DisplayMemberPath = "Name";
            cboCategory.ItemsSource = ComboItemlist;
            cboCategory.SelectedIndex = 0;



            if (data != null)
            {
                this.idx = data["idx"].ToString();
                txtTitle.Text = data["title"].ToString();
                txtContent.Text = data["content"].ToString();

                int i = 0;

                foreach (ComboBoxPairs c in ComboItemlist)
                {
                    if (c.Name == data["category"].ToString())
                    {
                        cboCategory.SelectedIndex = i;
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

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                ComboBoxPairs ComboItem = (ComboBoxPairs)cboCategory.SelectedItem;
                string strCategory = ComboItem.Value;

                if (strCategory == "")
                {
                    MessageBox.Show("FAQ 카테고리를 선택하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (txtTitle.Text == "")
                {
                    MessageBox.Show("질문을 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtTitle.Focus();
                    return;
                }
                if (txtContent.Text == "")
                {
                    MessageBox.Show("내용을 입력하세요.", "알림", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtContent.Focus();
                    return;
                }


                if (this.idx == "")
                {
                    string data = string.Format("proc_type={0}&category={1}&title={2}&content={3}&admin_id={4}&admin_name={5}", "20", strCategory, txtTitle.Text.Replace("'", "''"), txtContent.Text.Replace("'", "''"), mw.strAdId, mw.strAdName);
                    string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/faq", "POST", data);

                    JObject jObject = JObject.Parse(strResult); //json 객체로

                    if (jObject["resultCode"].ToString() != "200")
                    {
                        MessageBox.Show("FAQ 저장 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    string data = string.Format("proc_type={0}&idx={1}&category={2}&title={3}&content={4}&admin_id={5}&admin_name={6}", "30", this.idx, strCategory, txtTitle.Text.Replace("'", "''"), txtContent.Text.Replace("'", "''"), mw.strAdId, mw.strAdName);
                    string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/faq", "POST", data);

                    JObject jObject = JObject.Parse(strResult); //json 객체로

                    if (jObject["resultCode"].ToString() != "200")
                    {
                        MessageBox.Show("FAQ 수정 중 오류발생", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("FAQ 데이터 저장 중 오류발생 :" + ex.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                this.Close();
            }
        }
    }
}
