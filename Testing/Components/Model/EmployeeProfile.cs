using System.ComponentModel.DataAnnotations;

namespace Testing.Components.Model;

public class EmployeeProfile
{
    [Required]
    [StringLength(14, MinimumLength = 12, ErrorMessage = "IC No must be between 12-14 characters.")]
    [RegularExpression(@"^\d{6}-\d{2}-\d{4}$", ErrorMessage = "Format: YYMMDD-XX-XXXX")]
    public string IC_No { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    public string PersonName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? DescriptionOfPerson { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    [Required(ErrorMessage = "Nationality is required.")]
    public int? NationalityId { get; set; }
    
    // For display purposes
    public string? NationalityName { get; set; }

    [Required]
    public string Gender { get; set; } = string.Empty;

    public string? MaritalStatus { get; set; }

    public string? Hobby { get; set; }

    // Read-only property for Age, calculated based on BirthDate
    public int? CurrentAge => BirthDate.HasValue 
        ? DateTime.Today.Year - BirthDate.Value.Year - (BirthDate.Value.Date > DateTime.Today.AddYears(-DateTime.Today.Year + BirthDate.Value.Year) ? 1 : 0) 
        : null;

    // File Upload Paths
    public string? PersonPortraitPath { get; set; }
    public string? SupportingDocumentPath { get; set; }

    // Drilldown extension
    public int VehicleCount { get; set; }
}
