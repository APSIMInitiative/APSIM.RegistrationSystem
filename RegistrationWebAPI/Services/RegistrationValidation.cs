using System.ComponentModel.DataAnnotations;
using RegistrationWebAPI.Models;

namespace RegistrationWebAPI.Services;

public static class RegistrationValidation
{
    public static Dictionary<string, string[]> Validate(RegistrationUpsertRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        AddRequired(errors, nameof(request.ContactName), request.ContactName, "ContactName is required.");
        AddRequired(errors, nameof(request.ContactEmail), request.ContactEmail, "ContactEmail is required.");

        if (!string.IsNullOrWhiteSpace(request.ContactEmail))
        {
            var email = new EmailAddressAttribute();
            if (!email.IsValid(request.ContactEmail))
            {
                errors[nameof(request.ContactEmail)] = ["ContactEmail must be a valid email address."];
            }
        }

        if (request.RegistrationType == RegistrationType.SpecialUse)
        {
            AddRequired(errors, nameof(request.OrganisationName), request.OrganisationName, "OrganisationName is required for SpecialUse registrations.");
            AddRequired(errors, nameof(request.OrganisationAddress), request.OrganisationAddress, "OrganisationAddress is required for SpecialUse registrations.");

            if (request.LicencePathway is null)
            {
                errors[nameof(request.LicencePathway)] = ["LicencePathway is required for SpecialUse registrations."];
            }

            if (request.AnnualTurnover is null)
            {
                errors[nameof(request.AnnualTurnover)] = ["AnnualTurnover is required for SpecialUse registrations."];
            }
        }

        return errors;
    }

    private static void AddRequired(Dictionary<string, string[]> errors, string key, string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors[key] = [message];
        }
    }
}
