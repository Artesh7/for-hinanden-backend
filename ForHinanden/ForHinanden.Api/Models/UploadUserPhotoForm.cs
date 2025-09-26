using Microsoft.AspNetCore.Http;

namespace ForHinanden.Api.Models;

/// <summary>
/// multipart/form-data: indeholder både deviceId og filen.
/// Felt-navne: "deviceId" og "file"
/// </summary>
public class UploadUserPhotoForm
{
    public string deviceId { get; set; } = null!;
    public IFormFile file { get; set; } = default!;
}