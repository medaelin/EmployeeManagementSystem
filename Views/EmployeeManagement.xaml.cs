using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace EmployeeManagementSystem.Views
{
    public partial class EmployeeManagement : ContentPage
    {
        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();
        public ObservableCollection<Employee> FilteredEmployees { get; set; } = new ObservableCollection<Employee>();
        private readonly DatabaseService _databaseService;
        private string _profileImageSource;

        public string ProfileImageSource
        {
            get => _profileImageSource;
            set
            {
                _profileImageSource = value;
                OnPropertyChanged();
            }
        }

        public EmployeeManagement()
        {
            InitializeComponent();
            _databaseService = App.Database; // Use DatabaseService from the App class
            ProfileImageSource = Preferences.Get("ProfileImagePath", "logo_app.png");

            LoadEmployeesAsync(); // Fetch live data
            BindingContext = this;
        }

        private async void LoadEmployeesAsync()
        {
            try
            {
                // Fetch employees from the database
                var employees = await _databaseService.GetEmployeesAsync();

                // Populate ObservableCollections
                Employees = new ObservableCollection<Employee>(employees);
                FilteredEmployees = new ObservableCollection<Employee>(Employees);

                // Notify UI of data changes
                OnPropertyChanged(nameof(Employees));
                OnPropertyChanged(nameof(FilteredEmployees));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load employees: {ex.Message}", "OK");
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue?.ToLower() ?? string.Empty;

            // Clear and repopulate the filtered list based on search text
            FilteredEmployees.Clear();

            var filtered = Employees.Where(emp =>
                emp.Name.ToLower().Contains(searchText) || // Match name
                emp.EmployeeID.ToString().Contains(searchText)); // Match employee ID

            foreach (var emp in filtered)
            {
                FilteredEmployees.Add(emp);
            }

            // Notify UI of data changes
            OnPropertyChanged(nameof(FilteredEmployees));
        }

        private async void OnAddEmployeeClicked(object sender, EventArgs e)
        {
            var newEmployee = new Employee
            {
                Name = "New Employee",
                Position = "New Position",
                Department = "New Department",
                Email = "new.email@example.com"
            };

            await _databaseService.AddEmployeeAsync(newEmployee);
            Employees.Add(newEmployee);
            FilteredEmployees.Add(newEmployee);
        }

        private async void OnUpdateEmployeeClicked(object sender, EventArgs e)
        {
            var selectedEmployee = Employees.FirstOrDefault(); // Replace with actual selection logic
            if (selectedEmployee != null)
            {
                selectedEmployee.Name = "Updated Name";
                selectedEmployee.Email = "updated.email@example.com";
                selectedEmployee.Department = "Updated Department";
                selectedEmployee.Position = "Updated Position";

                await _databaseService.UpdateEmployeeAsync(selectedEmployee);

                // Refresh employees list
                Employees.Clear();
                var allEmployees = await _databaseService.GetEmployeesAsync();
                foreach (var emp in allEmployees)
                {
                    Employees.Add(emp);
                }

                FilteredEmployees = new ObservableCollection<Employee>(Employees);
                OnPropertyChanged(nameof(FilteredEmployees));
            }
        }

        private async void OnDeleteEmployeeClicked(object sender, EventArgs e)
        {
            var selectedEmployee = FilteredEmployees.FirstOrDefault(); // Replace with actual selection logic
            if (selectedEmployee != null)
            {
                bool confirm = await DisplayAlert("Confirm", $"Delete {selectedEmployee.Name}?", "Yes", "No");
                if (confirm)
                {
                    await _databaseService.DeleteEmployeeAsync(selectedEmployee.EmployeeID);
                    Employees.Remove(selectedEmployee);
                    FilteredEmployees.Remove(selectedEmployee);
                }
            }
        }

        private void OnSearchButtonClicked(object sender, EventArgs e)
        {
            // Use the SearchEntry text for filtering
            var searchText = SearchEntry.Text?.ToLower() ?? string.Empty;

            // Clear and repopulate the filtered list based on search text
            FilteredEmployees.Clear();

            var filtered = Employees.Where(emp =>
                emp.Name.ToLower().Contains(searchText) || // Match name
                emp.EmployeeID.ToString().Contains(searchText)); // Match employee ID

            foreach (var emp in filtered)
            {
                FilteredEmployees.Add(emp);
            }

            // Notify UI of data changes
            OnPropertyChanged(nameof(FilteredEmployees));
        }

        private async void OnDashboardButtonClicked(object sender, EventArgs e) => await App.NavigateToPage(new Dashboard());
        private async void OnLogOutButtonClicked(object sender, EventArgs e) => await App.NavigateToPage(new LoginView());
        private async void OnLeaveRequestsButtonClicked(object sender, EventArgs e) => await App.NavigateToPage(new LeaveRequests());
        private async void OnAdminSettingsButtonClicked(object sender, EventArgs e) => await App.NavigateToPage(new AdminSettings());
        private async void OnUserSettingsButtonClicked(object sender, EventArgs e) => await App.NavigateToPage(new UserSettings());
    }
}
