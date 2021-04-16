using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SangAdmin;

namespace SangAdmin.Common
{

    public class ComboBoxPairs
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public ComboBoxPairs(string Name, string Value)
        {
            this.Name = Name;
            this.Value = Value;
        }
    }

    class BasePage
    {
        //        public static string mGetServerUrl = "https://shelper.kr";
        public static string mGetServerUrl = "http://115.85.182.247:3003";
        public static string mCommAttachFileUrl = "/DataFile/CommFile/";

        public static Dictionary<string, UserControl> dicView = new Dictionary<string, UserControl>();

        public static MainWindow mw = null;

        #region Web Server Url
        public string GetServerUrl
        {
            get { return mGetServerUrl; }
        }
        #endregion

        #region Web 공통파일 Url
        public string CommAttachFileUrl
        {
            get { return mCommAttachFileUrl; }
        }
        #endregion


        #region [ 서버에 이미지 업로드 ]
        public string HttpUploadFile(string ServerUrl, string FilePath, string ParamName, string ContentType)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(ServerUrl);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";

            string header = string.Format(headerTemplate, ParamName, FilePath, ContentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            FileStream fileStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            string strResult = "";
            try
            {
                wresp = wr.GetResponse();
                Stream stream = wresp.GetResponseStream();
                StreamReader reader = new StreamReader(stream);
                strResult = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }

            return strResult;
        }
        #endregion

        #region 서버 통신
        public string HttpSendData(string PushServerUrl, string Method, string data)
        {
            try
            {
                HttpWebRequest request;

                if (Method == "POST" || Method == "DELETE")
                {
                    // 요청 String -> 요청 Byte 변환
                    byte[] byteDataParams = UTF8Encoding.UTF8.GetBytes(data);

                    request = (HttpWebRequest)WebRequest.Create(PushServerUrl);
                    request.Method = Method;
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = byteDataParams.Length;

                    // 요청 Byte -> 요청 Stream 변환
                    Stream stDataParams = request.GetRequestStream();
                    stDataParams.Write(byteDataParams, 0, byteDataParams.Length);
                    stDataParams.Close();
                }
                else
                {
                    request = (HttpWebRequest)WebRequest.Create(PushServerUrl + "?" + data);
                    request.Method = "GET";
                }

                // 요청, 응답 받기
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // 응답 Stream 읽기
                Stream stReadData = response.GetResponseStream();
                StreamReader srReadData = new StreamReader(stReadData, Encoding.UTF8);

                // 응답 Stream -> 응답 String 변환
                string strResult = srReadData.ReadToEnd();

                //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PushServerUrl);
                //request.Method = "POST";
                //request.ContentType = "application/x-www-form-urlencoded";

                //// 인코딩 UTF-8
                //byte[] byteDataParams = UTF8Encoding.UTF8.GetBytes(data);

                //request.ContentLength = byteDataParams.Length;

                //Stream st = request.GetRequestStream();
                //st.Write(byteDataParams, 0, byteDataParams.Length);
                //st.Close();

                //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                //Stream stream = response.GetResponseStream();
                //StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                //string strResult = reader.ReadToEnd();
                //stream.Close();
                //response.Close();
                //reader.Close();

                return strResult;
            }
            catch (Exception e)
            {
                return "{resultCode: 900, resultMsg: \"" + e + "\"}";
            }
        }
        #endregion

        #region [ 서버에 이미지 업로드 ]
        public string HttpPostFileData(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            if (file != "")
            {
                FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    rs.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();
            }

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            string strResult = "";
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                strResult = reader2.ReadToEnd();
            }
            catch (Exception ex)
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }

            return strResult;
        }
        #endregion
    }

    public class Api
    {
        public static string rootURL = "http://125.130.6.21:9092";
        public static string userAttachFileUrl = "/DataFile/UserFile/";
        public static string commAttachFileUrl = "/DataFile/CommFile/";
        public static string commAttachUploadUrl = rootURL + "/MHWebService/CommFileUpload.aspx";
        //        public static string serverURL = "https://shelper.kr";
        public static string serverURL = "http://115.85.182.247:3003";

        public static string user_url = serverURL + "/admin/user";
        public static string login_url = serverURL + "/login/loginctl";
        public static string faq_url = serverURL + "/admin/faq";
        public static string qna_url = serverURL + "/admin/qna";
        public static string infocall_url = serverURL + "/admin/infocall";
        public static string notice_url = serverURL + "/admin/notice";
        public static string vraccnt_url = serverURL + "/admin/vraccnt";
        public static string vrnum_url = serverURL + "/admin/vrnum";
        public static string sms_url = serverURL + "/admin/sms";
        public static string push_url = serverURL + "/admin/push";
        public static string pickup_url = serverURL + "/admin/pickup";
        public static string statPick_url = serverURL + "/stat/pickup";
        public static string statTaxi_url = serverURL + "/stat/taxi";
        public static string statLogin_url = serverURL + "/stat/login";
        public static string statUser_url = serverURL + "/stat/user";
        public static string position_url = serverURL + "/RedisMgr/Position";
        public static string redisPickup_rul = serverURL + "/RedisMgr/PickUp";
        public static string adminInfo_url = serverURL + "/admin/admin_info";
        public static string tmap_url = serverURL + "/tmap";


        #region api 연결
        public static JObject PostResponseJObject(string uri, NameValueCollection nv)
        {
            WebClient webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            webClient.Encoding = UTF8Encoding.UTF8;
            JObject jObj = new JObject();

            try
            {
                byte[] responsebytes = webClient.UploadValues(uri, "POST", nv);
                string responsebody = Encoding.UTF8.GetString(responsebytes);
                responsebody = responsebody.Trim();

                if (responsebody == null)
                    return null;

                jObj = JObject.Parse(responsebody);
            }
            catch (WebException ex)
            {
                MessageBox.Show("서버에서 오류가 발생했습니다.");
                Console.WriteLine("서버오류: " + ex);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("서버에서 오류가 발생했습니다.");
                Console.WriteLine("서버오류: " + ex);
                return null;
            }

            return jObj;
        }

        public static JObject GetResponseJObject(string uri)
        {
            WebClient webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            webClient.Encoding = UTF8Encoding.UTF8;
            JObject jObj = new JObject();

            try
            {
                Stream stream = webClient.OpenRead(uri);
                string responsebody = new StreamReader(stream).ReadToEnd();
                responsebody = responsebody.Trim();

                if (responsebody == null)
                    return null;

                jObj = JObject.Parse(responsebody);
            }
            catch (WebException ex)
            {
                MessageBox.Show("서버에서 오류가 발생했습니다.");
                Console.WriteLine("서버오류: " + ex);
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("서버에서 오류가 발생했습니다.");
                Console.WriteLine("서버오류: " + ex);
                return null;
            }

            return jObj;
        }

        public static JObject set_file_formdata_JObject(string url, MultipartFormDataContent form)
        {
            try
            {
                // only for test purposes, for stable environment, use ApiRequest class.
                var response = Task.Run(() => PostURI(url, form));
                response.Wait();//필수

                if (response.Result == null || response.Result == "")
                {
                    return JObject.Parse("{resultCode: 400, resultMsg: \"오류가 발생했습니다.\"}");
                }

                return JObject.Parse(response.Result);
            }
            catch (WebException ex)
            {
                string responseText;

                var responseStream = ex.Response?.GetResponseStream();

                if (responseStream != null)
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        responseText = reader.ReadToEnd();

                        JObject res = JObject.Parse(responseText);

                        //임시
                        Console.WriteLine("res: " + res);
                        return JObject.Parse("{resultCode: " + res["status"] + ", resultMsg: \"" + res["resultMsg"] + "\"}");
                    }
                }
                else
                {
                    Console.WriteLine("서버오류: " + ex);
                    return JObject.Parse("{resultCode: 900, resultMsg: \"서버오류가 발생했습니다.\"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("서버오류: " + ex);
                return JObject.Parse("{resultCode: 900, resultMsg: \"서버오류가 발생했습니다.\"}");
            }
        }

        static async Task<string> PostURI(string u, HttpContent hc)
        {
            var response = string.Empty;
            using (var client = new HttpClient())
            {
                //client.DefaultRequestHeaders.Add("Authentication", key);
                HttpResponseMessage result = await client.PostAsync(u, hc);
                if (result.IsSuccessStatusCode)
                {
                    response = await result.Content.ReadAsStringAsync();
                }
            }

            Console.WriteLine(response);
            return response;
        }


        public static string HttpPostFileData(string url, string file, string paramName, string contentType, NameValueCollection nvc)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, file, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            if (file != "")
            {
                FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    rs.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();
            }

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            string strResult = "";
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                strResult = reader2.ReadToEnd();
            }
            catch (Exception ex)
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                wr = null;
            }

            return strResult;
        }
        #endregion
    }

    public class StatData
    {
        public string stat_date { get; set; }
        public string stat_time { get; set; }
        public string stat_rcnt { get; set; }
        public string stat_acnt { get; set; }
        public string stat_icnt { get; set; }
        public string stat_ocnt { get; set; }
        public string stat_ccnt { get; set; }
        public string stat_direct_call { get; set; }
        public string stat_normal_call { get; set; }
        public string stat_sum_fee { get; set; }
        public string stat_start { get; set; }
        public string stat_end { get; set; }
        public string stat_avg_time { get; set; }
        public string stat_avg_fee { get; set; }
        public string stat_fee { get; set; }
        public string stat_cancel { get; set; }
        public string stat_cnt { get; set; }
    }

    public class TaxiData
    {
        public string stat_date { get; set; }
        public string stat_chatcnt { get; set; }
        public string stat_time { get; set; }
        public string stat_area { get; set; }
    }

    public class ConnectData
    {
        public string stat_date { get; set; }
        public string stat_helpercnt { get; set; }
        public string stat_drivercnt { get; set; }
        public int stat_plus { get; set; }
    }

    public class LoginData
    {
        public string stat_time { get; set; }
        public string stat_drivercnt { get; set; }
        public string stat_helpercnt { get; set; }
        public int stat_plus { get; set; }
    }
    public class StatUserData
    {
        public string stat_date { get; set; }
        public string stat_dtot { get; set; }
        public string stat_htot { get; set; }
        public string stat_hreq { get; set; }
        public string stat_signup { get; set; }
        public string stat_tot { get; set; }
        public string stat_active { get; set; }
        public string stat_download { get; set; }
        public string stat_area { get; set; }
    }

    public class LocationData
    {
        public string user_id { get; set; }
        public string user_type { get; set; }
        public string user_lat { get; set; }
        public string user_lon { get; set; }
        public string distance { get; set; }
    }

    public class AdminData
    {
        public string admin_id { get; set; }
        public string admin_name { get; set; }
        public string admin_power { get; set; }
        public string admin_use_yn { get; set; }
        public string admin_join_dt { get; set; }
        public string admin_login_dt { get; set; }
    }
}
