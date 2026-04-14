using System.ComponentModel.DataAnnotations;

namespace Testing.Components.Model
{
    public class Vehicle
    {
        public int Id { get; set; }

        [Required]
        [StringLength(14, MinimumLength = 12, ErrorMessage = "IC No must be between 12-14 characters.")]
        [RegularExpression(@"^\d{6}-\d{2}-\d{4}$", ErrorMessage = "Format: YYMMDD-XX-XXXX")]
        public string IC_No { get; set; } = string.Empty;

        [Required]
        [StringLength(20, ErrorMessage = "Vehicle No cannot exceed 20 characters.")]
        [RegularExpression(@"^[A-Za-z0-9\s\-]+$", ErrorMessage = "Vehicle No can only contain letters, numbers, spaces, or hyphens.")]
        public string VehicleNo { get; set; } = string.Empty;

        [Required]
        [StringLength(30, ErrorMessage = "Color cannot exceed to 30 characters.")]
        public string Color { get; set; } = string.Empty;

        [Required]
        public string brand { get; set; } = string.Empty;

        [Required]
        [StringLength(50, ErrorMessage = "Model cannot exceed to 50 characters")]
        public string Model { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }

        public EmployeeProfile? EmployeeProfile { get; set; }


    }
}
