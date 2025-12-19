using Application.DTOs.Upload;

namespace Application.Interfaces.Upload;

public interface IUploadService
{
    Task<UploadResultDto> UploadFileAsync(UploadFileDto dto);
}