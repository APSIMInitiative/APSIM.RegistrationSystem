using RegistrationShared.Models;

namespace RegistrationWebApp.Components.Utilities.Models;

/// <summary>
/// A model for the response from the registration API. 
/// Contains a message to display to the user and the registration details if the registration was successful.
/// </summary>
public class UserResponseModel
{
    public string Message { get; set; } = string.Empty;

    public required User? User { get; set; }

    public bool IsSuccess => User != null;
}