namespace Confirmo.Api.Services;

public interface IStorageService
{
    Task<string> UploadVoucherAsync(Guid empresaId, Guid vendedorId, byte[] imageBytes, string contentType);
    Task<byte[]> DownloadVoucherAsync(string objectName);
    Task DeleteVoucherAsync(string objectName);
    Task<string> GetSignedUrlAsync(string objectName, TimeSpan? duration = null);
}