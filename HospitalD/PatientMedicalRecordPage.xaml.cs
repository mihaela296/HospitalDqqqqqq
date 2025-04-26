using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HospitalD
{
    public partial class PatientMedicalRecordPage : Page
    {
        private readonly HospitalDRmEntities _db = new HospitalDRmEntities();
        private Patients _patient;

        public PatientMedicalRecordPage(Patients patient)
        {
            InitializeComponent();
            _patient = patient;
            LoadMedicalRecords();
        }

        private void LoadMedicalRecords()
        {
            var records = _db.PatientMedicalRecord
                .Include(r => r.Diagnoses)
                .Include(r => r.MedicalProcedures)
                .Include(r => r.Medications)
                .Include(r => r.Staff)
                .Include(r => r.Departments)
                .Where(r => r.ID_Patient == _patient.ID_Patient)
                .OrderByDescending(r => r.RecordDate)
                .ToList();

            MedicalRecordsDataGrid.ItemsSource = records;
        }

        private void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            var fromDate = DateFromFilter.SelectedDate;
            var toDate = DateToFilter.SelectedDate;

            var records = _db.PatientMedicalRecord
                .Include(r => r.Diagnoses)
                .Include(r => r.MedicalProcedures)
                .Include(r => r.Medications)
                .Include(r => r.Staff)
                .Include(r => r.Departments)
                .Where(r => r.ID_Patient == _patient.ID_Patient)
                .AsQueryable();

            if (fromDate.HasValue)
                records = records.Where(r => r.RecordDate >= fromDate);

            if (toDate.HasValue)
                records = records.Where(r => r.RecordDate <= toDate);

            MedicalRecordsDataGrid.ItemsSource = records
                .OrderByDescending(r => r.RecordDate)
                .ToList();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            DateFromFilter.SelectedDate = null;
            DateToFilter.SelectedDate = null;
            LoadMedicalRecords();
        }
    }
}