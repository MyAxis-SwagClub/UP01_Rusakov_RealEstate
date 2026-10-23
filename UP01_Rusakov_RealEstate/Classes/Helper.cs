using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using UP01_Rusakov_RealEstate.Model;

namespace UP01_Rusakov_RealEstate.Classes
{
    public static class Helper
    {
        public static User CurrentUser;

        public static string GetHash(string password)
        {
            using (SHA256 hash = SHA256.Create())
            {
                byte[] bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                string result = "";
                foreach (byte b in bytes)
                    result += b.ToString("X2");
                return result;
            }
        }

        public static User Authenticate(string login, string password)
        {
            string hashedPassword = GetHash(password);
            using (Entities db = new Entities())
            {
                foreach (User user in db.Users)
                {
                    if (user.Login == login && user.Password == hashedPassword)
                        return user;
                }
            }
            return null;
        }

        public static string GenerateCaptcha()
        {
            string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabdefghjkmnpqrtuy23456789";
            Random r = new Random();
            string captcha = "";
            for (int i = 0; i < 5; i++)
                captcha += chars[r.Next(chars.Length)];
            return captcha;
        }

        public static string CheckPassword(string password)
        {
            if (password.Length < 6)
                return "Пароль слишком короткий, должно быть минимум 6 символов!";

            bool en = true;
            bool number = false;
            for (int i = 0; i < password.Length; i++)
            {
                if (password[i] >= '0' && password[i] <= '9')
                    number = true;
                else if (!((password[i] >= 'A' && password[i] <= 'Z') || (password[i] >= 'a' && password[i] <= 'z')))
                    en = false;
            }

            if (!en)
                return "Используйте только английскую раскладку!";
            if (!number)
                return "Добавьте хотя бы одну цифру!";
            return null;
        }

        public static void ShowInfo(string text)
        {
            MessageBox.Show(text, "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowError(string text)
        {
            MessageBox.Show(text, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public static bool Confirm(string text, string caption = "Подтверждение")
        {
            return MessageBox.Show(text, caption, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public static bool IsDarkTheme = false;

        private static string themeFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UP01_Rusakov_theme.txt");

        public static void ApplyTheme(string dictionaryName)
        {
            var uri = new Uri(dictionaryName, UriKind.Relative);
            ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;
            Application.Current.Resources.Clear();
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(resourceDict);

            IsDarkTheme = dictionaryName == "RusakovDictionary.xaml";
            File.WriteAllText(themeFile, dictionaryName);
        }

        public static void LoadSavedTheme()
        {
            if (File.Exists(themeFile) && File.ReadAllText(themeFile) == "RusakovDictionary.xaml")
                ApplyTheme("RusakovDictionary.xaml");
            else
                ApplyTheme("Dictionary.xaml");
        }
    }
}
