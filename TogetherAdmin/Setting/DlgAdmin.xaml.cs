using SangAdmin.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

namespace SangAdmin.Setting
{
    /// <summary>
    /// DlgAdmin.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DlgAdmin : Window
    {
        private string key = "sangsang_54540123456789012345678";
        private string iv = "sang_54540123456";

        DataRowView drv = null;

        public DlgAdmin(DataRowView drv)
        {
            InitializeComponent();

            this.drv = drv;

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            bdMain.MouseLeftButtonDown += (o, e) => DragMove();

            if (drv != null)
            {
                txtId.IsReadOnly = true;
                txtId.Text = drv["admin_id"].ToString();
                txtName.Text = drv["admin_name"].ToString();
                cboPower.SelectedIndex = int.Parse(drv["admin_power"].ToString());
            }
        }
        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (drv == null)
            {
                // 관리자 등록
                NameValueCollection nv = new NameValueCollection();
                nv.Add("proc_type", "30");
                nv.Add("admin_id", txtId.Text);
                nv.Add("admin_pwd", AESEncrypt(pwd.Password));
                nv.Add("admin_name", txtName.Text);
                nv.Add("admin_power", cboPower.SelectedIndex.ToString());

                JObject jObject = Api.PostResponseJObject(Api.adminInfo_url, nv);

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObject["resultMsg"].ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("등록되었습니다.");
            }
            else
            {
                // 관리자 수정
                NameValueCollection nv = new NameValueCollection();
                nv.Add("proc_type", "60");
                nv.Add("admin_id", txtId.Text);
                nv.Add("admin_pwd", AESEncrypt(pwd.Password));
                nv.Add("admin_name", txtName.Text);
                nv.Add("admin_power", cboPower.SelectedIndex.ToString());

                JObject jObject = Api.PostResponseJObject(Api.adminInfo_url, nv);

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObject["resultMsg"].ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("수정되었습니다.");
            }
            this.Close();
        }

        private string AESEncrypt(string input)
        {
            try
            {
                RijndaelManaged aes = new RijndaelManaged();
                aes.KeySize = 256; //AES256으로 사용시 
                //aes.KeySize = 128; //AES128로 사용시 
                aes.BlockSize = 128;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = Encoding.UTF8.GetBytes(this.key);
                aes.IV = Encoding.UTF8.GetBytes(this.iv);
                var encrypt = aes.CreateEncryptor(aes.Key, aes.IV);
                byte[] buf = null;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encrypt, CryptoStreamMode.Write))
                    {
                        byte[] xXml = Encoding.UTF8.GetBytes(input);
                        cs.Write(xXml, 0, xXml.Length);
                    }
                    buf = ms.ToArray();
                }
                string Output = Convert.ToBase64String(buf);
                return Output;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return ex.Message;
            }
        }
    }
}
