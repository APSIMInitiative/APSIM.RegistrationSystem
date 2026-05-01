using System.ComponentModel;

namespace RegistrationShared.Enums;

/// <summary>
/// Defines the possible annual turnover categories for an organisation in the registration system.
/// Each category represents a different range of annual turnover in 
/// Australian dollars (AUD).
/// The "Not Applicable" category is used for organisations that do not have 
/// a relevant annual turnover.
/// </summary>
public enum AnnualTurnover
{

    [Description("Less than $2 Million AUD")]
    BelowTwoMillion,
    [Description("$2 Million - $40 Million AUD")]
    TwoToFortyMillion,
    [Description("Above $40 Million AUD")]
    AboveFortyMillion,
    [Description("Not Applicable")]
    NotApplicable,
}
