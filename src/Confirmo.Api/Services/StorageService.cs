using Google.Cloud.Storage.V1;

namespace Confirmo.Api.Services;

public class StorageService : IStorageService
{
    private readonly StorageClient _client;
    private readonly string _bucketName;
    private readonly ILogger<StorageService> _logger;

    public StorageService(IConfiguration config, ILogger<StorageService> logger)
    {
        _client = StorageClient.Create();
        _bucketName = config["Gcp:StorageBucket"]!;
        _logger = logger;
    }

    public async Task<string> UploadVoucherAsync(Guid empresaId, Guid vendedorId, byte[] imageBytes, string contentType)
    {
        var objectName = $"{empresaId}/{vendedorId}/{Guid.NewGuid()}.jpg";

        using var stream = new MemoryStream(imageBytes);
        await _client.UploadObjectAsync(_bucketName, objectName, contentType, stream);

        _logger.LogInformation("Voucher subido a GCS: {ObjectName}", objectName);
        return objectName;
    }

    public async Task<byte[]> DownloadVoucherAsync(string objectName)
    {
        using var stream = new MemoryStream();
        await _client.DownloadObjectAsync(_bucketName, objectName, stream);
        return stream.ToArray();
    }

    public async Task DeleteVoucherAsync(string objectName)
    {
        await _client.DeleteObjectAsync(_bucketName, objectName);
    }
}