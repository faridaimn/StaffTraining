using System.ComponentModel.DataAnnotations;

namespace Testing.Components.Model;

public class Vehicle
{
    public int VehicleID { get; set; }

    [Required(ErrorMessage = "Please select an owner.")]
    public string Owner_IC { get; set; } = string.Empty;

    public string? OwnerName { get; set; } // For display purposes

    [Required(ErrorMessage = "Vehicle No is required.")]
    [StringLength(20, ErrorMessage = "Vehicle No cannot exceed 20 characters.")]
    public string VehicleNo { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Color { get; set; }

    [StringLength(50)]
    public string? Brand { get; set; }

    [StringLength(50)]
    public string? Model { get; set; }
}
