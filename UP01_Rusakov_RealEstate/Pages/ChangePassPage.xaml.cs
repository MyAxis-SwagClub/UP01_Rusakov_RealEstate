using System;
using System.Windows;
using System.Windows.Controls;
using UP01_Rusakov_RealEstate.Classes;
using UP01_Rusakov_RealEstate.Model;

namespace UP01_Rusakov_RealEstate.Pages
{
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) || string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password) || string.IsNullOrEmpty(TbLogin.Text))
            {
                Helper.ShowError("Все поля обязательны к заполнению!");
                return;
            }

            string hashedPass = Helper.GetHash(CurrentPasswordBox.Password);
            User user = null;
            foreach (User u in Entities.GetContext().Users)
            {
                if (u.Login == TbLogin.Text && u.Password == hashedPass)
                    user = u;
            }
            if (user == null)
            {
                Helper.ShowError("Текущий пароль или логин неверный!");
                return;
            }

            string passwordError = Helper.CheckPassword(NewPasswordBox.Password);
            if (passwordError != null)
            {
                Helper.ShowError(passwordError);
                return;
            }
            if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
            {
                Helper.ShowError("Пароли не совпадают!");
                return;
            }

            try
            {
                user.Password = Helper.GetHash(NewPasswordBox.Password);
                Entities.GetContext().SaveChanges();
                Helper.ShowInfo("Пароль успешно изменен!");
                NavigationService.Navigate(new AuthPage());
            }
            catch (Exception ex)
            {
                Helper.ShowError(ex.Message);
            }
        }

        private void TbLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintLogin.Visibility = TbLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void CurrentPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintCurrent.Visibility = CurrentPasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintNew.Visibility = NewPasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintConfirm.Visibility = ConfirmPasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
