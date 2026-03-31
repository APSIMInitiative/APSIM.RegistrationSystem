using RegistrationShared.Enums;
using RegistrationShared.Models;
using RegistrationWebAPI.Models;

namespace RegistrationWebAPI.Data;

public class RegistrationEntity
{
    public Guid Id { get; set; }

    public RegistrationType RegistrationType { get; set; }

    public string ContactName { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public DateTime ApplicationDate { get; set; }

    public LicenceStatus LicenceStatus { get; set; }

    public string? OrganisationName { get; set; }

    public string? OrganisationAddress { get; set; }

    public string? OrganisationWebsite { get; set; }

    public string? ContactPhone { get; set; }

    public LicencePathway? LicencePathway { get; set; }

    public AnnualTurnover? AnnualTurnover { get; set; }

    public bool AgreesToTerms { get; set; }
}
