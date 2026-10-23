using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using UP01_Rusakov_RealEstate.Classes;

namespace UP01_Rusakov_RealEstate.Pages
{
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            AdminNameText.Text = Helper.CurrentUser.FIO;
            ContentFrame.Navigate(new UserPage());
        }

        private void ButtonCatalog_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new UserPage());
        }

        private void ButtonUsers_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new UsersTabPage());
        }

        private void ButtonTypes_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new PropertyTypeTabPage());
        }

        private void ButtonProperties_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new PropertyTabPage());
        }

        private void ButtonDiagram_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new DiagrammPage());
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (!(e.Content is AddUserPage || e.Content is AddPropertyTypePage || e.Content is AddPropertyPage))
            {
                while (ContentFrame.CanGoBack)
                    ContentFrame.RemoveBackEntry();
            }

            ((MainWindow)Application.Current.MainWindow).UpdateHeader();
        }

        private void ButtonLogout_Click(object sender, RoutedEventArgs e)
        {
            if (Helper.Confirm("Вы действительно хотите выйти из учётной записи?"))
                NavigationService.Navigate(new AuthPage());
        }
    }
}
