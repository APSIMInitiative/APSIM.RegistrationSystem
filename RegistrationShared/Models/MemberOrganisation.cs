using System.ComponentModel.DataAnnotations;

namespace RegistrationShared.Models;

public class MemberOrganisation
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please enter the organisation's name.")]
    public string OrganisationName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter the organisation's domain.")]
    public string OrganisationDomain { get; set; } = string.Empty;

    public DateTime MembershipEstablishmentDate { get; set; } = DateTime.UtcNow;

    public List<MemberOrganisationRegistration> MemberRegistrations { get; set; } = new List<MemberOrganisationRegistration>();


}