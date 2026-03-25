using System.ComponentModel;

namespace RegistrationWebApp.Components.Classes.Utilities;

public static class EnumUtility
{
    public static string GetDescription(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var descriptionAttribute = fieldInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
        return descriptionAttribute != null ? descriptionAttribute.Description : value.ToString();
    }
}