using System.Threading.Tasks;

namespace Khadamat.Shared.Interfaces;

/// <summary>
/// Interface for device camera operations
/// </summary>
public interface IDeviceCameraService
{
    /// <summary>
    /// Capture photo from camera
    /// </summary>
    Task<byte[]?> CapturePhotoAsync();

    /// <summary>
    /// Pick photo from gallery
    /// </summary>
    Task<byte[]?> PickPhotoAsync();

    /// <summary>
    /// Pick multiple photos from gallery
    /// </summary>
    Task<List<byte[]>> PickMultiplePhotosAsync(int maxCount = 5);

    /// <summary>
    /// Compress image to specified quality
    /// </summary>
    Task<byte[]> CompressImageAsync(byte[] imageData, int quality = 80);

    /// <summary>
    /// Check if camera is available
    /// </summary>
    Task<bool> IsCameraAvailableAsync();
}
