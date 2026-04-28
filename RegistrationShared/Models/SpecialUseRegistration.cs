using System.ComponentModel.DataAnnotations;
using RegistrationShared.Enums;
using RegistrationShared.Interfaces;

namespace RegistrationShared.Models;

/// <summary>
/// Represents a company registration for a special use licence.
/// Organisations can add further affiliated registrations to the same 
/// company to handle staff members who require access to the software 
/// under the same licence.
/// </summary>
public class SpecialUseRegistration : IRegistration
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Please enter the organisation's name.")]
    public string? OrganisationName { get; set; }

    [Required(ErrorMessage = "Please enter the contact person's name.")]
    public string? ContactName { get; set; }

    [Required]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? ContactEmail { get; set; }

    [Required(ErrorMessage = "Please enter the organisation's address.")]
    public string? OrganisationAddress { get; set; }

    public string? OrganisationWebsite { get; set; }

    public string? ContactPhone { get; set; }

    [Required(ErrorMessage = "Please select a licence pathway.")]
    public LicencePathway LicencePathWay { get; set; }

    [Required(ErrorMessage = "Please select an annual turnover range.")]
    public AnnualTurnover AnnualTurnover { get; set; }

    [DataType(DataType.Date)]
    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

    public LicenceStatus LicenceStatus { get; set; } = LicenceStatus.AwaitingEmailVerification;

    /// <summary>
    /// Indicates whether the applicant agrees to the terms and conditions of 
    /// the special use registration. This is a required field and must 
    /// be true for the application to be valid.
    /// </summary>
    [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept licence terms to proceed.")]
    public bool AgreesToTerms { get; set; }

    public List<SpecialUseStaffRegistration> SpecialUseStaffRegistrations { get; set; } = new List<SpecialUseStaffRegistration>();
}
