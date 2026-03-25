using APSIM.Registration.Contracts.Enums;

namespace APSIM.RegistrationAPIV2.Models;

public class RegistrationUpsertRequest
{
    public RegistrationType RegistrationType { get; set; }

    public string? ContactName { get; set; }

    public string? ContactEmail { get; set; }

    public DateTime? ApplicationDate { get; set; }

    public LicenceStatus? LicenceStatus { get; set; }

    public string? OrganisationName { get; set; }

    public string? OrganisationAddress { get; set; }

    public string? OrganisationWebsite { get; set; }

    public string? ContactPhone { get; set; }

    public LicencePathway? LicencePathway { get; set; }

    public AnnualTurnover? AnnualTurnover { get; set; }
}
