
namespace RegistrationShared.Enums;

public enum UserLicenceStatus
{
    /// <summary>
    /// Indicates that the user does not have a licence. 
    /// This is the default status for new users and may indicate that the user's registration is still being processed or that they have not yet been approved for a licence.
    /// </summary>
    None,
    /// <summary>
    /// Indicates that the user's licence is pending email verification.
    /// This status is assigned to users who have registered but have not yet verified their email address.
    /// </summary>
    Pending,
    /// <summary>
    /// Indicates that the user has a general use licence.
    /// </summary>
    General,
    /// <summary>
    /// Indicates that the user has a special use licence.
    /// </summary>
    Special
}
