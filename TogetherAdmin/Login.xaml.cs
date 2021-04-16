using SangAdmin.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace SangAdmin
{
    /// <summary>
    /// Login.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Login : Window
    {
        BasePage _page = new BasePage();
        private string key = "sangsang_54540123456789012345678";
        private string iv = "sang_54540123456";

        public Login()
        {
            InitializeComponent();

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            windowLogin.MouseLeftButtonDown += (o, e) => DragMove();

            getId();
        }
        #endregion

        private void getId()
        {
            string path = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\id.txt";
            string id;
            try
            {
                StreamReader sw = new StreamReader(path, Encoding.Default, true); ;
                id = sw.ReadLine();
                sw.Close();
            }
            catch (Exception)
            {
                id = "";
                txtid.Focus();
                return;
            }

            txtid.Text = id;
            cbIdSave.IsChecked = true;
            pwd.Focus();
        }

        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            idSave();
            this.Close();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            loginAction();
        }

        private void idSave()
        {
            if (cbIdSave.IsChecked == false) { return; }

            // 아이디 저장하기
            string path = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\id.txt";
            StreamWriter sw = new StreamWriter(path, false, Encoding.Default);
            sw.WriteLine(txtid.Text);
            sw.Flush();
            sw.Close();
        }



        private void loginAction()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                if (txtid.Text == "") { MessageBox.Show("아이디를 입력해주세요."); return; }
                if (pwd.Password == "") { MessageBox.Show("비밀번호를 입력해주세요."); return; }


                string data = string.Format("proc_type={0}&admin_id={1}&admin_pwd={2}", "40", txtid.Text, Uri.EscapeDataString(AESEncrypt(pwd.Password)));
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/admin_info", "POST", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObject["resultMsg"].ToString(), "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                JObject jObj = JObject.Parse(jObject["resultData"].ToString());

                MainWindow mw = new MainWindow(txtid.Text, jObj["admin_name"].ToString(), int.Parse(jObj["admin_power"].ToString()));
                mw.Show();

                idSave();

                this.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show("로그인 에러: " + ex.Message, "오류", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }



        //AES 암호화
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

        //AES 복호화
        private string AESDecrypt(string input)
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
                var decrypt = aes.CreateDecryptor();
                byte[] buf = null;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, decrypt, CryptoStreamMode.Write))
                    {
                        byte[] xXml = Convert.FromBase64String(input);
                        cs.Write(xXml, 0, xXml.Length);
                    }
                    buf = ms.ToArray();
                }
                string Output = Encoding.UTF8.GetString(buf);
                return Output;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return string.Empty;
            }
        }

        private void pwd_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                loginAction();
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
