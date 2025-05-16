using Jmgram_mk1.src.JMgram.Core.Dtos;
using Jmgram_mk1.src.JMgram.Core.Requestes;
using Jmgram_mk1.src.JMgram.Core.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("[controller]")]
public class ContactController : ControllerBase
{
    private readonly AddContactUseCase _addContactUseCase;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContactController(AddContactUseCase addContactUseCase, IHttpContextAccessor httpContextAccessor)
    {
        _addContactUseCase = addContactUseCase ?? throw new ArgumentNullException(nameof(addContactUseCase));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    [HttpPost("Add")]
    public async Task<IActionResult> Add([FromBody] AddContactRequest request)
    {
        var response = await _addContactUseCase.Execute(request);

        if (!response.IsSuccess)
        {
            return BadRequest(response.ErrorMessage);
        }

        return Ok(response.Contact);
    }
}