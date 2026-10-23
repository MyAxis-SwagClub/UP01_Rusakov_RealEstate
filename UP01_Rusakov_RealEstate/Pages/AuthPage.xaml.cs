using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UP01_Rusakov_RealEstate.Classes;
using UP01_Rusakov_RealEstate.Model;

namespace UP01_Rusakov_RealEstate.Pages
{
    public partial class AuthPage : Page
    {
        private int failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
        }

        private void ButtonEnter_Click(object sender, RoutedEventArgs e)
        {
            if (CaptchaPanel.Visibility == Visibility.Visible)
                return;

            if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                Helper.ShowError("Введите логин и пароль!");
                return;
            }

            User user;
            try
            {
                user = Helper.Authenticate(TextBoxLogin.Text, PasswordBox.Password);
            }
            catch (Exception ex)
            {
                Helper.ShowError("Ошибка подключения к базе данных:\n" + ex.Message);
                return;
            }

            if (user == null)
            {
                failedAttempts++;
                Helper.ShowError("Пользователь с такими данными не найден!");
                PasswordBox.Clear();

                if (failedAttempts >= 3)
                    CaptchaSwitch();
                else
                    AttemptsText.Text = "Осталось попыток до капчи: " + (3 - failedAttempts);
                return;
            }

            Helper.CurrentUser = user;
            Helper.ShowInfo("Добро пожаловать, " + user.FIO + "!");

            if (user.Role == "Admin")
                NavigationService.Navigate(new AdminPage());
            else
                NavigationService.Navigate(new UserPage());
        }

        private void CaptchaSwitch()
        {
            if (CaptchaPanel.Visibility == Visibility.Collapsed)
            {
                LoginPanel.Visibility = Visibility.Collapsed;
                CaptchaPanel.Visibility = Visibility.Visible;
                captcha.Text = Helper.GenerateCaptcha();
                captchaInput.Clear();
            }
            else
            {
                CaptchaPanel.Visibility = Visibility.Collapsed;
                LoginPanel.Visibility = Visibility.Visible;
                TextBoxLogin.Clear();
                PasswordBox.Clear();
                AttemptsText.Text = "";
            }
        }

        private void SubmitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (captchaInput.Text != captcha.Text)
            {
                Helper.ShowError("Неверно введена капча");
                captcha.Text = Helper.GenerateCaptcha();
                captchaInput.Clear();
            }
            else
            {
                Helper.ShowInfo("Капча введена успешно, можете продолжить авторизацию");
                failedAttempts = 0;
                CaptchaSwitch();
            }
        }

        private void RefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            captcha.Text = Helper.GenerateCaptcha();
            captchaInput.Clear();
        }

        private void Captcha_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
                e.Handled = true;
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ChangePassPage());
        }

        private void Login_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Z0-9._@-]+$");
        }

        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintLogin.Visibility = TextBoxLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            HintPassword.Visibility = PasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void CaptchaInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            HintCaptcha.Visibility = captchaInput.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }
    }
}
