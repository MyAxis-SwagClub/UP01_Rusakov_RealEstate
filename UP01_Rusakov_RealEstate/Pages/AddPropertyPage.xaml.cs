using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using UP01_Rusakov_RealEstate.Classes;
using UP01_Rusakov_RealEstate.Model;

namespace UP01_Rusakov_RealEstate.Pages
{
    public partial class AddPropertyPage : Page
    {
        private Property currentProperty = new Property();

        public AddPropertyPage(Property selectedProperty)
        {
            InitializeComponent();

            List<PropertyType> types = new List<PropertyType>(Entities.GetContext().PropertyTypes);
            types.Sort((a, b) => a.Name.CompareTo(b.Name));
            CBType.ItemsSource = types;

            List<User> agents = new List<User>(Entities.GetContext().Users);
            agents.Sort((a, b) => a.FIO.CompareTo(b.FIO));
            CBUser.ItemsSource = agents;

            DPDate.DisplayDateStart = new DateTime(2000, 1, 1);
            DPDate.DisplayDateEnd = DateTime.Today;

            if (selectedProperty != null)
            {
                currentProperty = selectedProperty;
                Title = "Редактирование объекта недвижимости";

                CBType.SelectedItem = currentProperty.PropertyType;
                CBUser.SelectedItem = currentProperty.User;
                DPDate.SelectedDate = currentProperty.Date;
                TBAddress.Text = currentProperty.Address;
                TBArea.Text = currentProperty.Area.ToString();
                TBRooms.Text = currentProperty.Rooms.ToString();
                TBPrice.Text = currentProperty.Price.ToString("0");
                TBPhoto.Text = currentProperty.Photo;
            }
            else
            {
                Title = "Добавление объекта недвижимости";
                DPDate.SelectedDate = DateTime.Today;
            }
            HeaderText.Text = Title;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (CBType.SelectedItem == null)
                errors.AppendLine("Выберите тип недвижимости!");
            if (CBUser.SelectedItem == null)
                errors.AppendLine("Выберите агента!");

            if (DPDate.SelectedDate == null)
                errors.AppendLine("Укажите дату размещения!");
            else if (DPDate.SelectedDate < new DateTime(2000, 1, 1) || DPDate.SelectedDate > DateTime.Today)
                errors.AppendLine("Дата должна быть с 01.01.2000 по сегодняшний день!");

            if (TBAddress.Text.Trim().Length < 10)
                errors.AppendLine("Укажите адрес (не менее 10 символов)!");

            decimal area;
            if (!decimal.TryParse(TBArea.Text.Replace(",", "."), NumberStyles.Number, CultureInfo.InvariantCulture, out area))
                errors.AppendLine("Укажите площадь числом!");
            else if (area < 1 || area > 100000)
                errors.AppendLine("Площадь должна быть от 1 до 100 000 м²!");

            int rooms = 0;
            if (TBRooms.Text != "" && (!int.TryParse(TBRooms.Text, out rooms) || rooms > 50))
                errors.AppendLine("Количество комнат — от 0 до 50!");

            decimal price;
            if (!decimal.TryParse(TBPrice.Text, out price))
                errors.AppendLine("Укажите цену числом!");
            else if (price < 10000 || price > 10000000000)
                errors.AppendLine("Цена должна быть от 10 000 до 10 000 000 000 ₽!");

            if (errors.Length > 0)
            {
                Helper.ShowError(errors.ToString());
                return;
            }

            currentProperty.PropertyType = CBType.SelectedItem as PropertyType;
            currentProperty.User = CBUser.SelectedItem as User;
            currentProperty.Date = DPDate.SelectedDate.Value;
            currentProperty.Address = TBAddress.Text.Trim();
            currentProperty.Area = area;
            if (TBRooms.Text == "")
                currentProperty.Rooms = null;
            else
                currentProperty.Rooms = rooms;
            currentProperty.Price = price;
            currentProperty.Photo = TBPhoto.Text == "" ? null : TBPhoto.Text;

            try
            {
                if (currentProperty.ID == 0)
                    Entities.GetContext().Properties.Add(currentProperty);
                Entities.GetContext().SaveChanges();
                Helper.ShowInfo("Данные успешно сохранены!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                Helper.ShowError(ex.Message);
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            CBType.SelectedIndex = -1;
            CBUser.SelectedIndex = -1;
            DPDate.SelectedDate = DateTime.Today;
            TBAddress.Text = "";
            TBArea.Text = "";
            TBRooms.Text = "";
            TBPrice.Text = "";
            TBPhoto.Text = "";
        }

        private void ButtonPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Изображения (*.jpg;*.png)|*.jpg;*.jpeg;*.png";
            if (dialog.ShowDialog() == true)
                TBPhoto.Text = dialog.FileName;
        }

        private void Digits_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void Area_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !(char.IsDigit(e.Text, 0) || e.Text == "," || e.Text == ".");
        }
    }
}
