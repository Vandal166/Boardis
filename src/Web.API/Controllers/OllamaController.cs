using Application.Abstractions.CQRS;
using Application.Contracts.User;
using Application.DTOs.Ollama;
using Application.Features.Ollama.Commands;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.API.Common;

namespace Web.API.Controllers;

[Route("/api/ollama")]
[ApiController]
[Authorize]
public sealed class OllamaController : ControllerBase
{
    private readonly ICommandHandler<GenerateListStructureCommand, ListStructureResponse> _generateListStructureHandler;
    private readonly ICurrentUser _currentUser;
    
    public OllamaController(ICommandHandler<GenerateListStructureCommand, ListStructureResponse> generateListStructureHandler, ICurrentUser currentUser)
    {
        _generateListStructureHandler = generateListStructureHandler;
        _currentUser = currentUser;
    }
    
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateResponse([FromBody] GenerateListStructureRequest request, CancellationToken ct = default)
    {
        var command = new GenerateListStructureCommand
        {
            Description = request.Description,
            RequestingUserId = _currentUser.Id
        };
        
        var response = await _generateListStructureHandler.Handle(command, ct);
        if (response.IsFailed)
            return response.ToProblemResponse(this);
        
        return Ok(response.Value);
    }
}