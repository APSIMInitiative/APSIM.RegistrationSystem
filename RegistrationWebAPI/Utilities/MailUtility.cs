using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;

namespace RegistrationWebAPI.Utilities;

/// <summary>
/// Class for handling the sending of emails.
/// </summary>
public class MailUtility
{
    private const string LogoUrl = "https://www.apsim.info/wp-content/uploads/2026/05/APSIM_transparent-154x100-1.png";

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

    private async Task<Response> SendEmailAsync(
        string toEmail,
        string subject,
        string plainTextContent,
        string htmlContent,
        IEnumerable<string>? ccEmails = null)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("SendGrid client is not initialized. Please provide an API key.");
        }

        var from = new EmailAddress(_fromEmail, _fromEmailName);
        var to = new EmailAddress(toEmail);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        if (ccEmails is not null)
        {
            foreach (var ccEmail in ccEmails)
            {
                if (!string.IsNullOrWhiteSpace(ccEmail))
                {
                    msg.AddCc(new EmailAddress(ccEmail.Trim()));
                }
            }
        }

        return await _client.SendEmailAsync(msg);
    }

    public async Task<Response> SendVerificationEmailAsync(string toEmail, string verificationLink)
    {
        string subject = "Verify your email for APSIM Registration System";
        string plainTextContent = $"Please verify your email by clicking the following link: {verificationLink}";
        string htmlContent = 
            $@"<html>
            <body style=""margin:0;padding:0;background:#f4f8f2;font-family:Arial,sans-serif;color:#1f2937;"">
                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:#f4f8f2;"">
                <tr><td align=""center"" style=""padding:40px 16px;"">
                    <table cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:#ffffff;border-radius:16px;box-shadow:0 12px 40px rgba(0,0,0,0.08);max-width:520px;width:100%;padding:40px 32px;text-align:center;"">
                    <tr><td align=""center"" style=""padding-bottom:24px;"">
                        <img src=""{LogoUrl}"" alt=""APSIM"" width=""154"" height=""100"" style=""display:block;margin:0 auto;"" />
                    </td></tr>
                    <tr><td align=""center"" style=""padding-bottom:16px;"">
                        <h1 style=""margin:0;color:#2f8f2f;font-size:2rem;"">Verify Your Email</h1>
                    </td></tr>
                    <tr><td align=""center"" style=""padding-bottom:24px;font-size:1rem;line-height:1.6;"">
                        <p style=""margin:0;"">Please verify your email address to complete your APSIM registration.</p>
                    </td></tr>
                    <tr><td align=""center"">
                        <table border=""0"" cellspacing=""0"" cellpadding=""0"" style=""margin:0 auto;"">
                        <tr><td align=""center"" bgcolor=""#2f8f2f"" style=""border-radius:999px;box-shadow:0 8px 20px rgba(47,143,47,0.25);"">
                            <a href=""{verificationLink}"" target=""_blank"" style=""background:#2f8f2f;color:#ffffff;text-decoration:none;border-radius:999px;padding:14px 28px;font-size:1rem;font-weight:600;display:inline-block;"">Verify Email</a>
                        </td></tr>
                        </table>
                    </td></tr>
                    </table>
                </td></tr>
                </table>
            </body>
            </html>";
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

    public async Task<Response> SendDownloadLinkEmailAsync(string toEmail, string downloadLink)
    {
        string subject = "Your APSIM Download Link";
        string plainTextContent = $"Your download link is ready. Click the following link to access your download. This link expires in 48 hours: {downloadLink}";
        string htmlContent = GetDownloadLinkEmailHtml(downloadLink);
        return await SendEmailAsync(toEmail, subject, plainTextContent, htmlContent);
    }

    public async Task<Response> SendOrganisationVerificationSummaryEmailAsync(
        string toEmail,
        string organisationName,
        string contactName,
        string contactEmail,
        string contactPhone,
        string contactAddress,
        IEnumerable<string> organisationEmails,
        string licencePathway,
        string annualTurnover,
        DateTime dateCreatedUtc)
    {
        const string subject = "APSIM Special Use Registration Submitted";
        const string businessManagerEmail = "APSIM@csiro.au";
        string emailList = string.Join(", ", organisationEmails.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));
        if (string.IsNullOrWhiteSpace(emailList))
        {
            emailList = "Not provided";
        }

        string createdAtText = dateCreatedUtc.ToString("yyyy-MM-dd HH:mm 'UTC'");

        string plainTextContent =
            $"Thank you for submitting an application to the APSIM Registration System.{Environment.NewLine}{Environment.NewLine}" +
            $"Your organisation email has been verified and your registration details were submitted for APSIM business review.{Environment.NewLine}{Environment.NewLine}" +
            $"Organisation: {organisationName}{Environment.NewLine}" +
            $"Contact Name: {contactName}{Environment.NewLine}" +
            $"Contact Email: {contactEmail}{Environment.NewLine}" +
            $"Contact Phone: {contactPhone}{Environment.NewLine}" +
            $"Contact Address: {contactAddress}{Environment.NewLine}" +
            $"Organisation Emails/Domains: {emailList}{Environment.NewLine}" +
            $"Licence Pathway: {licencePathway}{Environment.NewLine}" +
            $"Annual Turnover: {annualTurnover}{Environment.NewLine}" +
            $"Date Submitted: {createdAtText}{Environment.NewLine}{Environment.NewLine}" +
            "The APSIM Initiative business manager will be in touch shortly to confirm information and to setup billing.";

        string htmlContent = GetOrganisationVerificationSummaryHtml(
            organisationName,
            contactName,
            contactEmail,
            contactPhone,
            contactAddress,
            emailList,
            licencePathway,
            annualTurnover,
            createdAtText);

        return await SendEmailAsync(
            toEmail,
            subject,
            plainTextContent,
            htmlContent,
            new[] { businessManagerEmail });
    }

    private static string Encode(string value)
    {
        return WebUtility.HtmlEncode(value);
    }

    private string GetOrganisationVerificationSummaryHtml(
        string organisationName,
        string contactName,
        string contactEmail,
        string contactPhone,
        string contactAddress,
        string emailList,
        string licencePathway,
        string annualTurnover,
        string createdAtText)
    {
        return $@"<html>
            <body style=""margin:0;padding:0;background:#f4f8f2;font-family:Arial,sans-serif;color:#1f2937;"">
                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:#f4f8f2;"">
                <tr><td align=""center"" style=""padding:40px 16px;"">
                    <table cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:#ffffff;border-radius:16px;box-shadow:0 12px 40px rgba(0,0,0,0.08);max-width:620px;width:100%;padding:40px 32px;text-align:left;"">
                    <tr><td align=""center"" style=""padding-bottom:24px;"">
                        <img src=""{LogoUrl}"" alt=""APSIM"" width=""154"" height=""100"" style=""display:block;margin:0 auto;"" />
                    </td></tr>
                    <tr><td align=""center"" style=""padding-bottom:16px;"">
                        <h1 style=""margin:0;color:#2f8f2f;font-size:1.8rem;"">Special Use Registration Submitted</h1>
                    </td></tr>
                    <tr><td align=""center"" style=""padding-bottom:24px;font-size:1rem;line-height:1.6;"">
                        <p style=""margin:0 0 12px;"">Thank you for submitting an application to the APSIM Registration System.</p>
                        <p style=""margin:0;"">Your email has been verified and the APSIM business manager has been copied with the registration summary below.</p>
                    </td></tr>
                    <tr><td>
                        <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""border-collapse:collapse;font-size:0.95rem;"">
                            <tr><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;color:#4b5563;width:38%;font-weight:600;"">Organisation</td><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;"">{Encode(organisationName)}</td></tr>
                            <tr><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;color:#4b5563;font-weight:600;"">Contact Name</td><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;"">{Encode(contactName)}</td></tr>
                            <tr><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;color:#4b5563;font-weight:600;"">Contact Email</td><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;"">{Encode(contactEmail)}</td></tr>
                            <tr><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;color:#4b5563;font-weight:600;"">Contact Phone</td><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;"">{Encode(contactPhone)}</td></tr>
                            <tr><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;color:#4b5563;font-weight:600;"">Contact Address</td><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;"">{Encode(contactAddress)}</td></tr>
                            <tr><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;color:#4b5563;font-weight:600;"">Organisation Emails/Domains</td><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;"">{Encode(emailList)}</td></tr>
                            <tr><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;color:#4b5563;font-weight:600;"">Licence Pathway</td><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;"">{Encode(licencePathway)}</td></tr>
                            <tr><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;color:#4b5563;font-weight:600;"">Annual Turnover</td><td style=""padding:10px 0;border-bottom:1px solid #e5e7eb;"">{Encode(annualTurnover)}</td></tr>
                            <tr><td style=""padding:10px 0;color:#4b5563;font-weight:600;"">Date Submitted</td><td style=""padding:10px 0;"">{Encode(createdAtText)}</td></tr>
                        </table>
                    </td></tr>
                    <tr><td style=""padding-top:24px;font-size:1rem;line-height:1.6;"">
                        <p style=""margin:0;"">The APSIM Initiative business manager will be in touch shortly to confirm information and to setup billing.</p>
                    </td></tr>
                    </table>
                </td></tr>
                </table>
            </body>
            </html>";
    }

    private string GetDownloadLinkEmailHtml(string downloadLink)
    {
        return $@"<html>
            <body style=""margin:0;padding:0;background:#f4f8f2;font-family:Arial,sans-serif;color:#1f2937;"">
                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:#f4f8f2;"">
                <tr><td align=""center"" style=""padding:40px 16px;"">
                    <table cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background:#ffffff;border-radius:16px;box-shadow:0 12px 40px rgba(0,0,0,0.08);max-width:520px;width:100%;padding:40px 32px;text-align:center;"">
                    <tr><td align=""center"" style=""padding-bottom:24px;"">
                        <img src=""{LogoUrl}"" alt=""APSIM"" width=""154"" height=""100"" style=""display:block;margin:0 auto;"" />
                    </td></tr>
                    <tr><td align=""center"" style=""padding-bottom:16px;"">
                        <h1 style=""margin:0;color:#2f8f2f;font-size:2rem;"">Your Download Link</h1>
                    </td></tr>
                    <tr><td align=""center"" style=""padding-bottom:16px;font-size:1rem;line-height:1.6;"">
                        <p style=""margin:0 0 12px;"">Click the button below to access downloads.</p>
                    </td></tr>
                    <tr><td align=""center"" style=""padding-bottom:24px;font-size:0.9rem;color:#666;line-height:1.6;"">
                        <p style=""margin:0;""><strong>Note:</strong> This link expires in <strong>48 hours</strong>.</p>
                    </td></tr>
                    <tr><td align=""center"">
                        <table border=""0"" cellspacing=""0"" cellpadding=""0"" style=""margin:0 auto;"">
                        <tr><td align=""center"" bgcolor=""#2f8f2f"" style=""border-radius:999px;box-shadow:0 8px 20px rgba(47,143,47,0.25);"">
                            <a href=""{downloadLink}"" target=""_blank"" style=""background:#2f8f2f;color:#ffffff;text-decoration:none;border-radius:999px;padding:14px 28px;font-size:1rem;font-weight:600;display:inline-block;"">Go to downloads</a>
                        </td></tr>
                        </table>
                    </td></tr>
                    <tr><td align=""center"" style=""padding-top:24px;font-size:0.85rem;color:#999;"">
                        <p style=""margin:0;"">If you didn't request this download, please disregard this email.</p>
                    </td></tr>
                    </table>
                </td></tr>
                </table>
            </body>
            </html>";
    }
}


