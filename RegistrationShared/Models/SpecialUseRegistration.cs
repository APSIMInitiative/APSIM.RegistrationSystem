using System.ComponentModel.DataAnnotations;
using RegistrationShared.Enums;
using RegistrationShared.Interfaces;

namespace RegistrationShared.Models;

public class SpecialUseRegistration : IRegistration
{
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

    public LicenceStatus LicenceStatus { get; set; } = LicenceStatus.SpecialAwaitingReview;

    /// <summary>
    /// Indicates whether the applicant agrees to the terms and conditions of 
    /// the special use registration. This is a required field and must 
    /// be true for the application to be valid.
    /// </summary>
    [Required(ErrorMessage = "You must agree to the terms and conditions.")]
    public bool AgreesToTerms { get; set; }
}
