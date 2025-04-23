using System.Windows;

namespace HospitalD
{
    public partial class AdminPage : Window
    {
        private Users _user;
        public AdminPage(Users user)
        {
            InitializeComponent();
            _user = user;
        }

        private void Departments_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DepartmentsPage());
        }

        private void Positions_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PositionsPage());
        }

        private void Staff_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new StaffPage());
        }

        private void Patients_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PatientsPage());
        }

        private void Diagnoses_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DiagnosesPage());
        }

        private void MedicalProcedures_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MedicalProceduresPage());
        }

        private void Medications_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new MedicationsPage());
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            this.Close();
        }
    }
}