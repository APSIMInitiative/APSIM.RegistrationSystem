using System.ComponentModel;

namespace RegistrationShared.Enums;

public enum AnnualTurnover
{
    [Description("Less than $2 Million AUD")]
    BelowTwoMillion,
    [Description("$2 Million - $40 Million AUD")]
    TwoToFortyMillion,
    [Description("Above $40 Million AUD")]
    AboveFortyMillion,
}
