using System.ComponentModel.DataAnnotations;
using RegistrationWebApp.Components.Classes.Interfaces;

namespace RegistrationWebApp.Components.Classes.Registration;

/// <summary>Class to hold the details of a special use registration application.</summary>
public class SpecialUseRegistration: IRegistration
{

    /// <summary>The name of the organisation applying for the special use registration. This is a required field.</summary>
    [Required(ErrorMessage = "Please enter the organisation's name.")]
    public string? OrganisationName { get; set; }

    /// <summary>The name of the contact person for this application. This is a required field.</summary>
    [Required(ErrorMessage = "Please enter the contact person's name.")]
    public string? ContactName { get; set; }

    /// <summary>The email address of the contact person for this application. This is a required field and must be in a valid email format.</summary>
    [Required]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? ContactEmail { get; set; }

    /// <summary>The address of the organisation applying for the special use registration. This is a required field.</summary>
    [Required(ErrorMessage = "Please enter the organisation's address.")]
    public string? OrganisationAddress { get; set; }

    /// <summary>The website of the organisation applying for the special use registration. This is an optional field.</summary>
    public string? OrganisationWebsite { get; set; }

    /// <summary>The phone number of the contact person for this application. This is an optional field.</summary>
    public string? ContactPhone { get; set; }

    /// <summary>The licence pathway for this application. This is a required field.</summary>
    [Required(ErrorMessage = "Please select a licence pathway.")] 
    public LicencePathway LicencePathWay { get; set; }

    /// <summary>The annual turnover of the organisation applying for the special use registration. This is a required field.</summary>
    [Required(ErrorMessage = "Please select an annual turnover range.")] 
    public AnnualTurnover AnnualTurnover { get; set; }

    /// <summary>The date when the application was submitted. This is automatically set to the current date and time.</summary>
    [DataType(DataType.Date)]
    public DateTime ApplicationDate { get; set; } = DateTime.Now;

    /// <summary>
    /// Indicates whether the applicant agrees to the terms and conditions of the special use registration. 
    /// This is a required field and must be true for the application to be valid.
    /// </summary>
    [Required(ErrorMessage = "You must agree to the terms and conditions.")]
    public bool AgreesToTerms { get; set; }

    public SpecialUseRegistration(){}

    public LicenceStatus LicenceStatus { get; set; }



    public override string ToString()
    {
        return $"Organisation Name: {OrganisationName}\nContact Name: {ContactName}\nContact Email: {ContactEmail}\nOrganisation Address: {OrganisationAddress}\nOrganisation Website: {OrganisationWebsite}\nContact Phone: {ContactPhone}\nLicence Pathway: {LicencePathWay}\nAnnual Turnover: {AnnualTurnover}\nLicence Status: {LicenceStatus}\nApplication Date: {ApplicationDate}";
    }

}

