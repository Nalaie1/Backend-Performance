namespace Application.DTOs.Upload;

public class UploadFileDto
{
    public Guid UserId { get; set; }     
    public string FileName { get; set; } = null!; 
    public byte[] FileContent { get; set; } = null!; 
}