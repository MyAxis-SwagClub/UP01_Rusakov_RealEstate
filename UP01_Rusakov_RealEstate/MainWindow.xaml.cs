using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using UP01_Rusakov_RealEstate.Classes;
using UP01_Rusakov_RealEstate.Pages;

namespace UP01_Rusakov_RealEstate
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new AuthPage());
            UpdateThemeButton();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += (o, t) => { DateTimeNow.Text = DateTime.Now.ToString(); };
            timer.Start();
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is AuthPage || e.Content is AdminPage || e.Content is UserPage)
            {
                while (MainFrame.CanGoBack)
                    MainFrame.RemoveBackEntry();
            }
            if (e.Content is AuthPage)
                Helper.CurrentUser = null;

            UpdateHeader();
        }

        public void UpdateHeader()
        {
            Page page = MainFrame.Content as Page;
            AdminPage admin = MainFrame.Content as AdminPage;

            if (admin != null && admin.ContentFrame.Content != null)
                page = admin.ContentFrame.Content as Page;

            if (page != null)
            {
                PageTitle.Text = page.Title;
                Title = "AuroraApartments — " + page.Title;
            }

            if (admin != null && admin.ContentFrame.CanGoBack)
                ButtonBack.Visibility = Visibility.Visible;
            else
                ButtonBack.Visibility = Visibility.Hidden;

            if (Helper.CurrentUser != null && admin == null)
                ButtonLogout.Visibility = Visibility.Visible;
            else
                ButtonLogout.Visibility = Visibility.Hidden;

            if (Helper.CurrentUser != null)
                CurrentUserText.Text = "Пользователь: " + Helper.CurrentUser.FIO;
            else
                CurrentUserText.Text = "Вход не выполнен";
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            AdminPage admin = MainFrame.Content as AdminPage;
            if (admin != null && admin.ContentFrame.CanGoBack)
                admin.ContentFrame.GoBack();
        }

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            if (Helper.Confirm("Вы действительно хотите выйти из учётной записи?"))
                MainFrame.Navigate(new AuthPage());
        }

        private void ButtonTheme_Click(object sender, RoutedEventArgs e)
        {
            if (Helper.IsDarkTheme)
                Helper.ApplyTheme("Dictionary.xaml");
            else
                Helper.ApplyTheme("RusakovDictionary.xaml");
            UpdateThemeButton();

            AdminPage admin = MainFrame.Content as AdminPage;
            if (admin != null)
            {
                DiagrammPage diagram = admin.ContentFrame.Content as DiagrammPage;
                if (diagram != null)
                    diagram.ApplyChartTheme();
            }
        }

        private void UpdateThemeButton()
        {
            ButtonTheme.Content = Helper.IsDarkTheme ? "☀" : "☾";
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!Helper.Confirm("Действительно хотите закрыть приложение? Это приведет к немедленному выходу из аккаунта.",
                                "Подтверждение выхода"))
                e.Cancel = true;
        }
    }
}
