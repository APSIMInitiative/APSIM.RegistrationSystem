using System.ComponentModel.DataAnnotations;
using RegistrationShared.Enums;

namespace RegistrationWebAPI.Data;

/// <summary>
/// Represents a user registration in the system. 
/// This class contains properties that store information about the user's contact details,
/// licence status, and organisation affiliation.
public class UserEntity
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
    /// The unique identifier of the organisation the user belongs to.
    /// This is an optional field and can be left empty.
    /// </summary>
    public Guid OrganisationId { get; set; }

    /// <summary>
    /// Navigation property to the organisation the user belongs to.
    /// </summary>
    public OrganisationEntity? Organisation { get; set; }

    /// <summary>
    /// One-time token used to verify the user's email address.
    /// </summary>
    public string? EmailVerificationToken { get; set; }

    /// <summary>
    /// The UTC date/time at which the verification token expires.
    /// </summary>
    public DateTime? EmailVerificationTokenExpiryUtc { get; set; }
}
