using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using UP01_Rusakov_RealEstate.Classes;
using UP01_Rusakov_RealEstate.Model;

namespace UP01_Rusakov_RealEstate.Pages
{
    public partial class AddUserPage : Page
    {
        private User currentUser = new User();

        public AddUserPage(User selectedUser)
        {
            InitializeComponent();

            if (selectedUser != null)
            {
                currentUser = selectedUser;
                Title = "Редактирование пользователя";
                PassLabel.Text = "Новый пароль (оставьте пустым, чтобы не менять)";

                TBLogin.Text = currentUser.Login;
                TBFio.Text = currentUser.FIO;
                TBPhoto.Text = currentUser.Photo;
                cmbRole.SelectedIndex = currentUser.Role == "Admin" ? 1 : 0;
            }
            else
            {
                Title = "Добавление пользователя";
                cmbRole.SelectedIndex = 0;
            }
            HeaderText.Text = Title;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(TBLogin.Text))
                errors.AppendLine("Укажите логин!");
            else if (TBLogin.Text.Length < 3)
                errors.AppendLine("Логин должен быть не короче 3 символов!");
            else
            {
                foreach (User u in Entities.GetContext().Users)
                {
                    if (u.Login == TBLogin.Text && u.ID != currentUser.ID)
                        errors.AppendLine("Пользователь с таким логином уже существует!");
                }
            }

            if (currentUser.ID == 0 && TBPass.Password == "")
                errors.AppendLine("Укажите пароль!");
            else if (TBPass.Password != "" && Helper.CheckPassword(TBPass.Password) != null)
                errors.AppendLine(Helper.CheckPassword(TBPass.Password));

            if (cmbRole.SelectedItem == null)
                errors.AppendLine("Выберите роль!");

            if (string.IsNullOrWhiteSpace(TBFio.Text))
                errors.AppendLine("Укажите ФИО!");
            else if (!TBFio.Text.Trim().Contains(" "))
                errors.AppendLine("Укажите хотя бы фамилию и имя!");

            if (errors.Length > 0)
            {
                Helper.ShowError(errors.ToString());
                return;
            }

            currentUser.Login = TBLogin.Text;
            currentUser.FIO = TBFio.Text.Trim();
            currentUser.Role = cmbRole.Text;
            currentUser.Photo = TBPhoto.Text == "" ? null : TBPhoto.Text;
            if (TBPass.Password != "")
                currentUser.Password = Helper.GetHash(TBPass.Password);

            try
            {
                if (currentUser.ID == 0)
                    Entities.GetContext().Users.Add(currentUser);
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
            TBLogin.Text = "";
            TBPass.Clear();
            cmbRole.SelectedIndex = 0;
            TBFio.Text = "";
            TBPhoto.Text = "";
        }

        private void ButtonPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Изображения (*.jpg;*.png)|*.jpg;*.jpeg;*.png";
            if (dialog.ShowDialog() == true)
                TBPhoto.Text = dialog.FileName;
        }

        private void Login_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9._@-]+$");
        }

        private void Fio_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[А-Яа-яЁё\- ]+$");
        }
    }
}
