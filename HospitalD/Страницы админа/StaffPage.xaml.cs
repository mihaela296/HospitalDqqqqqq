using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HospitalD
{
    public partial class StaffPage : Page
    {
        private readonly HospitalDRmEntities _db = new HospitalDRmEntities();

        public StaffPage()
        {
            InitializeComponent();
            LoadStaff();
            InitializeDepartmentFilter();
        }

        private void InitializeDepartmentFilter()
        {
            DepartmentFilter.Items.Clear();
            DepartmentFilter.Items.Add(new ComboBoxItem() { Content = "Все отделения" });

            var departments = _db.Departments
                .OrderBy(d => d.Name)
                .ToList();

            foreach (var department in departments)
            {
                DepartmentFilter.Items.Add(new ComboBoxItem()
                {
                    Content = department.Name,
                    Tag = department.ID_Department
                });
            }

            DepartmentFilter.SelectedIndex = 0;
        }

        private void LoadStaff()
        {
            // Сначала получаем все данные из базы
            var allStaff = _db.Staff
                .Include(s => s.Departments)
                .Include(s => s.Positions)
                .Include(s => s.Roles)
                .AsNoTracking()
                .ToList();  // Материализуем запрос, чтобы работать с данными в памяти

            // Применяем фильтры к материализованным данным
            var filteredStaff = allStaff.AsQueryable();

            // Фильтрация по отделению
            if (DepartmentFilter.SelectedIndex > 0 &&
                DepartmentFilter.SelectedItem is ComboBoxItem selectedDepartmentItem &&
                selectedDepartmentItem.Tag is int departmentId)
            {
                filteredStaff = filteredStaff.Where(s => s.ID_Department == departmentId);
            }

            // Фильтрация по фамилии (теперь работает, так как данные в памяти)
            if (!string.IsNullOrWhiteSpace(SearchLastName.Text))
            {
                string searchTerm = SearchLastName.Text.ToLower();
                filteredStaff = filteredStaff.Where(s =>
                    s.FullName.ToLower().Split(' ').FirstOrDefault() == searchTerm ||
                    s.FullName.ToLower().Contains(searchTerm));
            }

            // Сортировка
            switch (SortStaffComboBox.SelectedIndex)
            {
                case 0: filteredStaff = filteredStaff.OrderBy(s => s.FullName); break;
                case 1: filteredStaff = filteredStaff.OrderBy(s => s.FullName); break;
                case 2: filteredStaff = filteredStaff.OrderByDescending(s => s.FullName); break;
                case 3: filteredStaff = filteredStaff.OrderBy(s => s.Positions.Name); break;
                case 4: filteredStaff = filteredStaff.OrderByDescending(s => s.Positions.Name); break;
            }

            StaffDataGrid.ItemsSource = filteredStaff.ToList();
        }

        private void DepartmentFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStaff();
        }

        private void SearchLastName_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadStaff();
        }

        private void SortStaffComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStaff();
        }

        private void CleanFilter_OnClick(object sender, RoutedEventArgs e)
        {
            SearchLastName.Text = string.Empty;
            SortStaffComboBox.SelectedIndex = 0;
            DepartmentFilter.SelectedIndex = 0;
            LoadStaff();
        }

        private void ButtonEdit_OnClick(object sender, RoutedEventArgs e)
        {
            if (StaffDataGrid.SelectedItem is Staff selectedStaff)
            {
                NavigationService.Navigate(new AddEditStaffPage(selectedStaff));
            }
            else
            {
                MessageBox.Show("Выберите сотрудника для редактирования!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditStaffPage());
        }

        private void ButtonDel_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(StaffDataGrid.SelectedItem is Staff selectedStaff))
            {
                MessageBox.Show("Выберите сотрудника для удаления!",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить сотрудника '{selectedStaff.FullName}'?", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                var staffToDelete = _db.Staff.Find(selectedStaff.ID_Staff);
                if (staffToDelete != null)
                {
                    _db.Staff.Remove(staffToDelete);
                    _db.SaveChanges();
                    LoadStaff();
                    MessageBox.Show("Сотрудник успешно удален!",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException)
            {
                MessageBox.Show("Невозможно удалить сотрудника, так как он связан с другими записями в базе данных.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.InnerException?.Message ?? ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}