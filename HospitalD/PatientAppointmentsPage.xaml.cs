using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class PatientAppointmentsPage : Page
    {
        private readonly HospitalDRmEntities _db = new HospitalDRmEntities();
        private Patients _patient;

        public PatientAppointmentsPage(Patients patient)
        {
            InitializeComponent();
            _patient = patient;
            LoadAppointments();
            StatusFilter.SelectedIndex = 0;
        }

        private void LoadAppointments()
        {
            var appointments = _db.PatientMedicalRecord
                .Include(a => a.Staff)
                .Include(a => a.Departments)
                .Where(a => a.ID_Patient == _patient.ID_Patient)
                .OrderByDescending(a => a.VisitDate)
                .ToList();

            AppointmentsDataGrid.ItemsSource = appointments;
        }

        private void StatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var appointments = _db.PatientMedicalRecord
                .Include(a => a.Staff)
                .Include(a => a.Departments)
                .Where(a => a.ID_Patient == _patient.ID_Patient)
                .AsQueryable();

            switch (StatusFilter.SelectedIndex)
            {
                case 1: // Предстоящие
                    appointments = appointments.Where(a => a.VisitDate >= DateTime.Today);
                    break;
                case 2: // Прошедшие
                    appointments = appointments.Where(a => a.VisitDate < DateTime.Today);
                    break;
            }

            AppointmentsDataGrid.ItemsSource = appointments
                .OrderByDescending(a => a.VisitDate)
                .ToList();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadAppointments();
        }

        private void ButtonCancel_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(AppointmentsDataGrid.SelectedItem is PatientMedicalRecord selectedAppointment))
            {
                MessageBox.Show("Выберите запись для отмены!");
                return;
            }

            if (selectedAppointment.VisitDate < DateTime.Today)
            {
                MessageBox.Show("Нельзя отменить прошедшую запись!");
                return;
            }

            if (MessageBox.Show($"Отменить запись на {selectedAppointment.VisitDate:dd.MM.yyyy}?",
                "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            try
            {
                _db.PatientMedicalRecord.Remove(selectedAppointment);
                _db.SaveChanges();
                LoadAppointments();
                MessageBox.Show("Запись успешно отменена!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отмене записи: {ex.Message}");
            }
        }
    }
}