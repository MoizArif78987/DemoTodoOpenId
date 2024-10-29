using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Collections.Immutable;
using System.Security.Claims;
using TodoAppBackend.Constants;
using TodoAppBackend.DTOs;
using static OpenIddict.Abstractions.OpenIddictConstants;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AuthController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
    {
        var user = new IdentityUser { UserName = registerDto.Email, Email = registerDto.Email };
        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (result.Succeeded)
        {
            return Ok();
        }

        return BadRequest(result.Errors);
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromForm] OpenIddictRequest request)
    {
        if (request.IsPasswordGrantType())
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return Unauthorized(new { error = ResponseMessages.INVALID_CREDENTIALS });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { error = ResponseMessages.INVALID_CREDENTIALS });
            }

            ImmutableArray<string> requestedScopes = request.GetScopes();
            return await GenerateTokenAsync(user, requestedScopes);
        }

        else if (request.IsRefreshTokenGrantType())
        {

            var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return Unauthorized(new { error = ResponseMessages.INVALID_REFRESH_TOKEN });
            }

            var userId = result.Principal.GetClaim(OpenIddictConstants.Claims.Subject);
            if (userId == null)
            {
                return Unauthorized(new { error = ResponseMessages.INVALID_ID_IN_RT });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized(new { error = ResponseMessages.USER_NOT_FOUND });
            }

            var requestedScopes = request.GetScopes();
            return await GenerateTokenAsync(user, requestedScopes);
        }
        return BadRequest(new { error = ResponseMessages.UNSUPPORTED_GRANT_TYPE });
    }


    private async Task<IActionResult> GenerateTokenAsync(IdentityUser user, ImmutableArray<string> requestedScopes)
    {
        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

        identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
                .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
                .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
                .SetClaims(Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

        identity.SetScopes(requestedScopes);
        identity.SetDestinations(GetDestinations);
        var principal = new ClaimsPrincipal(identity);

        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
    public static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case Claims.Name:
                yield return Destinations.AccessToken;
                if (claim.Subject is not null && claim.Subject.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;
                yield break;
            case Claims.Email:
                yield return Destinations.AccessToken;
                if (claim.Subject is not null && claim.Subject.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;
                yield break;
            case Claims.Role:
                yield return Destinations.AccessToken;
                if (claim.Subject is not null && claim.Subject.HasScope(Scopes.Roles))
                    yield return Destinations.IdentityToken;
                yield break;
            case StringConstants.ASP_ID_SECURITY_STAMP: yield break;
            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}