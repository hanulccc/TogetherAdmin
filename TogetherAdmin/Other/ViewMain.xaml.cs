using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using SangAdmin.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SangAdmin.Other
{
    /// <summary>
    /// ViewMain.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ViewMain : UserControl
    {
        public SeriesCollection seriesCollection { get; set; }
        public ChartValues<double> value1 { get; set; }
        public ChartValues<double> value2 { get; set; }
        public ChartValues<double> value3 { get; set; }
        public ChartValues<double> value4 { get; set; }
        public ChartValues<double> value5 { get; set; }
        public ChartValues<double> value6 { get; set; }
        public ChartValues<double> value7 { get; set; }
        public ChartValues<double> value8 { get; set; }
        public ChartValues<double> value9 { get; set; }
        public ChartValues<double> value10 { get; set; }
        public ChartValues<double> value11 { get; set; }
        public ChartValues<double> value12 { get; set; }
        public ChartValues<double> value13 { get; set; }
        public ChartValues<double> value14 { get; set; }
        public ChartValues<double> value15 { get; set; }
        public string[] label1 { get; set; }
        public string[] label2 { get; set; }
        public string[] label3 { get; set; }

        MainWindow mw;
        BasePage _page = new BasePage();

        public ViewMain(MainWindow mw)
        {
            InitializeComponent();

            this.mw = mw;

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            Query();
        }

        public void Restart()
        {
            Query();
        }
        #endregion

        #region [ 조회 ]
        private void Query()
        {
            try
            {
                this.DataContext = null;

                /*seriesCollection = new SeriesCollection
                {
                    new PieSeries
                    {
                        Title = "매칭 18",
                        Values = new ChartValues<ObservableValue> { new ObservableValue(18) },
                        DataLabels = true,
                        Fill = new SolidColorBrush(Color.FromRgb(91, 180, 170))
                    },
                    new PieSeries
                    {
                        Title = "취소 6",
                        Values = new ChartValues<ObservableValue> { new ObservableValue(6) },
                        DataLabels = true,
                        Fill = new SolidColorBrush(Color.FromRgb(215, 221, 220))
                    }
                };*/

                /// 사용자 위치 현황 ///
                value1 = new ChartValues<double> { };
                value2 = new ChartValues<double> { };
                label1 = new string[] { };
                string[] la = new string[9];
                int i = 0;

                JObject jObj = Api.GetResponseJObject(Api.statUser_url + "?proc_type=30");

                if (jObj == null) return;

                if (jObj["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObj["resultMsg"].ToString());
                    return;
                }
                if (jObj["resultData"].ToString() == "") return;

                JArray jAry = JArray.Parse(jObj["resultData"].ToString());

                //Console.WriteLine(" [ 사용자위치현황 ]");
                //Console.WriteLine(jAry);

                foreach (JObject jItem in jAry)
                {
                    if (i < 9)
                    {
                        value1.Add(double.Parse(jItem["driver"].ToString()));
                        value2.Add(double.Parse(jItem["shelper"].ToString()));

                        la[i] = jItem["label"].ToString();

                        i++;
                    }
                }

                label1 = la;




                /// 픽업 매칭 현황 ///
                value3 = new ChartValues<double> { };
                value4 = new ChartValues<double> { };
                value5 = new ChartValues<double> { };
                value6 = new ChartValues<double> { };
                value7 = new ChartValues<double> { };
                value8 = new ChartValues<double> { };

                jObj = Api.GetResponseJObject(Api.statPick_url + "?proc_type=100");

                if (jObj == null) return;

                if (jObj["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObj["resultMsg"].ToString());
                    return;
                }
                if (jObj["resultData"].ToString() == "") return;

                jAry = JArray.Parse(jObj["resultData"].ToString());

                foreach (JObject jItem in jAry)
                {
                    value3.Add(double.Parse(jItem["cntAll"].ToString()));
                    value4.Add(double.Parse(jItem["cntR"].ToString()));
                    value5.Add(double.Parse(jItem["cntA"].ToString()));
                    value6.Add(double.Parse(jItem["cntI"].ToString()));
                    value7.Add(double.Parse(jItem["cntO"].ToString()));
                    value8.Add(double.Parse(jItem["cntC"].ToString()));
                }





                /// 택시동승현황 ///
                value9 = new ChartValues<double> { };
                value10 = new ChartValues<double> { };
                value11 = new ChartValues<double> { };

                jObj = Api.GetResponseJObject(Api.statTaxi_url + "?proc_type=40");

                if (jObj == null) return;

                if (jObj["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObj["resultMsg"].ToString());
                    return;
                }
                if (jObj["resultData"].ToString() == "") return;

                value9.Add(double.Parse(jObj["resultData"]["active"].ToString()));
                value10.Add(double.Parse(jObj["resultData"]["total"].ToString()));
                value11.Add(double.Parse(jObj["resultData"]["close"].ToString()));

                // Console.WriteLine(" [ 택시동승현황 ]");
                //Console.WriteLine(jObj);



                /// 입출금 현황 ///
                value12 = new ChartValues<double> { };
                value13 = new ChartValues<double> { };
                label2 = new string[] { };
                la = new string[7];

                i = 0;

                jObj = Api.GetResponseJObject("http://114.207.112.42:5455/together/weekstat");

                if (jObj == null) return;

                if (jObj["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObj["resultMsg"].ToString());
                    return;
                }
                if (jObj["resultData"].ToString() == "") return;

                jAry = JArray.Parse(jObj["resultData"].ToString());

                //Console.WriteLine(" [ 입출금현황 ]");
                //Console.WriteLine(jAry);

                foreach (JObject jItem in jAry)
                {
                    if (i < 7)
                    {
                        value12.Add(double.Parse(jItem["output"].ToString()));
                        value13.Add(double.Parse(jItem["input"].ToString()));

                        la[i] = jItem["label"].ToString();

                        i++;
                    }
                }

                label2 = la;




                /// 가입자 현황 ///
                value14 = new ChartValues<double> { };
                value15 = new ChartValues<double> { };
                label3 = new string[] { };
                la = new string[7];

                i = 0;

                jObj = Api.GetResponseJObject(Api.statUser_url + "?proc_type=10&start_date=" + DateTime.Now.AddDays(-8) + "&end_date=" + DateTime.Now + "&yoil=");

                if (jObj == null) return;

                if (jObj["resultCode"].ToString() != "200")
                {
                    MessageBox.Show(jObj["resultMsg"].ToString());
                    return;
                }
                if (jObj["resultData"].ToString() == "") return;

                jAry = JArray.Parse(jObj["resultData"].ToString());

                //Console.WriteLine(" [ 가입자현황 ]");
                //Console.WriteLine(jAry);

                foreach (JObject jItem in jAry)
                {
                    if (i < 7)
                    {
                        value14.Add(double.Parse(jItem["stat_dtot"].ToString()));
                        value15.Add(double.Parse(jItem["stat_htot"].ToString()));

                        la[i] = jItem["stat_date"].ToString();

                        i++;
                    }
                }

                label3 = la;


                /// 현황판 ///

                // 금일 접속자 수 - 접속통계
                jObj = Api.GetResponseJObject(Api.statLogin_url + "?proc_type=10&start_date=" + DateTime.Now.ToString("yyyy-MM-dd") + "&end_date=" + DateTime.Now.ToString("yyyy-MM-dd") + "&yoil=");

                if (jObj["resultCode"].ToString() == "200")
                {
                    jAry = JArray.Parse(jObj["resultData"].ToString());
                    btnTodayUser.Content = int.Parse(jObj["resultData"][0]["stat_helpercnt"].ToString()) + int.Parse(jObj["resultData"][0]["stat_drivercnt"].ToString());
                }

                // 실시간 접속자 수 - 사용자 위치 현황
                string lat = "36.7828003";
                string lon = "127.9942873";
                string strDistance = "5000000";

                string data = string.Format("proc_type={0}&lat={1}&lon={2}&distance={3}", "60", lat, lon, strDistance);
                string strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/pickup", "GET", data);

                JObject jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    jAry = JArray.Parse(jObject["resultData"].ToString());
                    btnNowUser.Content = jAry.Count;
                }

                // 프로그램 다운로드 수 ( 가입자 수 ) - 사용자 목록
                data = string.Format("proc_type={0}", "140");
                strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/user", "GET", data);

                jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    string shelperCnt = jObject["resultData"][0]["helper_cnt"].ToString();
                    string driverCnt = jObject["resultData"][0]["driver_cnt"].ToString();

                    btnDownNum.Content = int.Parse(shelperCnt) + int.Parse(driverCnt);
                }

                // 심사대기 인원 수 - 픽업 매칭 현황
                data = string.Format("proc_type={0}", "50");
                strResult = _page.HttpSendData(_page.GetServerUrl + "/admin/pickup", "GET", data);

                jObject = JObject.Parse(strResult); //json 객체로

                if (jObject["resultCode"].ToString() == "200")
                {
                    btnWaitCnt.Content = jObject["resultData"][0]["wait_cnt"].ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                this.DataContext = this;
            }
        }
        #endregion

        private void btnPageChange_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            DockPanel dp = btn.Parent as DockPanel;
            Label lb = dp.Children[0] as Label;

            mw.pageChange(btn.Tag.ToString(), lb.Tag.ToString());
        }
    }
}
