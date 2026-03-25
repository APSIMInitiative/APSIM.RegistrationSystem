using System.ComponentModel.DataAnnotations;
using APSIM.Registration.Contracts.Enums;
using APSIM.Registration.Contracts.Interfaces;

namespace APSIM.Registration.Contracts.Models;

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
}
