// ...existing code...
using System.Threading.Tasks;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace ForHinanden.Api.Services;

public class CloudinaryWrapper : ICloudinaryService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryWrapper(Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }

    public Task<ImageUploadResult> UploadAsync(ImageUploadParams uploadParams)
    {
        return _cloudinary.UploadAsync(uploadParams);
    }

    public Task<DeletionResult> DestroyAsync(DeletionParams deletionParams)
    {
        return _cloudinary.DestroyAsync(deletionParams);
    }
}

