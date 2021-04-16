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
    /// DlgShelperSrvList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DlgShelperSrvList : Window
    {
        BasePage _page = new BasePage();

        private string gUserId;
        private string gUserNm;

        public DlgShelperSrvList(string strUserId, string strUserNm)
        {
            InitializeComponent();

            bdMain.MouseLeftButtonDown += (o, e) => DragMove();

            gUserId = strUserId;
            gUserNm = strUserNm;

            SetDefault();

            Query();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            ctlGrid.RowHeaderWidth = 40;

            lblUserNm.Content = gUserNm;
        }
        #endregion

        #region [ 조회 ]
        private bool Query()
        {
            try
            {
                ctlGrid.Cursor = Cursors.Wait;
                ctlGrid.ItemsSource = null;

                string data = string.Format("proc_type={0}&user_id={1}", "110", gUserId);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    DataTable userTable = JsonConvert.DeserializeObject<DataTable>(jObject["resultData"].ToString());

                    ctlGrid.ItemsSource = userTable.DefaultView;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("쉘퍼 이용현황 조회 중 오류발생 :" + e.Message.ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
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
