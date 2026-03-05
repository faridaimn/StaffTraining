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

    public async Task<List<RefNationality>> GetNationalitiesAsync()
    {
        var nationalities = new List<RefNationality>();
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT nationality_id, nationality_name FROM ref_nationality WHERE is_active = 1 ORDER BY nationality_name ASC";
            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                nationalities.Add(new RefNationality
                {
                    NationalityId = Convert.ToInt32(reader["nationality_id"]),
                    NationalityName = reader["nationality_name"]?.ToString() ?? ""
                });
            }
        }
        catch (Exception)
        {
            throw;
        }
        return nationalities;
    }

    public async Task<List<EmployeeProfile>> GetEmployeesAsync(
        string? searchName = null, 
        string? searchIC = null, 
        int? searchNationalityId = null,
        string? searchGender = null,
        string? searchMaritalStatus = null,
        string? searchBirthYear = null)
    {
        var employees = new List<EmployeeProfile>();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT e.*, r.nationality_name,
                       (SELECT COUNT(*) FROM Vehicles v WHERE v.Owner_IC = e.IC_No) as VehicleCount 
                FROM EmployeeProfiles e
                LEFT JOIN ref_nationality r ON e.nationality_id = r.nationality_id 
                WHERE 1=1";
            
            if (!string.IsNullOrWhiteSpace(searchName)) query += " AND e.PersonName LIKE @Name";
            if (!string.IsNullOrWhiteSpace(searchIC)) query += " AND e.IC_No LIKE @IC";
            if (searchNationalityId.HasValue) query += " AND e.nationality_id = @NationalityId";
            if (!string.IsNullOrWhiteSpace(searchGender)) query += " AND e.Gender = @Gender";
            if (!string.IsNullOrWhiteSpace(searchMaritalStatus)) query += " AND e.MaritalStatus = @MaritalStatus";
            if (!string.IsNullOrWhiteSpace(searchBirthYear)) query += " AND YEAR(e.BirthDate) = @BirthYear";

            using var command = new SqlCommand(query, connection);

            if (!string.IsNullOrWhiteSpace(searchName)) command.Parameters.AddWithValue("@Name", "%" + searchName + "%");
            if (!string.IsNullOrWhiteSpace(searchIC)) command.Parameters.AddWithValue("@IC", "%" + searchIC + "%");
            if (searchNationalityId.HasValue) command.Parameters.AddWithValue("@NationalityId", searchNationalityId.Value);
            if (!string.IsNullOrWhiteSpace(searchGender)) command.Parameters.AddWithValue("@Gender", searchGender);
            if (!string.IsNullOrWhiteSpace(searchMaritalStatus)) command.Parameters.AddWithValue("@MaritalStatus", searchMaritalStatus);
            if (!string.IsNullOrWhiteSpace(searchBirthYear)) command.Parameters.AddWithValue("@BirthYear", searchBirthYear);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var emp = new EmployeeProfile
                {
                    IC_No = reader["IC_No"]?.ToString() ?? "",
                    PersonName = reader["PersonName"]?.ToString() ?? "",
                    DescriptionOfPerson = reader["DescriptionOfPerson"]?.ToString(),
                    BirthDate = reader["BirthDate"] != DBNull.Value ? Convert.ToDateTime(reader["BirthDate"]) : null,
                    NationalityId = reader["nationality_id"] != DBNull.Value ? Convert.ToInt32(reader["nationality_id"]) : null,
                    NationalityName = reader["nationality_name"]?.ToString(),
                    Gender = reader["Gender"]?.ToString() ?? "",
                    MaritalStatus = reader["MaritalStatus"]?.ToString(),
                    Hobby = reader["Hobby"]?.ToString(),
                    PersonPortraitPath = reader["PersonPortraitPath"]?.ToString(),
                    SupportingDocumentPath = reader["SupportingDocumentPath"]?.ToString(),
                    VehicleCount = reader["VehicleCount"] != DBNull.Value ? Convert.ToInt32(reader["VehicleCount"]) : 0
                };
                employees.Add(emp);
            }
        }
        catch (Exception)
        {
            throw;
        }

        return employees;
    }

    public async Task CreateEmployeeAsync(EmployeeProfile employee)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            INSERT INTO EmployeeProfiles 
            (IC_No, PersonName, DescriptionOfPerson, BirthDate, nationality_id, Gender, MaritalStatus, Hobby, PersonPortraitPath, SupportingDocumentPath) 
            VALUES 
            (@IC_No, @PersonName, @DescriptionOfPerson, @BirthDate, @NationalityId, @Gender, @MaritalStatus, @Hobby, @PersonPortraitPath, @SupportingDocumentPath)";

        using var command = new SqlCommand(query, connection);
        AddEmployeeParameters(command, employee);
        
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateEmployeeAsync(EmployeeProfile employee)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            UPDATE EmployeeProfiles SET
            PersonName = @PersonName, 
            DescriptionOfPerson = @DescriptionOfPerson, 
            BirthDate = @BirthDate, 
            nationality_id = @NationalityId, 
            Gender = @Gender, 
            MaritalStatus = @MaritalStatus,
            Hobby = @Hobby,
            PersonPortraitPath = @PersonPortraitPath,
            SupportingDocumentPath = @SupportingDocumentPath
            WHERE IC_No = @IC_No";

        using var command = new SqlCommand(query, connection);
        AddEmployeeParameters(command, employee);
        
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteEmployeeAsync(string icNo)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "DELETE FROM EmployeeProfiles WHERE IC_No = @IC_No";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@IC_No", icNo);
        
        await command.ExecuteNonQueryAsync();
    }

    private void AddEmployeeParameters(SqlCommand command, EmployeeProfile employee)
    {
        command.Parameters.AddWithValue("@IC_No", employee.IC_No);
        command.Parameters.AddWithValue("@PersonName", employee.PersonName);
        command.Parameters.AddWithValue("@DescriptionOfPerson", (object?)employee.DescriptionOfPerson ?? DBNull.Value);
        command.Parameters.AddWithValue("@BirthDate", (object?)employee.BirthDate ?? DBNull.Value);
        command.Parameters.AddWithValue("@NationalityId", (object?)employee.NationalityId ?? DBNull.Value);
        command.Parameters.AddWithValue("@Gender", employee.Gender);
        command.Parameters.AddWithValue("@MaritalStatus", (object?)employee.MaritalStatus ?? DBNull.Value);
        command.Parameters.AddWithValue("@Hobby", (object?)employee.Hobby ?? DBNull.Value);
        command.Parameters.AddWithValue("@PersonPortraitPath", (object?)employee.PersonPortraitPath ?? DBNull.Value);
        command.Parameters.AddWithValue("@SupportingDocumentPath", (object?)employee.SupportingDocumentPath ?? DBNull.Value);
    }

    // --- Vehicle Management Methods ---

    public async Task<List<Vehicle>> GetVehiclesAsync(
        string? searchOwnerIC = null, 
        string? searchVehicleNo = null,
        string? searchColor = null,
        string? searchBrand = null,
        string? searchModel = null)
    {
        var vehicles = new List<Vehicle>();

        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT v.*, e.PersonName as OwnerName 
                FROM Vehicles v
                INNER JOIN EmployeeProfiles e ON v.Owner_IC = e.IC_No 
                WHERE 1=1";
            
            if (!string.IsNullOrWhiteSpace(searchOwnerIC)) query += " AND v.Owner_IC LIKE @Owner_IC";
            if (!string.IsNullOrWhiteSpace(searchVehicleNo)) query += " AND v.VehicleNo LIKE @VehicleNo";
            if (!string.IsNullOrWhiteSpace(searchColor)) query += " AND v.Color = @Color";
            if (!string.IsNullOrWhiteSpace(searchBrand)) query += " AND v.Brand = @Brand";
            if (!string.IsNullOrWhiteSpace(searchModel)) query += " AND v.Model = @Model";

            query += " ORDER BY e.PersonName ASC";

            using var command = new SqlCommand(query, connection);

            if (!string.IsNullOrWhiteSpace(searchOwnerIC)) command.Parameters.AddWithValue("@Owner_IC", "%" + searchOwnerIC + "%");
            if (!string.IsNullOrWhiteSpace(searchVehicleNo)) command.Parameters.AddWithValue("@VehicleNo", "%" + searchVehicleNo + "%");
            if (!string.IsNullOrWhiteSpace(searchColor)) command.Parameters.AddWithValue("@Color", searchColor);
            if (!string.IsNullOrWhiteSpace(searchBrand)) command.Parameters.AddWithValue("@Brand", searchBrand);
            if (!string.IsNullOrWhiteSpace(searchModel)) command.Parameters.AddWithValue("@Model", searchModel);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                vehicles.Add(new Vehicle
                {
                    VehicleID = Convert.ToInt32(reader["VehicleID"]),
                    Owner_IC = reader["Owner_IC"].ToString() ?? "",
                    OwnerName = reader["OwnerName"].ToString() ?? "",
                    VehicleNo = reader["VehicleNo"].ToString() ?? "",
                    Color = reader["Color"]?.ToString(),
                    Brand = reader["Brand"]?.ToString(),
                    Model = reader["Model"]?.ToString()
                });
            }
        }
        catch (Exception)
        {
            throw;
        }

        return vehicles;
    }

    public async Task CreateVehicleAsync(Vehicle vehicle)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            INSERT INTO Vehicles (Owner_IC, VehicleNo, Color, Brand, Model) 
            VALUES (@Owner_IC, @VehicleNo, @Color, @Brand, @Model)";

        using var command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@Owner_IC", vehicle.Owner_IC);
        command.Parameters.AddWithValue("@VehicleNo", vehicle.VehicleNo);
        command.Parameters.AddWithValue("@Color", (object?)vehicle.Color ?? DBNull.Value);
        command.Parameters.AddWithValue("@Brand", (object?)vehicle.Brand ?? DBNull.Value);
        command.Parameters.AddWithValue("@Model", (object?)vehicle.Model ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateVehicleAsync(Vehicle vehicle)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
            UPDATE Vehicles SET
            Owner_IC = @Owner_IC, 
            VehicleNo = @VehicleNo, 
            Color = @Color, 
            Brand = @Brand, 
            Model = @Model
            WHERE VehicleID = @VehicleID";

        using var command = new SqlCommand(query, connection);
        
        command.Parameters.AddWithValue("@VehicleID", vehicle.VehicleID);
        command.Parameters.AddWithValue("@Owner_IC", vehicle.Owner_IC);
        command.Parameters.AddWithValue("@VehicleNo", vehicle.VehicleNo);
        command.Parameters.AddWithValue("@Color", (object?)vehicle.Color ?? DBNull.Value);
        command.Parameters.AddWithValue("@Brand", (object?)vehicle.Brand ?? DBNull.Value);
        command.Parameters.AddWithValue("@Model", (object?)vehicle.Model ?? DBNull.Value);
        
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteVehicleAsync(int vehicleId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = "DELETE FROM Vehicles WHERE VehicleID = @VehicleID";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@VehicleID", vehicleId);
        
        await command.ExecuteNonQueryAsync();
    }
}
