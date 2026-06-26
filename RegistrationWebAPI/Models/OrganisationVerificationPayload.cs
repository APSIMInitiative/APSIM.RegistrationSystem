using RegistrationShared.Enums;

namespace RegistrationWebAPI.Models;

public class OrganisationVerificationPayload
{
    public Guid OrganisationId { get; set; }

    public string OrganisationName { get; set; } = string.Empty;

    public string ContactName { get; set; } = string.Empty;

    public string ContactEmail { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;

    public string ContactAddress { get; set; } = string.Empty;

    public List<string> OrganisationEmails { get; set; } = new();

    public LicencePathway LicencePathway { get; set; }

    public AnnualTurnover AnnualTurnover { get; set; }

    public DateTime DateCreatedUtc { get; set; }
}
