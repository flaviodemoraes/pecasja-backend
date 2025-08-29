using MediatR;
using Microsoft.AspNetCore.Mvc;
using PecasJa.Backend.Api.Features.Authentication;

namespace PecasJa.Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthenticationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateUser.Command command)
    {
        var result = await _mediator.Send(command);

        if (result.Succeeded)
        {
            return Ok();
        }

        return BadRequest(result.Errors);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Login.Command command)
    {
        var result = await _mediator.Send(command);

        if (result.Succeeded)
        {
            return Ok(new { result.Token });
        }

        return Unauthorized(result.Errors);
    }
}
