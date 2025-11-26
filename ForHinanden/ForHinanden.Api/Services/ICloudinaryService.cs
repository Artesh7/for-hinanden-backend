// ...existing code...
using System.Threading.Tasks;
using CloudinaryDotNet.Actions;

namespace ForHinanden.Api.Services;

public interface ICloudinaryService
{
    Task<ImageUploadResult> UploadAsync(ImageUploadParams uploadParams);
    Task<DeletionResult> DestroyAsync(DeletionParams deletionParams);
}

