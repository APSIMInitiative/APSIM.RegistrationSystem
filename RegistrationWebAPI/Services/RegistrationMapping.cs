using RegistrationShared.Enums;
using RegistrationWebAPI.Data;
using RegistrationWebAPI.Models;

namespace RegistrationWebAPI.Services;

public static class RegistrationMapping
{
    public static RegistrationEntity ToNewEntity(RegistrationUpsertRequest request)
    {
        var now = DateTime.UtcNow;
        var status = request.LicenceStatus ?? DefaultStatusFor(request.RegistrationType);

        return new RegistrationEntity
        {
            Id = Guid.NewGuid(),
            RegistrationType = request.RegistrationType,
            ContactName = request.ContactName!.Trim(),
            ContactEmail = request.ContactEmail!.Trim(),
            ApplicationDate = request.ApplicationDate ?? now,
            LicenceStatus = status,
            OrganisationName = request.OrganisationName,
            OrganisationAddress = request.OrganisationAddress,
            OrganisationWebsite = request.OrganisationWebsite,
            ContactPhone = request.ContactPhone,
            LicencePathway = request.LicencePathway,
            AnnualTurnover = request.AnnualTurnover,
        };
    }

    public static void ApplyUpdate(RegistrationEntity entity, RegistrationUpsertRequest request)
    {
        entity.RegistrationType = request.RegistrationType;
        entity.ContactName = request.ContactName!.Trim();
        entity.ContactEmail = request.ContactEmail!.Trim();
        entity.ApplicationDate = request.ApplicationDate ?? entity.ApplicationDate;
        entity.LicenceStatus = request.LicenceStatus ?? DefaultStatusFor(request.RegistrationType);

        if (request.RegistrationType == RegistrationType.SpecialUse)
        {
            entity.OrganisationName = request.OrganisationName;
            entity.OrganisationAddress = request.OrganisationAddress;
            entity.OrganisationWebsite = request.OrganisationWebsite;
            entity.ContactPhone = request.ContactPhone;
            entity.LicencePathway = request.LicencePathway;
            entity.AnnualTurnover = request.AnnualTurnover;
        }
        else
        {
            entity.OrganisationName = null;
            entity.OrganisationAddress = null;
            entity.OrganisationWebsite = null;
            entity.ContactPhone = null;
            entity.LicencePathway = null;
            entity.AnnualTurnover = null;
        }
    }

    public static RegistrationResponse ToResponse(RegistrationEntity entity)
    {
        return new RegistrationResponse
        {
            Id = entity.Id,
            RegistrationType = entity.RegistrationType,
            ContactName = entity.ContactName,
            ContactEmail = entity.ContactEmail,
            ApplicationDate = entity.ApplicationDate,
            LicenceStatus = entity.LicenceStatus,
            OrganisationName = entity.OrganisationName,
            OrganisationAddress = entity.OrganisationAddress,
            OrganisationWebsite = entity.OrganisationWebsite,
            ContactPhone = entity.ContactPhone,
            LicencePathway = entity.LicencePathway,
            AnnualTurnover = entity.AnnualTurnover,
        };
    }

    private static LicenceStatus DefaultStatusFor(RegistrationType registrationType)
    {
        return LicenceStatus.AwaitingEmailVerification;
    }
}
