using Application.DTOs.Upload;
using Application.Interfaces;
using Application.Interfaces.Upload;

namespace Application.Services;

public class UploadService : IUploadService
{
    private readonly string _basePath;
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

    public UploadService(string basePath)
    {
        _basePath = basePath;
    }

    public async Task<UploadResultDto> UploadFileAsync(UploadFileDto dto)
    {
        // ===== Validate =====
        if (dto.FileContent.Length == 0)
            throw new InvalidOperationException("File is empty.");

        if (dto.FileContent.Length > _maxFileSize)
            throw new InvalidOperationException("File is too large.");

        var extension = Path.GetExtension(dto.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Invalid file type.");

        // ===== Lưu file =====
        var userFolder = Path.Combine(_basePath, dto.UserId.ToString());
        Directory.CreateDirectory(userFolder);

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(userFolder, fileName);

        await File.WriteAllBytesAsync(filePath, dto.FileContent);

        return new UploadResultDto
        {
            FilePath = filePath
        };
    }
}