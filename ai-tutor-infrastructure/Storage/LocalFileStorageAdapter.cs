namespace Ai.Tutor.Infrastructure.Storage;

using Ai.Tutor.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public sealed class LocalFileStorageAdapter : IFileStorageAdapter
{
    private readonly string rootPath;
    private readonly ILogger<LocalFileStorageAdapter> logger;

    public LocalFileStorageAdapter(IConfiguration configuration, ILogger<LocalFileStorageAdapter> logger)
    {
        this.logger = logger;
        var configured = configuration["Storage:Files:RootPath"];
        this.rootPath = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(AppContext.BaseDirectory, "storage", "files")
            : configured!;
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        Directory.CreateDirectory(this.rootPath);

        var ext = Path.GetExtension(fileName);
        var key = string.IsNullOrWhiteSpace(ext) ? Guid.NewGuid().ToString("N") : $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(this.rootPath, key);

        this.logger.LogInformation("Saving file to {FullPath} (contentType={ContentType}, name={FileName})", fullPath, contentType, fileName);

        try
        {
            await using var output = File.Create(fullPath);
            await fileStream.CopyToAsync(output, ct);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error saving file to local storage");
            throw;
        }

        return key;
    }

    public Task<Stream> DownloadAsync(string storageKey, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(this.rootPath, storageKey);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("File not found in local storage", fullPath);
        }

        Stream stream = File.OpenRead(fullPath);
        return Task.FromResult(stream);
    }

    public Task<string?> GetPresignedUrlAsync(string storageKey, TimeSpan expiry, CancellationToken ct = default)
    {
        // For local storage, return a relative path that could be served by the API/static files middleware.
        string relative = Path.Combine("storage", "files", storageKey).Replace('\\', '/');
        return Task.FromResult<string?>(relative);
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(this.rootPath, storageKey);
        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                this.logger.LogInformation("Deleted file at {FullPath}", fullPath);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error deleting file at {FullPath}", fullPath);
            throw;
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string storageKey, CancellationToken ct = default)
    {
        var fullPath = Path.Combine(this.rootPath, storageKey);
        return Task.FromResult(File.Exists(fullPath));
    }
}
