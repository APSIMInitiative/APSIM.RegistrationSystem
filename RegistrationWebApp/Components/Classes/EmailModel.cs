
using System.ComponentModel.DataAnnotations;

namespace RegistrationWebApp.Components.Classes;

    /// <summary>
    /// Model for the email input form, with validation attributes to ensure a valid email address is entered.
    /// </summary>
    public class EmailModel
    {
        public string? Email { get; set; }

    }