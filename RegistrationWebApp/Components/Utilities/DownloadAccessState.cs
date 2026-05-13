namespace RegistrationWebApp.Components.Utilities;

public class DownloadAccessState
{
    public string? Token { get; private set; }

    public void SetToken(string token)
    {
        Token = token;
    }

    public string? GetToken()
    {
        return Token;
    }

    public void ClearToken()
    {
        Token = null;
    }
}
