using System.ComponentModel.DataAnnotations;
using RegistrationShared.Enums;

namespace RegistrationWebAPI.Data;

public class OrganisationEntity
{    
    /// <summary>
    /// The unique identifier of the user. 
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The name of the organisation. This is a required field and should be 
    /// unique across all organisations in the system.
    /// </summary>
    [Required]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// A list of email addresses or domains associated with the organisation. 
    /// </summary>
    public List<string> Emails { get; set; } = new List<string>();

    /// <summary>
    /// The licence status of the organisation. This indicates whether the 
    /// organisation has an active licence, is pending review, or is inactive.
    /// </summary>
    public OrganisationLicenceStatus LicenceStatus { get; set; } = OrganisationLicenceStatus.Pending;

    /// <summary>
    /// The licence pathway for the organisation.
    /// </summary>
    public LicencePathway LicencePathway {get; set;}

    /// <summary>
    /// The date and time when the organisation registered. 
    /// This is automatically set to the current date and time 
    /// when the organisation object is created.
    /// </summary>
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// A list of users associated with the organisation. 
    /// This can include staff members or other users who are linked to the 
    /// organisation's licence and registration.
    /// </summary>
    public List<UserEntity> Users { get; set; } = new List<UserEntity>();

    /// <summary>
    /// One-time token used to verify the organisation contact email.
    /// </summary>
    public string? EmailVerificationToken { get; set; }

    /// <summary>
    /// The UTC date/time at which the organisation verification token expires.
    /// </summary>
    public DateTime? EmailVerificationTokenExpiryUtc { get; set; }


}