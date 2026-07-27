using Microsoft.AspNetCore.Components.Server.Circuits;

namespace RegistrationWebApp.Components.Utilities;

public class UserCircuitHandler : CircuitHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserContext _userContext;
    private readonly ILogger<UserCircuitHandler> _logger;

    public UserCircuitHandler(
        IHttpContextAccessor httpContextAccessor,
        UserContext userContext,
        ILogger<UserCircuitHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _userContext = userContext;
        _logger = logger;
    }

    public override Task OnConnectionUpAsync(
        Circuit circuit,
        CancellationToken cancellationToken)
    {
        var ipAddress = _httpContextAccessor.HttpContext?
            .Connection.RemoteIpAddress?
            .MapToIPv4()
            .ToString();

        _userContext.IPAddress = ipAddress;

        _logger.LogInformation(
            "Captured IP in circuit: {IP}",
            ipAddress);

        return Task.CompletedTask;
    }
}