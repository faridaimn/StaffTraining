using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Testing.Components.Model;

namespace Testing.Components.Data;

public class EmployeeService
{
    private readonly string _connectionString;

    public EmployeeService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found.");
    }

    #region GetEmployeeAsync
    public async Task<List<EmployeeProfile>> GetEmployeesAsync(
        string? searchName = null, 
        string? searchIC = null)
    {
        var employees = new List<EmployeeProfile>();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT * 
                FROM EmployeeProfiles 
                WHERE 1=1";
            
            if (!string.IsNullOrWhiteSpace(searchName)) query += " AND PersonName LIKE @Name";
            if (!string.IsNullOrWhiteSpace(searchIC)) query += " AND IC_No LIKE @IC";

            using var employeeCommand = new SqlCommand(query, connection);

            if (!string.IsNullOrWhiteSpace(searchName)) employeeCommand.Parameters.AddWithValue("@Name", "%" + searchName + "%");
            if (!string.IsNullOrWhiteSpace(searchIC)) employeeCommand.Parameters.AddWithValue("@IC", "%" + searchIC + "%");

            using var employeeReader = await employeeCommand.ExecuteReaderAsync();

            while (await employeeReader.ReadAsync())
            {
                var emp = new EmployeeProfile
                {
                    IC_No = employeeReader["IC_No"]?.ToString() ?? "",
                    PersonName = employeeReader["PersonName"]?.ToString() ?? "",
                    DescriptionOfPerson = employeeReader["DescriptionOfPerson"]?.ToString(),
                    BirthDate = employeeReader["BirthDate"] != DBNull.Value ? Convert.ToDateTime(employeeReader["BirthDate"]) : null,
                    nationality_id = employeeReader["nationality_id"] != DBNull.Value ? Convert.ToInt32(employeeReader["nationality_id"]) : null,
                    Gender = employeeReader["Gender"]?.ToString() ?? "",
                    Hobby = employeeReader["Hobby"]?.ToString()?.Split('.').ToList() ?? new List<string>(),
                    SupportingDocumentPath = employeeReader["SupportingDocumentPath"]?.ToString(),
                    PersonPortraitPath = employeeReader["PersonPortraitPath"]?.ToString(),
                    MaritalStatus = employeeReader["MaritalStatus"]?.ToString()
                };
                employees.Add(emp);
            }

            await employeeReader.CloseAsync();
            await LoadVehiclesForEmployees(employees, connection);
        }
        catch (Exception)
        {
            throw;
        }

        return employees;
    }
    #endregion

    #region GetAllEmployeesAsync (without filter)

    public async Task<List<EmployeeProfile>> GetAllEmployeesAsync()
    {
        var employees = new List<EmployeeProfile>();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"SELECT * FROM EmployeeProfiles";

            using var employeeCommand = new SqlCommand(query, connection);
            using var employeeReader = await employeeCommand.ExecuteReaderAsync();

            while (await employeeReader.ReadAsync())
            {
                var emp = new EmployeeProfile
                {
                    IC_No = employeeReader["IC_No"]?.ToString() ?? "",
                    PersonName = employeeReader["PersonName"]?.ToString() ?? "",
                    DescriptionOfPerson = employeeReader["DescriptionOfPerson"]?.ToString(),
                    BirthDate = employeeReader["BirthDate"] != DBNull.Value ? Convert.ToDateTime(employeeReader["BirthDate"]) : null,
                    nationality_id = employeeReader["nationality_id"] != DBNull.Value ? Convert.ToInt32(employeeReader["nationality_id"]) : null,
                    Gender = employeeReader["Gender"]?.ToString() ?? "",
                    Hobby = employeeReader["Hobby"]?.ToString()?.Split('.').ToList() ?? new List<string>(),
                    SupportingDocumentPath = employeeReader["SupportingDocumentPath"]?.ToString(),
                    PersonPortraitPath = employeeReader["PersonPortraitPath"]?.ToString(),
                    MaritalStatus = employeeReader["MaritalStatus"]?.ToString()
                };
                employees.Add(emp);
            }

            await employeeReader.CloseAsync();
            await LoadVehiclesForEmployees(employees, connection);
        }
        catch (Exception)
        {
            throw;
        }

        return employees;
    }

    #endregion

    #region LoadVehiclesForEmployees

    private async Task LoadVehiclesForEmployees(List<EmployeeProfile> employees, SqlConnection connection)
    {
        if (!employees.Any()) return;

        var icNumbers = employees.Select(e => $"'{e.IC_No}'").ToList();
        var vehicleQuery = $@"
            SELECT * FROM vehicles 
            WHERE IC_No IN ({string.Join(",", icNumbers)})";

        using var vehicleCommand = new SqlCommand(vehicleQuery, connection);
        using var vehicleReader = await vehicleCommand.ExecuteReaderAsync();

        var employeeDict = employees.ToDictionary(e => e.IC_No);

        while (await vehicleReader.ReadAsync())
        {
            var vehicle = new Vehicle
            {
                Id = vehicleReader["id"] != DBNull.Value ? Convert.ToInt32(vehicleReader["Id"]) : 0,
                VehicleNo = vehicleReader["vehicle_no"]?.ToString() ?? "",
                Color = vehicleReader["color"].ToString() ?? "",
                brand = vehicleReader["brand"].ToString() ?? "",
                Model = vehicleReader["model"].ToString() ?? "",
                IC_No = vehicleReader["Ic_No"].ToString() ?? "",
                CreatedAt = vehicleReader["created_at"] != DBNull.Value ? Convert.ToDateTime(vehicleReader["created_at"]) : null
            };

            if(employeeDict.ContainsKey(vehicle.IC_No))
            {
                employeeDict[vehicle.IC_No].Vehicles.Add(vehicle);
            }
        }

    }

    #endregion

    #region CreateEmployeeAsync
    public async Task CreateEmployeeAsync(EmployeeProfile employee, List<Vehicle> vehicles)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        SqlTransaction transaction = (SqlTransaction)await connection.BeginTransactionAsync();

        try
        {
            var employeeQuery = @"
            INSERT INTO EmployeeProfiles 
            (IC_No, PersonName, DescriptionOfPerson, BirthDate, nationality_id, Gender, Hobby, SupportingDocumentPath, PersonPortraitPath, MaritalStatus) 
            VALUES 
            (@IC_No, @PersonName, @DescriptionOfPerson, @BirthDate, @Nationality_id, @Gender, @Hobby, @SupportingDocumentPath, @PersonPortraitPath, @MaritalStatus)";

            using var employeeCommand = new SqlCommand(employeeQuery, connection, transaction);
            AddEmployeeParameters(employeeCommand, employee);
            await employeeCommand.ExecuteNonQueryAsync();

            foreach (var vehicle in vehicles)
            {
                var vehicleQuery = @"
            INSERT INTO vehicles(vehicle_no, color, brand, model, IC_No)
            VALUES (@VehicleNo, @Color, @brand, @Model, @IC_No)";

                using var vehicleCommand = new SqlCommand(vehicleQuery, connection, transaction);
                vehicleCommand.Parameters.AddWithValue("@VehicleNo", vehicle.VehicleNo);
                vehicleCommand.Parameters.AddWithValue("@Color", vehicle.Color);
                vehicleCommand.Parameters.AddWithValue("@brand", vehicle.brand);
                vehicleCommand.Parameters.AddWithValue("@Model", vehicle.Model);
                vehicleCommand.Parameters.AddWithValue("@IC_No", vehicle.IC_No);
                await vehicleCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    #endregion

    #region UpdateEmployeeAsync
    public async Task UpdateEmployeeAsync(EmployeeProfile employee)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            UPDATE EmployeeProfiles SET
            PersonName = @PersonName, 
            DescriptionOfPerson = @DescriptionOfPerson,
            BirthDate = @BirthDate,
            nationality_id =@nationality_id,
            Gender = @Gender,
            Hobby = @Hobby,
            SupportingDocumentPath = @SupportingDocumentPath,
            PersonPortraitPath = @PersonPortraitPath
            WHERE IC_No = @IC_No";

        using var command = new SqlCommand(query, connection);
        AddEmployeeParameters(command, employee);
        
        await command.ExecuteNonQueryAsync();
    }
    #endregion

    #region UpdateEmployeeAsync 

    public async Task UpdateEmployeeAsync(EmployeeProfile employee, List<Vehicle> vehicles)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var employeeQuery = @"
            UPDATE EmployeeProfiles SET
            PersonName = @PersonName, 
            DescriptionOfPerson = @DescriptionOfPerson,
            BirthDate = @BirthDate,
            nationality_id = @nationality_id,
            Gender = @Gender,
            Hobby = @Hobby,
            SupportingDocumentPath = @SupportingDocumentPath,
            PersonPortraitPath = @PersonPortraitPath,
            MaritalStatus = @MaritalStatus
            WHERE IC_No = @IC_No";

            using var employeeCommand = new SqlCommand(employeeQuery, connection, (SqlTransaction)transaction);
            AddEmployeeParameters(employeeCommand, employee);
            await employeeCommand.ExecuteNonQueryAsync();

            var deleteQuery = "DELETE FROM vehicles WHERE IC_No = @IC_No";
            using var deleteCommand = new SqlCommand(deleteQuery, connection, (SqlTransaction)transaction);
            deleteCommand.Parameters.AddWithValue("@IC_No", employee.IC_No);
            await deleteCommand.ExecuteNonQueryAsync();

            foreach (var vehicle in vehicles)
            {
                var vehicleQuery = @"
                INSERT INTO vehicles(vehicle_no, color, brand, model, IC_No)
                VALUES (@VehicleNo, @Color, @brand, @Model, @IC_No)";

                using var vehicleCommand = new SqlCommand(vehicleQuery, connection, (SqlTransaction)transaction);
                vehicleCommand.Parameters.AddWithValue("@VehicleNo", vehicle.VehicleNo);
                vehicleCommand.Parameters.AddWithValue("@Color", vehicle.Color);
                vehicleCommand.Parameters.AddWithValue("@brand", vehicle.brand);
                vehicleCommand.Parameters.AddWithValue("@Model", vehicle.Model);
                vehicleCommand.Parameters.AddWithValue("@IC_No", employee.IC_No);
                await vehicleCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    #endregion

    #region DeleteEmployeeAsync
    public async Task DeleteEmployeeAsync(string icNo)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "DELETE FROM EmployeeProfiles WHERE IC_No = @IC_No";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IC_No", icNo);
        
        await command.ExecuteNonQueryAsync();
    }
    #endregion

    #region GetNationalitiesAsync

    public async Task<List<(int Id, string Code, string Name)>> GetNationalitiesAsync()
    {
        var list = new List<(int Id, string Code, string Name)>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT nationality_id, nationality_code, nationality_name FROM dbo.ref_Nationality";
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while(await reader.ReadAsync())
        {
            list.Add((
                Convert.ToInt32(reader["nationality_id"]),
                reader["nationality_code"].ToString() ?? "",
                reader["nationality_name"].ToString() ?? ""
                ));
        }

        return list;
    }

    #endregion

    #region UpdateVehicleAsync
    public async Task UpdateVehicleAsync(Vehicle vehicle)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            UPDATE vehicles SET
            vehicle_no = @VehicleNo,
            color = @Color,
            brand = @brand,
            model = @Model,
            IC_No = @IC_No
            WHERE id = @Id";

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@VehicleNo", vehicle.VehicleNo);
        command.Parameters.AddWithValue("@Color", vehicle.Color);
        command.Parameters.AddWithValue("@brand", vehicle.brand);
        command.Parameters.AddWithValue("@Model", vehicle.Model);
        command.Parameters.AddWithValue("@IC_No", vehicle.IC_No);
        command.Parameters.AddWithValue("@Id", vehicle.Id);
        await command.ExecuteNonQueryAsync();
    }
    #endregion

    #region AddVehiclesAsync
    public async Task AddVehiclesAsync(List<Vehicle> vehicles)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = await connection.BeginTransactionAsync();

        try
        {
            foreach (var vehicle in vehicles)
            {
                var query = @"
                    INSERT INTO vehicles(vehicle_no, color, brand, model, IC_No)
                    VALUES (@VehicleNo, @Color, @brand, @Model, @IC_No)";

                using var command = new SqlCommand(query, connection, (SqlTransaction)transaction);
                command.Parameters.AddWithValue("@VehicleNo", vehicle.VehicleNo);
                command.Parameters.AddWithValue("@Color", vehicle.Color);
                command.Parameters.AddWithValue("@brand", vehicle.brand);
                command.Parameters.AddWithValue("@Model", vehicle.Model);
                command.Parameters.AddWithValue("@IC_No", vehicle.IC_No);
                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    #endregion

    #region DeleteVehicleAsync
    public async Task DeleteVehicleAsync(int vehicleId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "DELETE FROM vehicles WHERE id = @Id";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@Id", vehicleId);

        await command.ExecuteNonQueryAsync();
    }
    #endregion

    public async Task<bool> IsVehicleNoExistsAsync(string vehicleNo, int? excludeVehicleId = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "SELECT COUNT(1) FROM vehicles WHERE vehicle_no = @VehicleNo";
        if (excludeVehicleId.HasValue)
        {
            query += " AND id <> @ExcludeId";
        }

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@VehicleNo", vehicleNo);
        if (excludeVehicleId.HasValue)
        {
            command.Parameters.AddWithValue("@ExcludeId", excludeVehicleId.Value);
        }

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }

    private void AddEmployeeParameters(SqlCommand command, EmployeeProfile employee)
    {
        command.Parameters.AddWithValue("@IC_No", employee.IC_No);
        command.Parameters.AddWithValue("@PersonName", employee.PersonName);
        command.Parameters.AddWithValue("@DescriptionOfPerson", (object?)employee.DescriptionOfPerson ?? DBNull.Value);
        command.Parameters.AddWithValue("@BirthDate", employee.BirthDate ?? DateTime.Today);
        command.Parameters.AddWithValue("@Nationality_id", (object?)employee.nationality_id ?? DBNull.Value);
        command.Parameters.AddWithValue("@Gender", employee.Gender);
        string hobbyString = employee.Hobby != null && employee.Hobby.Any() ? string.Join(".", employee.Hobby) : "";
        command.Parameters.AddWithValue("@Hobby", hobbyString);
        command.Parameters.AddWithValue("@SupportingDocumentPath", (object?)employee.SupportingDocumentPath ?? DBNull.Value);
        command.Parameters.AddWithValue("@PersonPortraitPath", (object?)employee.PersonPortraitPath ?? DBNull.Value);
        command.Parameters.AddWithValue("@MaritalStatus", (object?)employee.MaritalStatus ?? DBNull.Value);
    }
}

