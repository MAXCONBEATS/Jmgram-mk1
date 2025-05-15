using Jmgram_mk1.src.JMgram.Core.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.HttpResults;
using Jmgram_mk1.src.JMgram.Core.Entities;
using System.ComponentModel.DataAnnotations;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Jmgram_mk1.src.JMgram.Core.Responses;
using Microsoft.EntityFrameworkCore;
using Jmgram_mk1.src.JMgram.Core.Storage;


[Authorize]
[ApiController]
[Route("[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILogger<ChatController> _logger;
    private readonly CreateChatUseCase _createChatUseCase;

    public ChatController(ILogger<ChatController> logger, CreateChatUseCase createChatUseCase)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _createChatUseCase = createChatUseCase ?? throw new ArgumentNullException(nameof(createChatUseCase));
    }

    [HttpPost("/Chat/Create")]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _createChatUseCase.Execute(request);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response.Chat);
    }
}