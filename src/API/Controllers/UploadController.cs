using Application.DTOs.Upload;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Application.Interfaces.Upload;
using Application.Services;

namespace API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]  
[ApiVersion("1.0")]  
[Authorize]
public class UploadController : ControllerBase
{
    private readonly IUploadService _uploadService;
    private readonly IWebHostEnvironment _env;

    public UploadController(IUploadService uploadService, IWebHostEnvironment env)
    {
        _uploadService = uploadService;
        _env = env;
    }

    [HttpPost("file")]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // Lấy userId từ JWT
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return BadRequest("Invalid user");

        // Đọc file thành byte[]
        byte[] fileBytes;
        await using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        // Tạo DTO
        var dto = new UploadFileDto
        {
            UserId = userId,
            FileName = file.FileName,
            FileContent = fileBytes
        };
        
        var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
        var uploadServiceWithPath = new UploadService(uploadsPath); 

        var result = await uploadServiceWithPath.UploadFileAsync(dto);

        return Ok(new
        {
            Message = "File uploaded successfully",
            Path = result.FilePath
        });
    }
}