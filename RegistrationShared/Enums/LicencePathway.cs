using System.ComponentModel;

namespace RegistrationShared.Enums;

/// <summary>
/// Defines the possible licence pathways for an organisation in the registration system.
/// Each pathway represents a different approach to how modifications to the 
/// software are shared and retained except for the APSIM Initiative Member 
/// pathway, which indicates that the organisation is a member of the APSIM Initiative.
/// </summary>
public enum LicencePathway
{
    [Description("Type 1 - Modifications shared with APSIM Initiative")]
    TypeOne,
    [Description("Type 2 - Modifications retained privately")]
    TypeTwo,
    [Description("Member Organisation of the APSIM Initiative")]
    APSIMInitiativeMember
}
