using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;

namespace Testing.Components.Model;

public class EmployeeProfile
{
    [Required]
    [StringLength(14, MinimumLength = 12, ErrorMessage = "IC No must be between 12-14 characters.")]
    [RegularExpression(@"^\d{6}-\d{2}-\d{4}$", ErrorMessage = "Format: YYMMDD-XX-XXXX")]
    public string IC_No { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-'\.]+$", ErrorMessage = "Name can only contain letters, spaces, hyphens, apostrophes, or dots.")]
    public string PersonName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
    public string? DescriptionOfPerson { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [CustomValidation(typeof(EmployeeProfile), nameof(ValidateBirthDate))]
    public DateTime? BirthDate { get; set; }

    public int? nationality_id { get; set; }

    public string Gender { get; set; } = "";

    public List<string> Hobby { get; set; } = new();

    //auto calculated age based on birthdate
    public int? CurrentAge => BirthDate.HasValue ? DateTime.Today.Year - BirthDate.Value.Year : null;

    [StringLength(255)]
    public string? SupportingDocumentPath { get; set; }

    [StringLength(255)]
    public string? PersonPortraitPath { get; set; }

    public List<Vehicle> Vehicles { get; set; } = new();

    public string? MaritalStatus { get; set; } = "Single";

    #region logic code function

    //birthdate range validation
    public static ValidationResult? ValidateBirthDate(DateTime? date, ValidationContext context)
    {
        if (date.HasValue && date.Value > DateTime.Today)
            return new ValidationResult("Birth date cannot be in the future.");

        if (date.HasValue && date.Value.Year < DateTime.Today.Year - 120)
            return new ValidationResult("Birth date is not valid (maximum age is 120 years).");

        return ValidationResult.Success;
    }

    //age logic
    public string? CurrentAgeDisplay
    {
        get
        {
            if (!BirthDate.HasValue) return null;

            var today = DateTime.Today;
            var birth = BirthDate.Value.Date;

            var years = today.Year - birth.Year;
            if (birth > today.AddYears(-years)) years--;

            if (years >= 1)
                return $"{years} {(years == 1 ? "year" : "years")}";

            var months = (today.Year - birth.Year) * 12 + today.Month - birth.Month;
            if (today.Day < birth.Day) months--;

            if (months >= 1)
                return $"{months} {(months == 1 ? "month" : "months")}";

            var days = (today - birth).Days;
            return $"{days} {(days == 1 ? "day" : "days")}";
        }
    }

    #endregion



}
