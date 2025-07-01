using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmgram_mk1.src.JMgram.Core.UseCases
{
    public interface IUploadFileUseCase
    {
        Task<UploadFileResponse> Execute(UploadFileRequest request);
    }

    public class UploadFileRequest
    {
        public IFormFile File { get; set; }
        public string ChatId { get; set; }
        public string UserId { get; set; }
    }

    public class UploadFileResponse
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string FileType { get; set; }
    }
    public class UploadFileUseCase : IUploadFileUseCase
    {
        private readonly ILogger<UploadFileUseCase> _logger;
        private readonly string _uploadPath = @"C:\Users\maxco\source\repos\Jmgram mk1\Jmgram Server\Uploaded Files";
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".avi", ".pdf", ".doc", ".docx" };

        public UploadFileUseCase(ILogger<UploadFileUseCase> logger)
        {
            _logger = logger;

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<UploadFileResponse> Execute(UploadFileRequest request)
        {
            try
            {
                if (request.File == null || request.File.Length == 0)
                {
                    return new UploadFileResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Файл не выбран или пустой"
                    };
                }

                if (request.File.Length > _maxFileSize)
                {
                    return new UploadFileResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Файл слишком большой. Максимальный размер: 10MB"
                    };
                }

                // Проверка расширения файла
                var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(fileExtension))
                {
                    return new UploadFileResponse
                    {
                        IsSuccess = false,
                        ErrorMessage = "Неподдерживаемый тип файла"
                    };
                }

                // Генерируем уникальное имя файла
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(_uploadPath, fileName);

                // Сохраняем файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.File.CopyToAsync(stream);
                }

                _logger.LogInformation($"File uploaded successfully: {fileName}, Size: {request.File.Length}, User: {request.UserId}");

                return new UploadFileResponse
                {
                    IsSuccess = true,
                    FilePath = filePath,
                    FileName = fileName,
                    FileSize = request.File.Length,
                    FileType = request.File.ContentType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return new UploadFileResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Ошибка при загрузке файла"
                };
            }
        }
    }
}
