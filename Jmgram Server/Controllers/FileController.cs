using Jmgram_mk1.src.JMgram.Core.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Security.Claims;

[ApiController]
[Route("[controller]")]
[Authorize]
public class FileController : ControllerBase
{
    private readonly IUploadFileUseCase _uploadFileUseCase;
    private readonly ILogger<FileController> _logger;

    public FileController(IUploadFileUseCase uploadFileUseCase, ILogger<FileController> logger)
    {
        _uploadFileUseCase = uploadFileUseCase;
        _logger = logger;
    }

    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request)
    {
        try
        {
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

    [HttpGet("download/{fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        try
        {
            var filePath = Path.Combine(@"C:\Users\maxco\source\repos\Jmgram mk1\Jmgram Server\Uploaded Files", fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            var contentType = GetContentType(fileName);

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downloading file: {fileName}");
            return StatusCode(500);
        }
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
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            _ => "application/octet-stream"
        };
    }
}

// Создайте отдельный класс для запроса
public class FileUploadRequest
{
    public IFormFile File { get; set; }
    public string ChatId { get; set; }
}