using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using UP01_Rusakov_RealEstate.Classes;
using UP01_Rusakov_RealEstate.Model;

namespace UP01_Rusakov_RealEstate.Pages
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();

            List<PropertyType> types = new List<PropertyType>(Entities.GetContext().PropertyTypes);
            types.Sort((a, b) => a.Name.CompareTo(b.Name));
            types.Insert(0, new PropertyType { ID = 0, Name = "Все типы" });
            TypeFilter.ItemsSource = types;

            TypeFilter.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
            UpdateProperties();
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
            string search = SearchBox.Text.ToLower();
            int allCount = 0;
            List<Property> current = new List<Property>();

            foreach (Property p in Entities.GetContext().Properties)
            {
                allCount++;

                if (search != "" && !p.Address.ToLower().Contains(search))
                    continue;
                if (type != null && type.ID != 0 && p.TypeID != type.ID)
                    continue;
                if (OnlyMineCheck.IsChecked == true && p.UserID != Helper.CurrentUser.ID)
                    continue;

                current.Add(p);
            }

            if (SortBox.SelectedIndex == 0)
                current.Sort((a, b) => a.Price.CompareTo(b.Price));
            else if (SortBox.SelectedIndex == 1)
                current.Sort((a, b) => b.Price.CompareTo(a.Price));
            else if (SortBox.SelectedIndex == 2)
                current.Sort((a, b) => a.Area.CompareTo(b.Area));
            else if (SortBox.SelectedIndex == 3)
                current.Sort((a, b) => b.Area.CompareTo(a.Area));

            ListProperties.ItemsSource = current;
            CountText.Text = "Найдено объектов: " + current.Count + " из " + allCount;
        }

        private void Filter_Changed(object sender, RoutedEventArgs e)
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
            SortBox.SelectedIndex = 0;
            OnlyMineCheck.IsChecked = false;
        }
    }
}
