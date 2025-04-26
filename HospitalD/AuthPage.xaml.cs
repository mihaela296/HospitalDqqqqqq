using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text) ||
                string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль!");
                return;
            }

            string passwordHash = GetHash(passwordBox.Password);

            using (var db = new HospitalDRmEntities())
            {
                var user = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Username == usernameTextBox.Text &&
                                       u.Password == passwordHash);

                if (user == null)
                {
                    MessageBox.Show("Неправильный логин или пароль.");
                    return;
                }

                Window newWindow;
                switch (user.ID_Role)
                {
                    case 1:
                        newWindow = new AdminPage(user);
                        break;
                    case 2:
                        newWindow = new EmployeePage(user);
                        break;
                    case 3:
                        // Используем Username для поиска пациента
                        var patient = db.Patients.FirstOrDefault(p => p.Username == user.Username);
                        if (patient != null)
                        {
                            newWindow = new PatientPage(user);
                        }
                        else
                        {
                            MessageBox.Show("Ошибка: данные пациента не найдены!");
                            return;
                        }
                        break;
                    default:
                        MessageBox.Show("Неизвестная роль пользователя!");
                        return;
                }

                newWindow.Show();
                Application.Current.MainWindow.Close();
            }
        }

        private void NavigateToReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RegPage());
        }

        public static string GetHash(string password)
        {
            using (var hash = SHA1.Create())
            {
                return string.Concat(hash
                    .ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(x => x.ToString("X2")));
            }
        }
    }
}