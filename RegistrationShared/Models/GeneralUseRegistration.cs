using System.ComponentModel.DataAnnotations;
using RegistrationShared.Enums;
using RegistrationShared.Interfaces;

namespace RegistrationShared.Models;

public class GeneralUseRegistration : IRegistration
{
    [Required]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? ContactEmail { get; set; }

    [Required(ErrorMessage = "Please enter the contact person's name.")]
    public string? ContactName { get; set; }

    public LicenceStatus LicenceStatus { get; set; } = LicenceStatus.GeneralUse;

    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether the applicant agrees to the terms and conditions of the special use registration. 
    /// This is a required field and must be true for the application to be valid.
    /// </summary>
    [Required(ErrorMessage = "You must agree to the terms and conditions.")]
    public bool AgreesToTerms { get; set; }
}
