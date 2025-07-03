using Jmgram_mk1.src.JMgram.Core.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;

[ApiController]
[Route("api/[controller]")]
[EnableCors("AllowReactApps")]
public class FileController : ControllerBase
{
    private readonly IUploadFileUseCase _uploadFileUseCase;
    private readonly ILogger<FileController> _logger;
    private readonly string _uploadPath = @"C:\Users\maxco\source\repos\Jmgram mk1\Jmgram Server\Uploaded Files";

    public FileController(IUploadFileUseCase uploadFileUseCase, ILogger<FileController> logger)
    {
        _uploadFileUseCase = uploadFileUseCase;
        _logger = logger;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [Authorize]
    public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request)
    {
        try
        {
            var origin = Request.Headers["Origin"].ToString();
            if (!string.IsNullOrEmpty(origin))
            {
                Response.Headers.Add("Access-Control-Allow-Origin", origin);
                Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var uploadRequest = new UploadFileRequest
            {
                File = request.File,
                ChatId = request.ChatId,
                UserId = userId
            };

            var response = await _uploadFileUseCase.Execute(uploadRequest);

            if (response.IsSuccess)
            {
                return Ok(new
                {
                    success = true,
                    fileName = response.FileName,
                    fileSize = response.FileSize,
                    fileType = response.FileType,
                    filePath = $"/api/file/download/{response.FileName}"
                });
            }

            return BadRequest(new { success = false, message = response.ErrorMessage });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in file upload endpoint");
            return StatusCode(500, new { success = false, message = "Внутренняя ошибка сервера" });
        }
    }

    // УПРОЩЕННАЯ версия для диагностики
    [HttpGet("download/{fileName}")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client)]
    [AllowAnonymous]
    public IActionResult DownloadFile(string fileName)
    {
        try
        {
            _logger.LogError($"=== ДИАГНОСТИКА DOWNLOAD ===");
            _logger.LogError($"1. Запрошенный файл: '{fileName}'");
            _logger.LogError($"2. Папка загрузок: '{_uploadPath}'");

            // CORS заголовки
            Response.Headers.Add("Access-Control-Allow-Origin", "*");

            // Проверка имени файла
            if (string.IsNullOrWhiteSpace(fileName))
            {
                _logger.LogError("3. ОШИБКА: Имя файла пустое");
                return BadRequest(new { message = "Имя файла пустое" });
            }

            // Декодирование
            string decodedFileName;
            try
            {
                decodedFileName = Uri.UnescapeDataString(fileName);
                _logger.LogError($"4. Декодированное имя: '{decodedFileName}'");
            }
            catch (Exception ex)
            {
                _logger.LogError($"4. ОШИБКА декодирования: {ex.Message}");
                decodedFileName = fileName; // Используем как есть
            }

            // Проверка папки
            if (!Directory.Exists(_uploadPath))
            {
                _logger.LogError($"5. ОШИБКА: Папка не существует: '{_uploadPath}'");
                return NotFound(new { message = "Папка не найдена", path = _uploadPath });
            }
            _logger.LogError($"5. Папка существует: OK");

            // Список файлов в папке
            try
            {
                var allFiles = Directory.GetFiles(_uploadPath);
                _logger.LogError($"6. Файлов в папке: {allFiles.Length}");
                for (int i = 0; i < Math.Min(allFiles.Length, 10); i++) // Показываем первые 10
                {
                    var fileInfo = new FileInfo(allFiles[i]);
                    _logger.LogError($"   - '{Path.GetFileName(allFiles[i])}' ({fileInfo.Length} bytes)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"6. ОШИБКА получения списка файлов: {ex.Message}");
            }

            // Полный путь к файлу
            var filePath = Path.Combine(_uploadPath, decodedFileName);
            _logger.LogError($"7. Полный путь: '{filePath}'");

            // Проверка существования файла
            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogError($"8. ОШИБКА: Файл не найден: '{filePath}'");

                // Поиск похожих файлов
                try
                {
                    var similarFiles = Directory.GetFiles(_uploadPath)
                        .Where(f => Path.GetFileName(f).Contains(decodedFileName, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (similarFiles.Any())
                    {
                        _logger.LogError($"9. Найдены похожие файлы:");
                        foreach (var similar in similarFiles)
                        {
                            _logger.LogError($"   - '{Path.GetFileName(similar)}'");
                        }
                    }
                    else
                    {
                        _logger.LogError($"9. Похожих файлов не найдено");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"9. ОШИБКА поиска похожих файлов: {ex.Message}");
                }

                return NotFound(new
                {
                    message = "Файл не найден",
                    requestedFile = decodedFileName,
                    fullPath = filePath
                });
            }
            _logger.LogError($"8. Файл найден: OK");

            // Информация о файле
            FileInfo targetFileInfo;
            try
            {
                targetFileInfo = new FileInfo(filePath);
                _logger.LogError($"9. Размер файла: {targetFileInfo.Length} bytes");
                _logger.LogError($"10. Создан: {targetFileInfo.CreationTime}");
                _logger.LogError($"11. Изменен: {targetFileInfo.LastWriteTime}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"9-11. ОШИБКА получения информации о файле: {ex.Message}");
                return StatusCode(500, new { message = "Ошибка получения информации о файле" });
            }

            // Проверка доступа к файлу
            try
            {
                using (var testStream = System.IO.File.OpenRead(filePath))
                {
                    _logger.LogError($"12. Доступ к файлу: OK (можно читать)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"12. ОШИБКА доступа к файлу: {ex.Message}");
                return StatusCode(500, new { message = "Нет доступа к файлу", error = ex.Message });
            }

            // Чтение файла
            byte[] fileBytes;
            try
            {
                fileBytes = System.IO.File.ReadAllBytes(filePath);
                _logger.LogError($"13. Файл прочитан: {fileBytes.Length} bytes");
            }
            catch (Exception ex)
            {
                _logger.LogError($"13. ОШИБКА чтения файла: {ex.Message}");
                return StatusCode(500, new { message = "Ошибка чтения файла", error = ex.Message });
            }

            // Тип контента
            var contentType = GetContentType(decodedFileName);
            _logger.LogError($"14. Тип контента: {contentType}");

            // Заголовки ответа
            try
            {
                Response.Headers.Add("Cache-Control", "public, max-age=3600");
                Response.Headers.Add("ETag", $"\"{decodedFileName}\"");
                Response.Headers.Add("Content-Disposition", $"inline; filename=\"{decodedFileName}\"");
                _logger.LogError($"15. Заголовки установлены: OK");
            }
            catch (Exception ex)
            {
                _logger.LogError($"15. ОШИБКА установки заголовков: {ex.Message}");
            }

            _logger.LogError($"=== УСПЕШНОЕ ЗАВЕРШЕНИЕ ===");

            return File(fileBytes, contentType, decodedFileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"КРИТИЧЕСКАЯ ОШИБКА в DownloadFile:");
            _logger.LogError($"Тип исключения: {ex.GetType().Name}");
            _logger.LogError($"Сообщение: {ex.Message}");
            _logger.LogError($"StackTrace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                _logger.LogError($"Внутреннее исключение: {ex.InnerException.Message}");
            }

            return StatusCode(500, new
            {
                message = "Критическая ошибка сервера",
                error = ex.Message,
                type = ex.GetType().Name
            });
        }
    }

    [HttpHead("download/{fileName}")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Client)]
    [AllowAnonymous]
    public IActionResult CheckFileExists(string fileName)
    {
        try
        {
            _logger.LogInformation($"HEAD ��апрос для файла: {fileName}");

            var origin = Request.Headers["Origin"].ToString();
            if (!string.IsNullOrEmpty(origin))
            {
                Response.Headers.Add("Access-Control-Allow-Origin", origin);
                Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
            else
            {
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest();
            }

            var decodedFileName = Uri.UnescapeDataString(fileName);
            var filePath = Path.Combine(_uploadPath, decodedFileName);

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning($"HEAD: Файл не найден: {filePath}");
                return NotFound();
            }

            var fileInfo = new FileInfo(filePath);
            var contentType = GetContentType(decodedFileName);

            Response.Headers.Add("Content-Type", contentType);
            Response.Headers.Add("Content-Length", fileInfo.Length.ToString());
            Response.Headers.Add("Cache-Control", "public, max-age=3600");
            Response.Headers.Add("ETag", $"\"{decodedFileName}\"");

            _logger.LogInformation($"HEAD: Файл найден: {decodedFileName} ({fileInfo.Length} bytes)");

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка в HEAD запросе: {fileName}");
            return StatusCode(500);
        }
    }

    [HttpGet("thumbnail/{fileName}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    [AllowAnonymous]
    public IActionResult GetThumbnail(string fileName, int width = 200, int height = 200)
    {
        try
        {
            var origin = Request.Headers["Origin"].ToString();
            if (!string.IsNullOrEmpty(origin))
            {
                Response.Headers.Add("Access-Control-Allow-Origin", origin);
                Response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
            else
            {
                Response.Headers.Add("Access-Control-Allow-Origin", "*");
            }

            var decodedFileName = Uri.UnescapeDataString(fileName);
            var filePath = Path.Combine(_uploadPath, decodedFileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var extension = Path.GetExtension(decodedFileName).ToLowerInvariant();

            if (new[] { ".jpg", ".jpeg", ".png", ".gif" }.Contains(extension))
            {
                var thumbnailBytes = CreateImageThumbnail(filePath, width, height);
                return File(thumbnailBytes, "image/jpeg");
            }

            if (new[] { ".mp4", ".avi", ".mov" }.Contains(extension))
            {
                return File(GetVideoIcon(), "image/svg+xml");
            }

            if (new[] { ".mp3", ".wav", ".ogg" }.Contains(extension))
            {
                return File(GetAudioIcon(), "image/svg+xml");
            }

            return File(GetFileIcon(), "image/svg+xml");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating thumbnail: {fileName}");
            return StatusCode(500);
        }
    }

    // Добавляем диагностический метод
    [HttpGet("debug/files")]
    [AllowAnonymous]
    public IActionResult ListFiles()
    {
        try
        {
            _logger.LogInformation($"=== ДИАГНОСТИКА ФАЙЛОВ ===");
            _logger.LogInformation($"Папка: {_uploadPath}");

            if (!Directory.Exists(_uploadPath))
            {
                return NotFound(new { message = "Папка не существует", path = _uploadPath });
            }

            var files = Directory.GetFiles(_uploadPath)
                .Select(f => new
                {
                    name = Path.GetFileName(f),
                    fullPath = f,
                    size = new FileInfo(f).Length,
                    created = new FileInfo(f).CreationTime,
                    modified = new FileInfo(f).LastWriteTime
                })
                .ToList();

            return Ok(new
            {
                uploadPath = _uploadPath,
                filesCount = files.Count,
                files = files
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в диагностике файлов");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpOptions("download/{fileName}")]
    [HttpOptions("thumbnail/{fileName}")]
    [AllowAnonymous]
    public IActionResult Options()
    {
        var origin = Request.Headers["Origin"].ToString();
        if (!string.IsNullOrEmpty(origin))
        {
            Response.Headers.Add("Access-Control-Allow-Origin", origin);
        }
        else
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        Response.Headers.Add("Access-Control-Allow-Methods", "GET, HEAD, POST, OPTIONS");
        Response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization, Range");
        Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        return Ok();
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".mp4" => "video/mp4",
            ".avi" => "video/avi",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }

    private byte[] CreateImageThumbnail(string imagePath, int width, int height)
    {
        try
        {
            return System.IO.File.ReadAllBytes(imagePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error reading image {imagePath}");
            return GetFileIcon();
        }
    }

    private byte[] GetVideoIcon()
    {
        var svgIcon = @"<svg width=""64"" height=""64"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
            <rect x=""2"" y=""3"" width=""20"" height=""14"" rx=""2"" ry=""2"" stroke=""#666"" stroke-width=""2"" fill=""#f0f0f0""/>
            <polygon points=""10,8 16,12 10,16"" fill=""#666""/>
        </svg>";
        return System.Text.Encoding.UTF8.GetBytes(svgIcon);
    }

    private byte[] GetAudioIcon()
    {
        var svgIcon = @"<svg width=""64"" height=""64"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
            <path d=""M12 2C13.1 2 14 2.9 14 4V12C14 13.1 13.1 14 12 14C10.9 14 10 13.1 10 12V4C10 2.9 10.9 2 12 2Z"" stroke=""#666"" stroke-width=""2"" fill=""#f0f0f0""/>
            <path d=""M19 10V12C19 15.87 15.87 19 12 19C8.13 19 5 15.87 5 12V10"" stroke=""#666"" stroke-width=""2"" fill=""none""/>
            <path d=""M12 19V22"" stroke=""#666"" stroke-width=""2""/>
            <path d=""M8 22H16"" stroke=""#666"" stroke-width=""2""/>
        </svg>";
        return System.Text.Encoding.UTF8.GetBytes(svgIcon);
    }

    private byte[] GetFileIcon()
    {
        var svgIcon = @"<svg width=""64"" height=""64"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
            <path d=""M14 2H6C4.9 2 4 2.9 4 4V20C4 21.1 4.9 22 6 22H18C19.1 22 20 21.1 20 20V8L14 2Z"" stroke=""#666"" stroke-width=""2"" fill=""#f0f0f0""/>
            <polyline points=""14,2 14,8 20,8"" stroke=""#666"" stroke-width=""2"" fill=""none""/>
        </svg>";
        return System.Text.Encoding.UTF8.GetBytes(svgIcon);
    }
}

public class FileUploadRequest
{
    public IFormFile File { get; set; }
    public string ChatId { get; set; }
}
