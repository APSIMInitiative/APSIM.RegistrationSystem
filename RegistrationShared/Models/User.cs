using System.ComponentModel.DataAnnotations;
using RegistrationShared.Enums;

namespace RegistrationShared.Models;

/// <summary>
/// Represents a user in the registration system. This class contains properties that store information about the user's email,
/// licence status, and registration date.
/// </summary>
public class User
{

    /// <summary>
    /// The unique identifier of the user. 
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Valid email address of the user. 
    /// This is a required field and should be unique across all users in the system.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The date and time when the user registered. 
    /// This is automatically set to the current date and time when the user object is created.
    /// </summary>
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The licence status of the user. 
    /// This indicates whether the user has an active licence, is pending review, or has no licence.
    /// </summary>
    public UserLicenceStatus LicenceStatus { get; set; } = UserLicenceStatus.None;

    /// <summary>
    /// Returns a string representation of the User object, including the email, licence status, and date created.
    /// </summary>
    /// <returns>A string representation of the User object.</returns>
    public override string ToString()
    {
        return $"User: {Email}, LicenceStatus: {LicenceStatus}, DateCreated: {DateCreated}";
    }


}