using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class MakeAppointmentPage : Page
    {
        private readonly HospitalDRmEntities _db = new HospitalDRmEntities();
        private Patients _patient;

        public MakeAppointmentPage(Patients patient)
        {
            InitializeComponent();
            _patient = patient;
            LoadDepartments();
            AppointmentDatePicker.SelectedDate = DateTime.Today;
            TimeComboBox.SelectedIndex = 0;
        }

        private void LoadDepartments()
        {
            var departments = _db.Departments
                .OrderBy(d => d.Name)
                .ToList();

            DepartmentComboBox.ItemsSource = departments;
            DepartmentComboBox.DisplayMemberPath = "Name";
            DepartmentComboBox.SelectedValuePath = "ID_Department";
        }

        private void DepartmentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DepartmentComboBox.SelectedValue is int departmentId)
            {
                var doctors = _db.Staff
                    .Include(s => s.Positions)
                    .Where(s => s.ID_Department == departmentId &&
                               s.Positions.Name.Contains("Врач"))
                    .OrderBy(s => s.FullName)
                    .ToList();

                DoctorComboBox.ItemsSource = doctors;
                DoctorComboBox.DisplayMemberPath = "FullName";
                DoctorComboBox.SelectedValuePath = "ID_Staff";
            }
        }

        private void MakeAppointment_Click(object sender, RoutedEventArgs e)
        {
            if (DoctorComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите врача!");
                return;
            }

            if (!AppointmentDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату приема!");
                return;
            }

            var selectedDate = AppointmentDatePicker.SelectedDate.Value;
            if (selectedDate < DateTime.Today)
            {
                MessageBox.Show("Нельзя записаться на прошедшую дату!");
                return;
            }

            if (string.IsNullOrWhiteSpace(ComplaintsTextBox.Text))
            {
                MessageBox.Show("Опишите ваши жалобы!");
                return;
            }

            try
            {
                var appointment = new PatientMedicalRecord
                {
                    ID_Patient = _patient.ID_Patient,
                    ID_Staff = (int)DoctorComboBox.SelectedValue,
                    ID_Department = (int)DepartmentComboBox.SelectedValue,
                    VisitDate = selectedDate,
                    RecordDate = DateTime.Now,
                    AdmissionDate = selectedDate,
                    Notes = ComplaintsTextBox.Text
                };

                _db.PatientMedicalRecord.Add(appointment);
                _db.SaveChanges();

                MessageBox.Show("Вы успешно записаны на прием!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при записи: {ex.Message}");
            }
        }
    }
}