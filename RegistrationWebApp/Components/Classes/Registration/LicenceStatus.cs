
namespace RegistrationWebApp.Components.Classes.Registration;
public enum LicenceStatus
{
    None,
    /// <summary>
    /// An active general use licence that has been approved and is currently valid. 
    /// This status indicates that the licence holder has met all 
    /// the necessary requirements and has been granted access 
    /// to the software for general use. 
    /// </summary>
    GeneralUse,
    /// <summary>
    /// A special use licence that is currently under review. 
    /// This status indicates that the application for a 
    /// special use licence has been submitted and is being evaluated 
    /// by the administrators. During this stage, the application is 
    /// being assessed to determine if it meets the criteria for 
    /// approval, and the applicant may be contacted for 
    /// additional information or clarification.
    /// </summary>
    SpecialAwaitingReview,
    /// <summary>The licence is in a provisional state, where the 
    /// applicant has been approved for a licence but has not yet paid the invoice. 
    /// This status allows the applicant to access the software while they arrange payment, 
    /// but it also indicates to administrators that the licence is not yet 
    /// fully active and may require follow-up if payment is not 
    /// received within a reasonable timeframe.</summary>
    SpecialProvisional,
    /// <summary>
    /// The licence has been invoiced but not yet paid. This status 
    /// indicates that the applicant has been approved for a 
    /// special use licence and has been invoiced for the licence 
    /// fee, but payment has not yet been received.
    /// </summary>
    SpecialInvoiced,
    /// <summary>
    /// The licence is active for special use. This status 
    /// indicates that the applicant has paid the invoice and 
    /// the special use licence is now active and valid.
    /// </summary>
    SpecialActive,

    /// <summary>
    /// The licence application for special use has been declined. 
    /// This status indicates that the application did not meet the 
    /// necessary criteria for approval, and the applicant has 
    /// been notified of the decision. 
    /// </summary>
    SpecialDeclined,
    /// <summary>
    /// The licence has been cancelled. This status indicates that the 
    /// licence holder has voluntarily cancelled their licence, or that 
    /// the administrators have cancelled the licence due to 
    /// non-compliance or other reasons.
    /// </summary>
    Cancelled,
    /// <summary>
    /// The licence has expired. This status indicates that 
    /// the licence has reached its expiration date and is no longer valid for use.
    /// </summary>
    Expired
}
