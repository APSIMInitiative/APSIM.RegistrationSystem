using System.Text.Json.Serialization;
using RegistrationShared.Enums;
using RegistrationShared.Models;

namespace RegistrationShared.Interfaces;

public interface IRegistration
{
    string? ContactName { get; set; }

    string? ContactEmail { get; set; }
    
    DateTime ApplicationDate { get; set; }

    bool AgreesToTerms { get; set; }

    LicenceStatus LicenceStatus { get; set; }
}
