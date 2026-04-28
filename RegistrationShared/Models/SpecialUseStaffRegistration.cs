using System.ComponentModel.DataAnnotations;
using RegistrationShared.Enums;
using RegistrationShared.Interfaces;

namespace RegistrationShared.Models;


/// <summary>
/// Represents a staff member registration affiliated with a company that has a special use licence.
/// </summary>
public class SpecialUseStaffRegistration : IRegistration
{
    public Guid Id { get; set; }

    public string? ContactName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string? ContactEmail { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public bool AgreesToTerms { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public LicenceStatus LicenceStatus { get; set; } = LicenceStatus.SpecialAwaitingReview;

    public DateTime ApplicationDate { get; set; } = DateTime.UtcNow;

    public Guid SpecialUseRegistrationId { get; set; }
    public string SpecialUseRegistration { get; set; } = null!;

}
