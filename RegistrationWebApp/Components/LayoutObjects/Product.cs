
using Microsoft.AspNetCore.Components;

namespace RegistrationWebApp.Components.LayoutObjects;

public class Product
{
    public required string Name { get; set; }
    public required MarkupString Description { get; set; }
    public required string ImageUrl { get; set; }

    public required string Href { get; set; }
    public required string FirstButtonText { get; set; }

    public string SecondHref { get; set; } = string.Empty;
    public string SecondButtonText { get; set; } = string.Empty;
}
