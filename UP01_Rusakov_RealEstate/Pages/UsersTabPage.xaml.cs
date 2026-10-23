using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using UP01_Rusakov_RealEstate.Classes;
using UP01_Rusakov_RealEstate.Model;

namespace UP01_Rusakov_RealEstate.Pages
{
    public partial class UsersTabPage : Page
    {
        public UsersTabPage()
        {
            InitializeComponent();
            RoleFilter.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                Entities.Refresh();
                UpdateUsers();
            }
        }

        private void UpdateUsers()
        {
            if (!IsInitialized)
                return;

            string search = SearchBox.Text.ToLower();
            int allCount = 0;
            List<User> current = new List<User>();

            foreach (User u in Entities.GetContext().Users)
            {
                allCount++;

                if (search != "" && !u.FIO.ToLower().Contains(search) && !u.Login.ToLower().Contains(search))
                    continue;
                if (RoleFilter.SelectedIndex == 1 && u.Role != "Admin")
                    continue;
                if (RoleFilter.SelectedIndex == 2 && u.Role != "User")
                    continue;

                current.Add(u);
            }

            if (SortBox.SelectedIndex == 0)
                current.Sort((a, b) => a.FIO.CompareTo(b.FIO));
            else
                current.Sort((a, b) => b.FIO.CompareTo(a.FIO));

            DataGridUser.ItemsSource = current;
            CountText.Text = "Показано: " + current.Count + " из " + allCount;
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintSearch.Visibility = SearchBox.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
            UpdateUsers();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            RoleFilter.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddUserPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddUserPage((sender as Button).DataContext as User));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            List<User> usersForRemoving = new List<User>();
            foreach (User u in DataGridUser.SelectedItems)
                usersForRemoving.Add(u);

            if (usersForRemoving.Count == 0)
            {
                Helper.ShowError("Выделите пользователей для удаления!");
                return;
            }
            foreach (User u in usersForRemoving)
            {
                if (u.ID == Helper.CurrentUser.ID)
                {
                    Helper.ShowError("Нельзя удалить свою учётную запись!");
                    return;
                }
            }

            if (!Helper.Confirm("Вы точно хотите удалить записи в количестве " + usersForRemoving.Count +
                                " элементов?\nОбъекты недвижимости этих пользователей тоже будут удалены."))
                return;

            try
            {
                Entities.GetContext().Users.RemoveRange(usersForRemoving);
                Entities.GetContext().SaveChanges();
                Helper.ShowInfo("Данные успешно удалены!");
            }
            catch (Exception ex)
            {
                Helper.ShowError(ex.Message);
            }
            Entities.Refresh();
            UpdateUsers();
        }
    }
}
