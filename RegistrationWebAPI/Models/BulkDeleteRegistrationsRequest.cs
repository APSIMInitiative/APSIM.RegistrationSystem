namespace RegistrationWebAPI.Models;

public class BulkDeleteRegistrationsRequest
{
    public List<Guid> Ids { get; set; } = [];
}
