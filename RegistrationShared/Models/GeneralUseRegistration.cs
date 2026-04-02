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

    public LicenceStatus LicenceStatus { get; set; } = LicenceStatus.AwaitingEmailVerification;

    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether the applicant agrees to the terms and conditions of the licence. 
    /// This is a required field and must be true for the application to be valid.
    /// </summary>
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept licence terms to proceed.")]
    public bool AgreesToTerms { get; set; }
}
