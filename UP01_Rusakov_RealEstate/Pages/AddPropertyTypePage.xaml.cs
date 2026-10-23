using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Rusakov_RealEstate.Classes;
using UP01_Rusakov_RealEstate.Model;

namespace UP01_Rusakov_RealEstate.Pages
{
    public partial class AddPropertyTypePage : Page
    {
        private PropertyType currentType = new PropertyType();

        public AddPropertyTypePage(PropertyType selectedType)
        {
            InitializeComponent();

            if (selectedType != null)
            {
                currentType = selectedType;
                Title = "Редактирование типа недвижимости";
                TBTypeName.Text = currentType.Name;
            }
            else
                Title = "Добавление типа недвижимости";
            HeaderText.Text = Title;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            string name = TBTypeName.Text.Trim();

            if (name == "")
            {
                Helper.ShowError("Укажите название типа недвижимости!");
                return;
            }
            if (name.Length < 2)
            {
                Helper.ShowError("Название должно быть не короче 2 символов!");
                return;
            }
            foreach (PropertyType t in Entities.GetContext().PropertyTypes)
            {
                if (t.Name == name && t.ID != currentType.ID)
                {
                    Helper.ShowError("Такой тип недвижимости уже существует!");
                    return;
                }
            }

            currentType.Name = name;
            try
            {
                if (currentType.ID == 0)
                    Entities.GetContext().PropertyTypes.Add(currentType);
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
            TBTypeName.Text = "";
        }

        private void TBTypeName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[А-Яа-яЁё\- ]+$");
        }
    }
}
