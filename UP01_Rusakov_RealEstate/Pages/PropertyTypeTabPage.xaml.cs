using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using UP01_Rusakov_RealEstate.Classes;
using UP01_Rusakov_RealEstate.Model;

namespace UP01_Rusakov_RealEstate.Pages
{
    public partial class PropertyTypeTabPage : Page
    {
        public PropertyTypeTabPage()
        {
            InitializeComponent();
            SortBox.SelectedIndex = 0;
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                Entities.Refresh();
                UpdateTypes();
            }
        }

        private void UpdateTypes()
        {
            if (!IsInitialized)
                return;

            string search = SearchBox.Text.ToLower();
            int allCount = 0;
            List<PropertyType> current = new List<PropertyType>();

            foreach (PropertyType t in Entities.GetContext().PropertyTypes)
            {
                allCount++;

                if (search != "" && !t.Name.ToLower().Contains(search))
                    continue;
                if (OnlyUsedCheck.IsChecked == true && t.Properties.Count == 0)
                    continue;

                current.Add(t);
            }

            if (SortBox.SelectedIndex == 0)
                current.Sort((a, b) => a.Name.CompareTo(b.Name));
            else if (SortBox.SelectedIndex == 1)
                current.Sort((a, b) => b.Name.CompareTo(a.Name));
            else if (SortBox.SelectedIndex == 2)
                current.Sort((a, b) => b.Properties.Count.CompareTo(a.Properties.Count));

            DataGridType.ItemsSource = current;
            CountText.Text = "Показано: " + current.Count + " из " + allCount;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintSearch.Visibility = SearchBox.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
            UpdateTypes();
        }

        private void SortBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateTypes();
        }

        private void OnlyUsedCheck_Changed(object sender, RoutedEventArgs e)
        {
            UpdateTypes();
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            SearchBox.Text = "";
            SortBox.SelectedIndex = 0;
            OnlyUsedCheck.IsChecked = false;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddPropertyTypePage(null));
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddPropertyTypePage((sender as Button).DataContext as PropertyType));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            List<PropertyType> typesForRemoving = new List<PropertyType>();
            foreach (PropertyType t in DataGridType.SelectedItems)
                typesForRemoving.Add(t);

            if (typesForRemoving.Count == 0)
            {
                Helper.ShowError("Выделите типы для удаления!");
                return;
            }

            if (!Helper.Confirm("Вы точно хотите удалить записи в количестве " + typesForRemoving.Count +
                                " элементов?\nОбъекты недвижимости этих типов тоже будут удалены."))
                return;

            try
            {
                Entities.GetContext().PropertyTypes.RemoveRange(typesForRemoving);
                Entities.GetContext().SaveChanges();
                Helper.ShowInfo("Данные успешно удалены!");
            }
            catch (Exception ex)
            {
                Helper.ShowError(ex.Message);
            }
            Entities.Refresh();
            UpdateTypes();
        }
    }
}
