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
    try
    {
        // Валидация ввода
        if (string.IsNullOrWhiteSpace(usernameRegTextBox.Text))
        {
            MessageBox.Show("Укажите логин!");
            return;
        }

        if (string.IsNullOrWhiteSpace(fullNameTextBox.Text))
        {
            MessageBox.Show("Укажите ваше полное имя!");
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

        using (var db = new HospitalDRmEntities())
        {
            // Проверка уникальности логина
            if (db.Patients.Any(p => p.Username == usernameRegTextBox.Text) || 
                db.Users.Any(u => u.Username == usernameRegTextBox.Text))
            {
                MessageBox.Show("Пользователь с таким логином уже существует.");
                return;
            }

            // Получаем значение пола с проверкой
            string gender = null;
            if (genderComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string genderValue = selectedItem.Content.ToString();
                gender = genderValue.StartsWith("М") ? "М" : 
                        genderValue.StartsWith("Ж") ? "Ж" : null;
            }

            // Создаем нового пациента
            var newPatient = new Patients
            {
                FullName = fullNameTextBox.Text.Trim(),
                Username = usernameRegTextBox.Text.Trim(), // Обязательное поле
                Password = GetHash(passwordRegBox.Password), // Хешируем пароль
                ID_Role = 3, // Роль пациента
                BirthDate = birthDatePicker.SelectedDate,
                Gender = gender,
                Phone = phoneNumberTextBox.Text.Trim(),
                Address = addressTextBox.Text.Trim(),
                Email = usernameRegTextBox.Text.Trim() // Если поле Email существует
            };

            // Создаем запись пользователя
            var newUser = new Users
            {
                Username = usernameRegTextBox.Text.Trim(),
                Password = GetHash(passwordRegBox.Password),
                ID_Role = 3
            };

            // Используем транзакцию для согласованности данных
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    db.Patients.Add(newPatient);
                    db.Users.Add(newUser);
                    
                    db.SaveChanges();
                    transaction.Commit();
                    
                    MessageBox.Show("Регистрация прошла успешно!");
                    NavigationService.Navigate(new AuthPage());
                }
                catch (DbEntityValidationException ex)
                {
                    transaction.Rollback();
                    var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => $"{x.PropertyName}: {x.ErrorMessage}");
                    MessageBox.Show(string.Join("\n", errorMessages), 
                                  "Ошибки валидации");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    MessageBox.Show($"Ошибка регистрации: {ex.Message}");
                }
            }
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Неожиданная ошибка: {ex.Message}");
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