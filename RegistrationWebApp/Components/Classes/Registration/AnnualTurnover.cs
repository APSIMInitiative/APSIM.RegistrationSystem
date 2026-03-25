using System.ComponentModel;

namespace RegistrationWebApp.Components.Classes.Registration;


public enum AnnualTurnover
{
    [Description("Less than $2 Million AUD")]
    BelowTwoMillion,
    [Description("$2 Million - $40 Million AUD")]
    TwoToFortyMillion,
    [Description("Above $40 Million AUD")]
    AboveFortyMillion,
}
