using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class AddEditStaffPage : Page
    {
        private Staff _currentStaff;
        private readonly HospitalDRmEntities _db = new HospitalDRmEntities();

        public AddEditStaffPage(Staff selectedStaff = null)
        {
            InitializeComponent();

            _currentStaff = selectedStaff ?? new Staff();

            if (selectedStaff == null)
            {
                TextBlockTitle.Text = "Добавление сотрудника";
                TextBoxPassword.Visibility = Visibility.Visible;
                TextBlockPassword.Visibility = Visibility.Visible;
            }
            else
            {
                TextBlockTitle.Text = "Редактирование сотрудника";
                TextBoxPassword.Visibility = Visibility.Collapsed;
                TextBlockPassword.Visibility = Visibility.Collapsed;

                _currentStaff = _db.Staff
                    .Include(s => s.Departments)
                    .Include(s => s.Positions)
                    .Include(s => s.Roles)
                    .FirstOrDefault(s => s.ID_Staff == selectedStaff.ID_Staff);
                
                if (_currentStaff == null)
                {
                    MessageBox.Show("Сотрудник не найден в базе данных!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService.GoBack();
                    return;
                }
            }

            LoadStaffData();
        }

        private void LoadStaffData()
        {
            CbDepartment.ItemsSource = _db.Departments.ToList();
            CbPosition.ItemsSource = _db.Positions.ToList();

            if (_currentStaff.ID_Staff != 0)
            {
                TextBoxFullName.Text = _currentStaff.FullName;
                CbDepartment.SelectedValue = _currentStaff.ID_Department;
                CbPosition.SelectedValue = _currentStaff.ID_Position;
                TextBoxPhone.Text = _currentStaff.Phone;
                TextBoxEmail.Text = _currentStaff.Email;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs()) return;

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var selectedDepartment = CbDepartment.SelectedItem as Departments;
                    var selectedPosition = CbPosition.SelectedItem as Positions;

                    _currentStaff.FullName = TextBoxFullName.Text.Trim();
                    _currentStaff.ID_Department = selectedDepartment.ID_Department;
                    _currentStaff.ID_Position = selectedPosition.ID_Position;
                    //_currentStaff.ID_Role = selectedRole.ID_Role;
                    _currentStaff.ID_Role = 1;
                    _currentStaff.Phone = TextBoxPhone.Text.Trim();
                    _currentStaff.Email = TextBoxEmail.Text.Trim();

                    string password = "";
                    if (_currentStaff.ID_Staff == 0)
                    {
                        password = string.IsNullOrEmpty(TextBoxPassword.Text)
                            ? GenerateTempPassword()
                            : TextBoxPassword.Text;

                        _currentStaff.Password = HashPassword(password);
                        _db.Staff.Add(_currentStaff);
                    }

                    _db.SaveChanges();
                    transaction.Commit();

                    if (_currentStaff.ID_Staff == 0)
                    {
                        MessageBox.Show($"Сотрудник добавлен! Пароль: {password}",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Данные сотрудника сохранены!",
                            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    NavigationService.GoBack();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    HandleException(ex);
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _db.Dispose();
            NavigationService.GoBack();
        }

        private void HandleException(Exception ex)
        {
            if (ex is DbEntityValidationException validationEx)
            {
                var errorMessages = validationEx.EntityValidationErrors
                    .SelectMany(x => x.ValidationErrors)
                    .Select(x => $"{x.PropertyName}: {x.ErrorMessage}");

                MessageBox.Show("Ошибки валидации:\n" + string.Join("\n", errorMessages),
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (ex is System.Data.Entity.Infrastructure.DbUpdateException dbEx)
            {
                string errorMessage = dbEx.InnerException?.InnerException?.Message
                                  ?? dbEx.InnerException?.Message
                                  ?? dbEx.Message;

                MessageBox.Show($"Ошибка базы данных: {errorMessage}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private string GenerateTempPassword()
        {
            const string chars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(TextBoxFullName.Text) ||
                CbDepartment.SelectedItem == null ||
                CbPosition.SelectedItem == null ||
                string.IsNullOrWhiteSpace(TextBoxPhone.Text))
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (_currentStaff.ID_Staff == 0 && string.IsNullOrEmpty(TextBoxPassword.Text))
            {
                MessageBox.Show("Введите пароль для нового сотрудника!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void CbRole_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}