using RegistrationShared.Interfaces;
using RegistrationShared.Models;

namespace RegistrationWebApp.Components.Utilities.Models;

/// <summary>
/// A model for the response from the registration API. 
/// Contains a message to display to the user and the registration details if the registration was successful.
/// </summary>
public class OrganisationResponseModel
{
    public string Message { get; set; } = string.Empty;

    public Organisation? Organisation { get; set; }  

    public bool IsSuccess => Organisation != null;
}