using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity.Validation; // Обязательно добавьте это пространство имен!

namespace HospitalD
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernameRegTextBox.Text))
            {
                MessageBox.Show("Укажите логин!");
                return;
            }

            if (passwordRegBox.Password.Length < 6)
            {
                MessageBox.Show("Пароль должен быть длиннее 6 символов!");
                return;
            }

            if (passwordRegBox.Password != confirmPasswordRegBox.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            var phoneRegex = new Regex(@"^\+7\d{10}$");
            if (!phoneRegex.IsMatch(phoneNumberTextBox.Text))
            {
                MessageBox.Show("Номер телефона должен быть в формате +7XXXXXXXXXX");
                return;
            }

            using (var db = new HospitalDRmEntities())
            {
                var existingUser = db.Users
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Username == usernameRegTextBox.Text);

                if (existingUser != null)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.");
                    return;
                }

                var newPatient = new Patients
                {
                    FullName = fullNameTextBox.Text,
                    BirthDate = birthDatePicker.SelectedDate ?? DateTime.Now,
                    Gender = (genderComboBox.SelectedItem as ComboBoxItem)?.Content.ToString().Substring(0, 1) ?? string.Empty,
                    Phone = phoneNumberTextBox.Text,
                    Address = addressTextBox.Text,
                    ID_Role = 3,
                    Password = passwordRegBox.Password
                };

                try // Добавляем блок try-catch для обработки исключений валидации
                {
                    db.Patients.Add(newPatient);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    // Вывод подробной информации об ошибках валидации
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            MessageBox.Show($"Property: {validationError.PropertyName}, Error: {validationError.ErrorMessage}");
                        }
                    }
                    return; // Прекращаем выполнение метода, чтобы избежать дальнейших проблем
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}");
                    return;
                }

                var newUser = new Users
                {
                    Username = usernameRegTextBox.Text,
                    Password = GetHash(passwordRegBox.Password),
                    ID_Role = 3
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                MessageBox.Show("Регистрация прошла успешно!");
                NavigationService.Navigate(new AuthPage());
            }
        }

        private void NavigateToAuth_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
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