
using EmployeeManagementSystem.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Maui.Controls;
using SQLite;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace EmployeeManagementSystem.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _database;


        public DatabaseService()
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "employees.db3");
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Employee>().Wait();
            _database.CreateTableAsync<UserAccount>().Wait();
            _database.CreateTableAsync<LeaveRequest>().Wait();
            // Add default user
            //AddDefaultUserAsync().Wait();
        }


        // SQLite database connection string
        private readonly string _connectionString = "Data Source=EmployeeManagement.db";


        // Initialize the SQLite database with tables for employees, departments, and leave requests
        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Employees (
            EmployeeID INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Department TEXT,
            Position TEXT,
            Email TEXT
        );

        CREATE TABLE IF NOT EXISTS Departments (
            DepartmentID INTEGER PRIMARY KEY AUTOINCREMENT,
            DepartmentName TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS LeaveRequests (
            LeaveRequestID INTEGER PRIMARY KEY AUTOINCREMENT,
            EmployeeID INTEGER,
            StartDate TEXT,
            EndDate TEXT,
            Reason TEXT,
            ApprovalStatus TEXT,
            FOREIGN KEY (EmployeeID) REFERENCES Employees (EmployeeID)
        );
    ";
            command.ExecuteNonQuery();
        }


        public async Task AddLeaveRequestAsync(LeaveRequest leaveRequest)
        {
            await _database.InsertAsync(leaveRequest);
        }

        public async Task<List<LeaveRequest>> GetLeaveRequestsAsync()
        {
            return await _database.Table<LeaveRequest>().ToListAsync();
        }

        public async Task UpdateLeaveRequestAsync(LeaveRequest leaveRequest)
        {
            await _database.UpdateAsync(leaveRequest);
        }

        public async Task DeleteLeaveRequestAsync(int leaveRequestId)
        {
            var leaveRequest = await _database.Table<LeaveRequest>()
                                              .FirstOrDefaultAsync(r => r.LeaveRequestID == leaveRequestId);
            if (leaveRequest != null)
            {
                await _database.DeleteAsync(leaveRequest);
            }
        }

        public async Task UpdateEmployeeAsync(Employee employee)
        {
            await _database.UpdateAsync(employee);
        }

        public async Task DeleteEmployeeAsync(int employeeId)
        {
            var employee = await _database.Table<Employee>()
                                           .FirstOrDefaultAsync(e => e.EmployeeID == employeeId);
            if (employee != null)
            {
                await _database.DeleteAsync(employee);
            }
        }


        public async Task AddEmployeeAsync(Employee employee)
        {
            await _database.InsertAsync(employee);
        }

        public async Task<List<Employee>> GetEmployeesAsync()
        {
            return await _database.Table<Employee>().ToListAsync();
        }

        public async Task<List<Employee>> GetEmployeesByDepartmentAsync(string department)
        {
            return await _database.Table<Employee>()
                                  .Where(e => e.Department.ToLower() == department.ToLower())
                                  .ToListAsync();
        }

        public async Task<List<Employee>> GetEmployeesByNameAsync(string name)
        {
            return await _database.Table<Employee>()
                                  .Where(e => e.Name.ToLower().Contains(name.ToLower()))
                                  .ToListAsync();
        }

        public async Task<List<Employee>> GetEmployeesByPositionAsync(string position)
        {
            return await _database.Table<Employee>()
                                  .Where(e => e.Position.ToLower() == position.ToLower())
                                  .ToListAsync();
        }

        public async Task<List<UserAccount>> GetUserAccountsAsync()
        {
            return await _database.Table<UserAccount>().ToListAsync();
        }
        public async Task AddUserAccountAsync(UserAccount userAccount)
        {
            await _database.InsertAsync(userAccount);
        }
        public async Task<UserAccount> AuthenticateUserAsync(string email)
        {
            return await _database.Table<UserAccount>()
                                      .FirstOrDefaultAsync(u => u.Email == email);
        }
        public async Task<bool> UpdateUserEmailAndPasswordAsync(int userId, string newEmail, string newPassword)
        {
            var user = await _database.Table<UserAccount>().FirstOrDefaultAsync(u => u.UserID == userId);
            if (user != null)
            {
                user.Email = newEmail;
                user.PasswordHash = newPassword; 
                await _database.UpdateAsync(user);
                return true;
            }
            return false;
        }
    }
}
