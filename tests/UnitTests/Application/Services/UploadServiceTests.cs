using System;
using System.IO;
using System.Threading.Tasks;
using Application.DTOs.Upload;
using Application.Services;
using FluentAssertions;
using Xunit;

namespace UnitTests.Application.Services
{
    public class UploadServiceTests
    {
        [Fact]
        public async Task UploadFileAsync_Throws_WhenFileEmpty()
        {
            var basePath = Path.Combine(Path.GetTempPath(), "upload_test_empty", Guid.NewGuid().ToString());
            var service = new UploadService(basePath);

            var dto = new UploadFileDto
            {
                UserId = Guid.NewGuid(),
                FileName = "image.jpg",
                FileContent = Array.Empty<byte>()
            };

            Func<Task> act = async () => await service.UploadFileAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("File is empty.");
        }

        [Fact]
        public async Task UploadFileAsync_Throws_WhenFileTooLarge()
        {
            var basePath = Path.Combine(Path.GetTempPath(), "upload_test_large", Guid.NewGuid().ToString());
            var service = new UploadService(basePath);

            // create 6MB buffer (> 5MB limit)
            var large = new byte[6 * 1024 * 1024];

            var dto = new UploadFileDto
            {
                UserId = Guid.NewGuid(),
                FileName = "image.jpg",
                FileContent = large
            };

            Func<Task> act = async () => await service.UploadFileAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("File is too large.");
        }

        [Fact]
        public async Task UploadFileAsync_Throws_WhenInvalidExtension()
        {
            var basePath = Path.Combine(Path.GetTempPath(), "upload_test_invalid_ext", Guid.NewGuid().ToString());
            var service = new UploadService(basePath);

            var dto = new UploadFileDto
            {
                UserId = Guid.NewGuid(),
                FileName = "payload.exe",
                FileContent = new byte[] {1,2,3}
            };

            Func<Task> act = async () => await service.UploadFileAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid file type.");
        }

        [Fact]
        public async Task UploadFileAsync_SavesFileAndReturnsPath()
        {
            var basePath = Path.Combine(Path.GetTempPath(), "upload_test_success", Guid.NewGuid().ToString());
            Directory.CreateDirectory(basePath);

            var service = new UploadService(basePath);

            var userId = Guid.NewGuid();
            var content = new byte[] { 10, 20, 30, 40, 50 };
            var dto = new UploadFileDto
            {
                UserId = userId,
                FileName = "photo.PNG", // uppercase to test ToLowerInvariant handling
                FileContent = content
            };

            string? resultPath = null;
            try
            {
                var result = await service.UploadFileAsync(dto);
                resultPath = result.FilePath;

                // file exists
                File.Exists(resultPath).Should().BeTrue();

                // path is under basePath and contains userId folder
                Path.GetFullPath(resultPath).Should().StartWith(Path.GetFullPath(Path.Combine(basePath, userId.ToString())));

                // extension preserved and lowercased
                Path.GetExtension(resultPath).Should().Be(".png");

                // content matches
                var saved = await File.ReadAllBytesAsync(resultPath);
                saved.Should().Equal(content);
            }
            finally
            {
                // cleanup
                try
                {
                    if (!string.IsNullOrEmpty(resultPath) && File.Exists(resultPath))
                        File.Delete(resultPath);
                }
                catch { }

                try
                {
                    var userFolder = Path.Combine(basePath, userId.ToString());
                    if (Directory.Exists(userFolder))
                        Directory.Delete(userFolder, true);
                }
                catch { }

                try
                {
                    if (Directory.Exists(basePath))
                        Directory.Delete(basePath, true);
                }
                catch { }
            }
        }
    }
}

