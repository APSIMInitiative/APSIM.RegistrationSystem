namespace RegistrationWebApp.Components.Utilities.Models;

public class Login
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public Login() { }

    public Login(string username, string password)
    {
        Username = username;
        Password = password;
    }
}