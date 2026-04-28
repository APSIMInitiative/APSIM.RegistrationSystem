namespace RegistrationWebAPI.Models;

/// <summary>
/// Represents the response model for a member organisation. 
/// This model is used to return member organisation data in API responses, 
/// providing a structured format for the client to consume. 
/// </summary>
public class MemberOrganisationResponse
{
    public Guid Id { get; set; }

    public string OrganisationName { get; set; } = string.Empty;

    public string OrganisationDomain { get; set; } = string.Empty;

    public DateTime MembershipEstablishmentDate { get; set; }
}