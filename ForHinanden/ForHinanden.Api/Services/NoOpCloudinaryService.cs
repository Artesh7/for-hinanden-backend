// ...existing code...
using System.Net;
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;

namespace ForHinanden.Api.Services;

public class NoOpCloudinaryService : ICloudinaryService
{
    public Task<ImageUploadResult> UploadAsync(ImageUploadParams uploadParams)
    {
        var res = new ImageUploadResult
        {
            SecureUrl = new System.Uri("https://via.placeholder.com/400"),
            StatusCode = HttpStatusCode.OK
        };
        return Task.FromResult(res);
    }

    public Task<DeletionResult> DestroyAsync(DeletionParams deletionParams)
    {
        var res = new DeletionResult
        {
            Result = "ok",
            StatusCode = HttpStatusCode.OK
        };
        return Task.FromResult(res);
    }
}

