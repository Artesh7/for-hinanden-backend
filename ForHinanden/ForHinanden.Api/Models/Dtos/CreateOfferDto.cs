namespace ForHinanden.Api.Models;

public class CreateOfferDto
{
    public string OfferedBy { get; set; } = null!; // bruger-id
    public string? Message { get; set; }           // valgfri
}