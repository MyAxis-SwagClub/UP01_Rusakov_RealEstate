using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Rusakov_RealEstate.Classes;
using UP01_Rusakov_RealEstate.Model;

namespace UP01_Rusakov_RealEstate.Pages
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
        }

        private void RegButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtbxLog.Text) || string.IsNullOrEmpty(txtbxFIO.Text) ||
                string.IsNullOrEmpty(passBxFrst.Password) || string.IsNullOrEmpty(passBxScnd.Password))
            {
                Helper.ShowError("Заполните все поля!");
                return;
            }

            if (txtbxLog.Text.Length < 3)
            {
                Helper.ShowError("Логин должен быть не короче 3 символов!");
                return;
            }
            if (!Regex.IsMatch(txtbxFIO.Text, @"^[А-Яа-яЁё\- ]+$") || !txtbxFIO.Text.Trim().Contains(" "))
            {
                Helper.ShowError("Введите ФИО русскими буквами (минимум фамилию и имя)!");
                return;
            }

            foreach (User u in Entities.GetContext().Users)
            {
                if (u.Login == txtbxLog.Text)
                {
                    Helper.ShowError("Пользователь с таким логином уже существует!");
                    return;
                }
            }

            string passwordError = Helper.CheckPassword(passBxFrst.Password);
            if (passwordError != null)
            {
                Helper.ShowError(passwordError);
                return;
            }

            if (passBxFrst.Password != passBxScnd.Password)
            {
                Helper.ShowError("Пароли не совпадают!");
                return;
            }

            User user = new User
            {
                Login = txtbxLog.Text,
                Password = Helper.GetHash(passBxFrst.Password),
                FIO = txtbxFIO.Text.Trim(),
                Role = "User"
            };
            try
            {
                Entities.GetContext().Users.Add(user);
                Entities.GetContext().SaveChanges();
                Helper.ShowInfo("Пользователь успешно зарегистрирован!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                Helper.ShowError(ex.Message);
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void Login_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9._@-]+$");
        }

        private void Fio_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[А-Яа-яЁё\- ]+$");
        }

        private void TxtbxLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintLogin.Visibility = txtbxLog.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void TxtbxFIO_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintFio.Visibility = txtbxFIO.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PassBxFrst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintPass.Visibility = passBxFrst.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PassBxScnd_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintPassSecond.Visibility = passBxScnd.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
