using System.Linq;
using System.Windows;

namespace HospitalD
{
    public partial class PatientPage : Window
    {
        private Users _user;
        private Patients _patient;

        public PatientPage(Users user)
        {
            InitializeComponent();
            _user = user;

            using (var db = new HospitalDRmEntities())
            {
                // Ищем пациента по Username
                _patient = db.Patients.FirstOrDefault(p => p.Username == user.Username);

                if (_patient == null)
                {
                    MessageBox.Show("Данные пациента не найдены!");
                    this.Close();
                    return;
                }
            }
        }
        // ... остальной код без изменений

        private void MedicalRecord_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PatientMedicalRecordPage(_patient));
        }

        private void Appointment_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MakeAppointmentPage(_patient));
        }

        private void MyAppointments_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PatientAppointmentsPage(_patient));
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
}