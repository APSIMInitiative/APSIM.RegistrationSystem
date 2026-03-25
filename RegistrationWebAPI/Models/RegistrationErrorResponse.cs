
namespace APSIM.RegistrationAPIV2.Models;

public class RegistrationErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public IDictionary<string, string[]> Errors { get; set; } = new Dictionary<string, string[]>();
}
