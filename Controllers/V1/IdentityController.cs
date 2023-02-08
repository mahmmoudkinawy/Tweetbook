using Microsoft.AspNetCore.Mvc;
using Tweetbook.Contracts.V1;
using Tweetbook.Contracts.V1.Requests;
using Tweetbook.Contracts.V1.Responses;
using Tweetbook.Services;

namespace Tweetbook.Controllers.V1;

[ApiController]
public class IdentityController : ControllerBase
{
    private readonly IIdentityService _identityService;

    public IdentityController(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    [HttpPost(ApiRoutes.Identity.Register)]
    public async Task<IActionResult> Register([FromBody] UserRegisterationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthFailedResponse
            {
                Errors = ModelState.Values
                    .SelectMany(a => a.Errors.Select(aa => aa.ErrorMessage))
            }) ;
        }

        var authResponse = await _identityService.RegisterAsync(request.Email, request.Password); ;

        if (!authResponse.Success)
        {
            return BadRequest(new AuthFailedResponse
            {
                Errors = authResponse.Errors
            });
        }

        return Ok(new AuthSuccessResponse
        {
            Token = authResponse.Token
        });
    }

    [HttpPost(ApiRoutes.Identity.Login)]
    public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
    {
        var authResponse = await _identityService.LoginAsync(request.Email, request.Password); ;

        if (!authResponse.Success)
        {
            return BadRequest(new AuthFailedResponse
            {
                Errors = authResponse.Errors
            });
        }

        return Ok(new AuthSuccessResponse
        {
            Token = authResponse.Token
        });
    }

}
