using RegistrationShared.Models;

namespace RegistrationWebAPI.Models;

public class MemberOrganisationMapping
{
	public static MemberOrganisation ToNewEntity(MemberOrganisationUpsertRequest request)
	{
		return new MemberOrganisation
		{
			OrganisationName = request.OrganisationName.Trim(),
			OrganisationDomain = request.OrganisationDomain.Trim(),
			MembershipEstablishmentDate = request.MembershipEstablishmentDate ?? DateTime.UtcNow,
		};
	}

	public static void ApplyUpdate(MemberOrganisation entity, MemberOrganisationUpsertRequest request)
	{
		entity.OrganisationName = request.OrganisationName.Trim();
		entity.OrganisationDomain = request.OrganisationDomain.Trim();
		entity.MembershipEstablishmentDate = request.MembershipEstablishmentDate ?? entity.MembershipEstablishmentDate;
	}

	public static MemberOrganisationResponse ToResponse(MemberOrganisation entity)
	{
		return new MemberOrganisationResponse
		{
			Id = entity.Id,
			OrganisationName = entity.OrganisationName,
			OrganisationDomain = entity.OrganisationDomain,
			MembershipEstablishmentDate = entity.MembershipEstablishmentDate,
		};
	}
}
