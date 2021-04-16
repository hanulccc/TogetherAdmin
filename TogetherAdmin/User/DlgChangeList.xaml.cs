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

namespace SangAdmin.User
{
    /// <summary>
    /// DlgChangeList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DlgChangeList : Window
    {
        BasePage _page = new BasePage();

        private string strId;
        private string strName;
        private string strType;
        private string strTitle;

        public DlgChangeList(string strId, string strName, string strType, string strTitle)
        {
            InitializeComponent();

            this.strId = strId;
            this.strName = strName;
            this.strType = strType;
            this.strTitle = strTitle;

            SetDefault();
            Query();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            lblUserNm.Content = strName;

            if (strName != null) { lblUserNm.Content += "님"; }

            this.Title = this.strTitle;
            lbTitle.Content = this.strTitle;
        }
        #endregion

        #region [ 조회 ]
        private bool Query()
        {
            try
            {
                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string data = string.Format("proc_type={0}&chg_type={1}&chg_id={2}", "50", this.strType, this.strId);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/admin_info", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable table = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlGrid.ItemsSource = table.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("수정현황 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
