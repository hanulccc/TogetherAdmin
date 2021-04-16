using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
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
using SangAdmin.Matching;
using SangAdmin.Other;
using SangAdmin.Setting;
using SangAdmin.Stats;
using SangAdmin.User;
using SangAdmin.VirtualAccnt;

namespace SangAdmin
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewMain viewMain;
        ViewMatching viewMatching;
        ViewUser viewUser;
        ViewVirtualAccnt  viewVirtualAccnt;
        ViewSetting viewSetting;
        ViewStats viewStats;

        public string strAdId;
        public string  strAdName;
        public int intAdPower;

        public SeriesCollection seriesCollection { get; set; }

        public MainWindow(string strAdId, string strAdName, int intAdPower)
        {
            InitializeComponent();

            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.strAdId = strAdId;
            this.strAdName = strAdName;
            this.intAdPower = intAdPower;

            SetDefault();
        }

        #region [ 초기값설정 ]
        private void SetDefault()
        {
            Application.Current.MainWindow = this; // 메인윈도우 지정해주기
            bdHeader.MouseLeftButtonDown += (o, e) => DragMove();
            viewMain = new ViewMain(this);
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.contentControl.DataContext = viewMain;

            // 관리자 이름, 등급 설정해주기
            txtAdminNm.Content = this.strAdName;
            if (this.intAdPower == 0) { txtAdminType.Content = "일반"; }
        }
        #endregion

        #region 메뉴 조정

        private void btnSubMenuOpner_Click(object sender, RoutedEventArgs e)
        {
            // 서브 메뉴 펼치고 접기
            Button btn = sender as Button;
            DockPanel dp = btn.Parent as DockPanel;
            DockPanel dpParent = dp.Parent as DockPanel;
            StackPanel sp = dpParent.Children[1] as StackPanel;

            if (sp.Visibility == Visibility.Collapsed)
            {
                // 보여주기
                dp.Children[1].Visibility = Visibility.Collapsed;
                dp.Children[2].Visibility = Visibility.Visible;

                sp.Visibility = Visibility.Visible;
            }
            else
            {
                // 숨기기
                subMenuClose(dp, sp);
            }
        }

        private void subMenuClose(DockPanel dp, StackPanel sp)
        {
            // 숨기기
            dp.Children[1].Visibility = Visibility.Visible;
            dp.Children[2].Visibility = Visibility.Collapsed;

            sp.Visibility = Visibility.Collapsed;
        }


        private void subMenu_MouseEnter(object sender, MouseEventArgs e)
        {
            // 서브 메뉴 마우스 올렸을 때 디자인 변경
            Label lb = sender as Label;
            lb.Foreground = new SolidColorBrush(Color.FromRgb(91, 180, 170));
            lb.FontWeight = FontWeights.Bold;
        }

        private void subMenu_MouseLeave(object sender, MouseEventArgs e)
        {
            // 서브 메뉴 마우스 내렸을 때 디자인 변경
            Label lb = sender as Label;
            lb.Foreground = new SolidColorBrush(Color.FromRgb(166, 166, 166));
            lb.FontWeight = FontWeights.Normal;
        }

        private void pageChange_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Label lb = sender as Label;

            pageChange(lb.Tag.ToString(), lb.Content.ToString());


            // 펼친 메뉴 리셋

            if (bdMenuParent.Tag.ToString() == "open") { menu_fold(); }
        }

        private void btnPageChange_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            pageChange(btn.Tag.ToString(), btn.Content.ToString());
        }

        public void pageChange(string tag, string content)
        {
            switch (tag)
            {
                case "viewMain":
                    this.viewMain.Restart();
                    this.contentControl.DataContext = viewMain;
                    this.lbMainHead.Content = "개요";
                    break;

                case "viewMatching":
                    if (content == "매칭 현황") { content = "사용자 위치 현황"; }

                    if (this.viewMatching == null) { this.viewMatching = new ViewMatching(content, this); }
                    else
                    {
                        // 페이지 변환
                        viewMatching.setPage(content);
                    }

                    this.contentControl.DataContext = viewMatching;
                    this.lbMainHead.Content = "매칭 현황";
                    break;
                case "viewUser":
                    if (content == "사용자 관리") { content = "사용자 목록"; }

                    if (this.viewUser == null) { this.viewUser = new ViewUser(content, this); }
                    else
                    {
                        // 페이지 변환
                        viewUser.setPage(content);
                    }

                    this.contentControl.DataContext = viewUser;
                    this.lbMainHead.Content = "사용자 목록";
                    break;

                case "viewVirtualAccnt":
                    if(content == "가상계좌 관리") { content = "가상계좌 발급 현황"; }

                    if (this.viewVirtualAccnt == null) { this.viewVirtualAccnt = new ViewVirtualAccnt(content, this); }
                    else
                    {
                        // 페이지 변환
                        viewVirtualAccnt.setPage(content);
                    }

                    this.contentControl.DataContext = viewVirtualAccnt;
                    this.lbMainHead.Content = "가상계좌 관리";
                    break;
                
                case "viewStats":
                    if (content == "통계") { content = "픽업 매칭 통계"; }

                    if (this.viewStats == null) { this.viewStats = new ViewStats(content, this); }
                    else
                    {
                        // 페이지 변환
                        viewStats.setPage(content);
                    }

                    this.contentControl.DataContext = viewStats;
                    this.lbMainHead.Content = "통계";
                    break;
                case "viewSetting":
                    if (content == "관리설정") { content = "고객지원"; }

                    if (this.viewSetting == null) { this.viewSetting = new ViewSetting(content, this); }
                    else
                    {
                        // 페이지 변환
                        viewSetting.setPage(content);
                    }

                    this.contentControl.DataContext = viewSetting;
                    this.lbMainHead.Content = "관리설정";
                    break;
            }

            // 펼친 메뉴 리셋

            if (bdMenuParent.Tag.ToString() == "open") { menu_fold(); }
        }

        private void menu_fold()
        {
            // 메뉴 접기
            Grid.SetColumnSpan(bdMenuParent, 1);
            bdMenuParent.Width = Double.NaN;

            // 맨 위 메뉴 오프너 변경
            dpMenu.Children[0].Visibility = Visibility.Collapsed;
            imgMenuOpener.Source = new BitmapImage(new Uri("pack://application:,,,/SangAdmin;component/Resources/MenuOpen.png"));

            // 다음 메뉴들 접기
            int i = 0;
            StackPanel spMain = bdMenuParent.Child as StackPanel;
            foreach (var child in spMain.Children)
            {
                // 첫번째 메뉴 오프너는 빼주기
                if (i == 0) { i++; continue; }

                // 타이틀 메뉴인가
                DockPanel dp = child as DockPanel;
                DockPanel dpChild = dp.Children[0] as DockPanel;

                dpChild.Children[0].Visibility = Visibility.Visible;
                dpChild.Children[1].Visibility = Visibility.Collapsed;

                if (i < 2) { i++; continue; }

                dpChild.Children[2].Visibility = Visibility.Collapsed;

                StackPanel sp = dp.Children[1] as StackPanel;
                sp.Visibility = Visibility.Collapsed;
            }

            bdMenuParent.Tag = "fold";
        }

        private void btnMenuOpener_Click(object sender, RoutedEventArgs e)
        {
            if (bdMenuParent.Tag.ToString() == "fold")
            {
                // 메뉴 펼치기
                Grid.SetColumnSpan(bdMenuParent, 2);
                bdMenuParent.Width = 220;

                // 맨 위 메뉴 오프너 변경
                dpMenu.Children[0].Visibility = Visibility.Visible;
                imgMenuOpener.Source = new BitmapImage(new Uri("pack://application:,,,/SangAdmin;component/Resources/MenuClose.png"));

                // 다음 메뉴들 펼치기
                int i = 0;
                StackPanel sp = bdMenuParent.Child as StackPanel;
                foreach (var child in sp.Children)
                {
                    // 첫번째 메뉴 오프너는 빼주기
                    if (i == 0) { i++; continue; }

                    // 타이틀 메뉴인가
                    DockPanel dp = child as DockPanel;
                    DockPanel dpChild = dp.Children[0] as DockPanel;

                    Button btn = dpChild.Children[0] as Button;

                    dpChild.Children[0].Visibility = Visibility.Collapsed;
                    dpChild.Children[1].Visibility = Visibility.Visible;
                }

                bdMenuParent.Tag = "open";
            }
            else
            {
                menu_fold();
            }
        }
        #endregion


        private void btnResizable_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                mdWinState.Kind = MaterialDesignThemes.Wpf.PackIconKind.SquareOutline;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                mdWinState.Kind = MaterialDesignThemes.Wpf.PackIconKind.SquareOutline;
            }
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }


        private void AdminMenu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (borAdminMenu.Visibility == Visibility.Collapsed)
            {
                borAdminMenu.Visibility = Visibility.Visible;
            }
            else
            {
                borAdminMenu.Visibility = Visibility.Collapsed;
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();

            this.Close();
        }

        private void AdminMenu_MouseLeave(object sender, MouseEventArgs e)
        {
            borAdminMenu.Visibility = Visibility.Collapsed;
        }
    }
}
