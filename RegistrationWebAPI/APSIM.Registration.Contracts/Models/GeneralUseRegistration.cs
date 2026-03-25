using System.ComponentModel.DataAnnotations;
using APSIM.Registration.Contracts.Enums;
using APSIM.Registration.Contracts.Interfaces;

namespace APSIM.Registration.Contracts.Models;

public class GeneralUseRegistration : IRegistration
{
    [Required]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? ContactEmail { get; set; }

    [Required(ErrorMessage = "Please enter the contact person's name.")]
    public string? ContactName { get; set; }

    public LicenceStatus LicenceStatus { get; set; } = LicenceStatus.GeneralUse;

    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;
}
