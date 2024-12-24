using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagementSystem.Views
{
    public partial class AdminSettings : ContentPage
    {
        private readonly DatabaseService _databaseService;
        public ObservableCollection<Employee> Employees { get; set; } = new ObservableCollection<Employee>();
        public ObservableCollection<Department> Departments { get; set; } = new ObservableCollection<Department>();
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

        public AdminSettings()
        {
            InitializeComponent();
            _databaseService = App.Database; // Reference DatabaseService
            BindingContext = this;
            ProfileImageSource = Preferences.Get("ProfileImagePath", "logo_app.png");

            LoadEmployeesAsync(); // Load employees from the database
        }

        private async void LoadEmployeesAsync()
        {
            try
            {
                var employees = await _databaseService.GetEmployeesAsync();
                Employees = new ObservableCollection<Employee>(employees);

                // Populate Departments based on employees
                Departments.Clear();
                foreach (var employee in employees)
                {
                    var department = Departments.FirstOrDefault(d => d.DepartmentName == employee.Department);
                    if (department != null)
                    {
                        department.Employees.Add(employee);
                    }
                    else
                    {
                        Departments.Add(new Department
                        {
                            DepartmentName = employee.Department,
                            Employees = new ObservableCollection<Employee> { employee }
                        });
                    }
                }

                OnPropertyChanged(nameof(Employees));
                OnPropertyChanged(nameof(Departments));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load employees: {ex.Message}", "OK");
            }
        }

        private async void OnAddEmployeeButtonClicked(object sender, EventArgs e)
        {
            string idInput = await DisplayPromptAsync("New Employee", "Enter the employee's ID:");
            if (string.IsNullOrWhiteSpace(idInput) || !int.TryParse(idInput, out int id))
            {
                await DisplayAlert("Error", "Please enter a valid numeric ID.", "OK");
                return;
            }

            string name = await DisplayPromptAsync("New Employee", "Enter the employee's name:");
            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Error", "Employee name cannot be empty.", "OK");
                return;
            }

            string department = await DisplayPromptAsync("New Employee", "Enter the employee's department:");
            if (string.IsNullOrWhiteSpace(department))
            {
                await DisplayAlert("Error", "Department cannot be empty.", "OK");
                return;
            }

            string email = await DisplayPromptAsync("New Employee", "Enter the employee's email:");
            if (string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Error", "Email cannot be empty.", "OK");
                return;
            }

            string position = await DisplayPromptAsync("New Employee", "Enter the employee's position:");
            if (string.IsNullOrWhiteSpace(position))
            {
                await DisplayAlert("Error", "Position cannot be empty.", "OK");
                return;
            }

            var newEmployee = new Employee { EmployeeID = id, Name = name, Department = department, Email = email, Position = position };

            try
            {
                await _databaseService.AddEmployeeAsync(newEmployee);
                Employees.Add(newEmployee);

                // Update Departments
                var existingDepartment = Departments.FirstOrDefault(d => d.DepartmentName == department);
                if (existingDepartment != null)
                {
                    existingDepartment.Employees.Add(newEmployee);
                }
                else
                {
                    Departments.Add(new Department { DepartmentName = department, Employees = new ObservableCollection<Employee> { newEmployee } });
                }

                await DisplayAlert("Success", "Employee added successfully!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to add employee: {ex.Message}", "OK");
            }
        }

        private async void OnEditEmployeeButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var employee = button?.BindingContext as Employee;
            if (employee == null) return;

            string name = await DisplayPromptAsync("Edit Employee", "Enter the employee's name:", initialValue: employee.Name);
            if (!string.IsNullOrWhiteSpace(name)) employee.Name = name;

            string department = await DisplayPromptAsync("Edit Employee", "Enter the employee's department:", initialValue: employee.Department);
            if (!string.IsNullOrWhiteSpace(department)) employee.Department = department;

            string email = await DisplayPromptAsync("Edit Employee", "Enter the employee's email:", initialValue: employee.Email);
            if (!string.IsNullOrWhiteSpace(email)) employee.Email = email;

            string position = await DisplayPromptAsync("Edit Employee", "Enter the employee's position:", initialValue: employee.Position);
            if (!string.IsNullOrWhiteSpace(position)) employee.Position = position;

            try
            {
                await _databaseService.UpdateEmployeeAsync(employee);
                await DisplayAlert("Success", "Employee updated successfully!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to update employee: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteEmployeeButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var employee = button?.BindingContext as Employee;
            if (employee == null) return;

            bool confirm = await DisplayAlert("Delete Employee", $"Are you sure you want to delete {employee.Name}?", "Yes", "No");
            if (!confirm) return;

            try
            {
                // Remove employee from the database
                await _databaseService.DeleteEmployeeAsync(employee.EmployeeID);

                // Remove employee from the local collection
                Employees.Remove(employee);

                // Remove from the department's employee list
                var department = Departments.FirstOrDefault(d => d.DepartmentName == employee.Department);
                department?.Employees.Remove(employee);

                // Remove the department if no employees are left
                if (department != null && !department.Employees.Any())
                {
                    Departments.Remove(department);
                }

                await DisplayAlert("Success", "Employee deleted successfully!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete employee: {ex.Message}", "OK");
            }
        }


        private async void OnAddDepartmentButtonClicked(object sender, EventArgs e)
        {
            string departmentName = await Application.Current.MainPage.DisplayPromptAsync("New Department", "Enter the name of the department:");
            if (string.IsNullOrWhiteSpace(departmentName))
            {
                await DisplayAlert("Error", "Department name cannot be empty.", "OK");
                return;
            }

            if (Departments.Any(d => d.DepartmentName.Equals(departmentName, StringComparison.OrdinalIgnoreCase)))
            {
                await DisplayAlert("Error", "A department with this name already exists.", "OK");
                return;
            }

            Departments.Add(new Department
            {
                DepartmentName = departmentName,
                Employees = new ObservableCollection<Employee>()
            });

            await DisplayAlert("Success", $"Department '{departmentName}' added successfully!", "OK");
        }

        private async void OnDashboardButtonClicked(object sender, EventArgs e) => await App.NavigateToPage(new Dashboard());
        private async void OnEmployeeManagementButtonClicked(object sender, EventArgs e) => await App.NavigateToPage(new EmployeeManagement());
        private async void OnUserSettingsButtonClicked(object sender, EventArgs e) => await App.NavigateToPage(new UserSettings());
        private async void OnLeaveRequestsButtonClicked(object sender, EventArgs e) => await App.NavigateToPage(new LeaveRequests());
        private async void OnLogOutButtonClicked(object sender, EventArgs e) => await App.NavigateToPage(new LoginView());
    }
}
