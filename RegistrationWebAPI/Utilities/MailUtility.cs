using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.DependencyInjection;

namespace RegistrationWebAPI.Utilities;

/// <summary>
/// Class for handling the sending of emails.
/// </summary>
public class MailUtility
{

    private string? _apiKey;
    private SendGridClient? _client;

    /// <summary>
    /// The email address and name that will appear in the "From" field of the email.
    /// </summary>
    private string _fromEmailName = "APSIM Registration System";


    /// <summary>
    /// The email address that will appear in the "From" field of the email. 
    /// </summary>
    private string _fromEmail = "no-reply@apsim.info";

    /// <summary>Default constructor.</summary>
    public MailUtility() { }

    /// <summary> Constructor for MailUtility. Initializes the SendGrid client with the provided API key. </summary>
    public MailUtility(string apiKey)
    {
        _apiKey = apiKey;
        _client = new SendGridClient(_apiKey);
    }

    private async Task<Response> SendEmailAsync(string toEmail, string subject, string plainTextContent, string htmlContent)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("SendGrid client is not initialized. Please provide an API key.");
        }

        var from = new EmailAddress(_fromEmail, _fromEmailName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        return await _client.SendEmailAsync(msg);
    }

    public async Task<Response> SendVerificationEmailAsync(string toEmail, string verificationLink)
    {
        string subject = "Verify your email for APSIM Registration System";
        string plainTextContent = $"Please verify your email by clicking the following link: {verificationLink}";
        string htmlContent = $"<p>Please verify your email by clicking the following link:</p><p><a href='{verificationLink}'>Verify Email</a></p>";
        return await SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
    }

    public async Task<Response> SendStatusUpdateEmailAsync(string toEmail, string status)
    {
        string subject = "Your APSIM Registration Status Update";
        string plainTextContent = $"Your registration status has been updated to: {status}";
        string htmlContent = $"<p>Your registration status has been updated to: <strong>{status}</strong></p>";
        return await SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
    }

    public async Task<Response> SendSpecialUseReviewConfirmationEmailAsync(string toEmail, string subject, string message)
    {
        string plainTextContent = message;
        string htmlContent = $"<p>{message}</p>";
        return await SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
    }
}
