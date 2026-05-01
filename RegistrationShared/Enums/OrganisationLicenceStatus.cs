
/// <summary>
/// Defines the possible licence statuses for an organisation in the registration system.
/// </summary>
public enum OrganisationLicenceStatus
{
    /// <summary>
    /// The organisation has applied for a licence and is awaiting review.
    /// </summary>
    Pending,
    /// <summary>
    /// The organisation's licence is active.
    /// </summary>
    Active,
    /// <summary>
    /// The organisation's licence is inactive. 
    /// This could be due to cancellation, expiry, or other reasons 
    /// that have resulted in the licence being no longer valid for use.
    /// </summary>
    Inactive

}