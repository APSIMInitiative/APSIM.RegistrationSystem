using System.Text.Json;

namespace RegistrationWebApp.Components.Utilities;

public partial class WebApiUtility
{
    private sealed class RegistrationApiModel
    {
        public JsonElement RegistrationType { get; set; }

        public string? ContactName { get; set; }

        public string? ContactEmail { get; set; }

        public DateTime ApplicationDate { get; set; }

        public RegistrationShared.Enums.LicenceStatus LicenceStatus { get; set; }

        public string? OrganisationName { get; set; }

        public string? OrganisationAddress { get; set; }

        public string? OrganisationWebsite { get; set; }

        public string? ContactPhone { get; set; }

        public RegistrationShared.Enums.LicencePathway? LicencePathway { get; set; }

        public RegistrationShared.Enums.AnnualTurnover? AnnualTurnover { get; set; }

        public bool? AgreesToTerms { get; set; }

        public bool IsSpecialUse()
        {
            if (RegistrationType.ValueKind == JsonValueKind.Number && RegistrationType.TryGetInt32(out int registrationTypeValue))
            {
                return registrationTypeValue == 1;
            }

            if (RegistrationType.ValueKind == JsonValueKind.String)
            {
                string? registrationTypeText = RegistrationType.GetString();
                if (!string.IsNullOrWhiteSpace(registrationTypeText) && string.Equals(registrationTypeText, "SpecialUse", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            // Fallback for payloads that omit registrationType but include special-use fields.
            return !string.IsNullOrWhiteSpace(OrganisationName);
        }
    }
}
