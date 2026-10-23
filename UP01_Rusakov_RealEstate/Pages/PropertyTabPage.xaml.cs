using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using UP01_Rusakov_RealEstate.Classes;
using UP01_Rusakov_RealEstate.Model;

namespace UP01_Rusakov_RealEstate.Pages
{
    public partial class PropertyTabPage : Page
    {
        public PropertyTabPage()
        {
            InitializeComponent();

            List<PropertyType> types = new List<PropertyType>(Entities.GetContext().PropertyTypes);
            types.Sort((a, b) => a.Name.CompareTo(b.Name));
            types.Insert(0, new PropertyType { ID = 0, Name = "Все типы" });
            TypeFilter.ItemsSource = types;

            List<User> agents = new List<User>(Entities.GetContext().Users);
            agents.Sort((a, b) => a.FIO.CompareTo(b.FIO));
            agents.Insert(0, new User { ID = 0, FIO = "Все агенты" });
            AgentFilter.ItemsSource = agents;

            TypeFilter.SelectedIndex = 0;
            AgentFilter.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                Entities.Refresh();
                UpdateProperties();
            }
        }

        private void UpdateProperties()
        {
            if (!IsInitialized)
                return;

            PropertyType type = TypeFilter.SelectedItem as PropertyType;
            User agent = AgentFilter.SelectedItem as User;
            string search = SearchBox.Text.ToLower();
            int allCount = 0;
            decimal sum = 0;
            List<Property> current = new List<Property>();

            foreach (Property p in Entities.GetContext().Properties)
            {
                allCount++;

                if (search != "" && !p.Address.ToLower().Contains(search))
                    continue;
                if (type != null && type.ID != 0 && p.TypeID != type.ID)
                    continue;
                if (agent != null && agent.ID != 0 && p.UserID != agent.ID)
                    continue;

                current.Add(p);
                sum += p.Price;
            }

            if (SortBox.SelectedIndex == 0)
                current.Sort((a, b) => a.Price.CompareTo(b.Price));
            else if (SortBox.SelectedIndex == 1)
                current.Sort((a, b) => b.Price.CompareTo(a.Price));
            else if (SortBox.SelectedIndex == 2)
                current.Sort((a, b) => a.Area.CompareTo(b.Area));
            else if (SortBox.SelectedIndex == 3)
                current.Sort((a, b) => b.Area.CompareTo(a.Area));

            DataGridProperty.ItemsSource = current;
            CountText.Text = "Показано: " + current.Count + " из " + allCount +
                             " · Сумма: " + sum.ToString("N0") + " ₽";
        }

        private void Filter_Changed(object sender, SelectionChangedEventArgs e)
        {
            UpdateProperties();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintSearch.Visibility = SearchBox.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
            UpdateProperties();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            TypeFilter.SelectedIndex = 0;
            AgentFilter.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddPropertyPage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddPropertyPage((sender as Button).DataContext as Property));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            List<Property> propertiesForRemoving = new List<Property>();
            foreach (Property p in DataGridProperty.SelectedItems)
                propertiesForRemoving.Add(p);

            if (propertiesForRemoving.Count == 0)
            {
                Helper.ShowError("Выделите объекты для удаления!");
                return;
            }

            if (!Helper.Confirm("Вы точно хотите удалить записи в количестве " + propertiesForRemoving.Count + " элементов?"))
                return;

            try
            {
                Entities.GetContext().Properties.RemoveRange(propertiesForRemoving);
                Entities.GetContext().SaveChanges();
                Helper.ShowInfo("Данные успешно удалены!");
            }
            catch (Exception ex)
            {
                Helper.ShowError(ex.Message);
            }
            Entities.Refresh();
            UpdateProperties();
        }
    }
}
