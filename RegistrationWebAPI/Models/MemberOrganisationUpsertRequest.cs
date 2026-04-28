namespace RegistrationWebAPI.Models;

/// <summary>
/// Represents the data required to create or update a member organisation. 
/// This model is used for both creating new member organisations and updating 
/// existing ones, allowing for flexibility in handling member organisation 
/// data in the API.
/// </summary>
public class MemberOrganisationUpsertRequest
{
	public string OrganisationName { get; set; } = string.Empty;

	public string OrganisationDomain { get; set; } = string.Empty;

	public DateTime? MembershipEstablishmentDate { get; set; }
}