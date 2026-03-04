using AuthService.Core.Entities;
using AuthService.Core.Interfaces;
using AuthService.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;

namespace AuthService.Web.Controllers.Account.Authentication;

[ApiController]
[Route("account")]
public sealed partial class AuthenticationController: ControllerBase
{
    private readonly SignInManager<User> _signInManager;
    private readonly ITokenService _tokenService;
    private readonly UserManager<User> _userManager;
    private readonly IUserService _userService;
    private readonly ILogger<AuthenticationController> _logger;
    private readonly IOpenIddictApplicationManager _applicationManager;


    public AuthenticationController(
        SignInManager<User> signInManager,
        ITokenService tokenService,
        UserManager<User> userManager,
        ILogger<AuthenticationController> logger,
        IUserService userService,
        IOpenIddictApplicationManager applicationManager, IOpenIddictScopeManager scopeManager, IOpenIddictAuthorizationManager authorizationManager)
    {
        _signInManager = signInManager;
        _tokenService = tokenService;
        _userManager = userManager;
        _logger = logger;
        _userService = userService;
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _scopeManager = scopeManager;
    }
    
    // deprecated игрался с oidc через гугл
    // [HttpGet("google/login")]
    // [AllowAnonymous]
    // public IActionResult LoginGoogleOidc(string returnUrl = "/")
    // {
    //     if (User.Identity?.IsAuthenticated == true)
    //     {
    //         return LocalRedirect(returnUrl);
    //     }
    //
    //     var properties = new AuthenticationProperties
    //     {
    //         RedirectUri = Url.Action(nameof(AuthenticateGoogleOidc), "Authentication", new { returnUrl }, "https"),
    //         IsPersistent = true,
    //         AllowRefresh = true,
    //         ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
    //     };
    //
    //     return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    // }
    //     
    // [HttpGet("google/logout")]
    // [Authorize]
    // public IActionResult LogoutGoogleOidc(string returnUrl = "/")
    // {
    //     return SignOut(
    //         new AuthenticationProperties { RedirectUri = returnUrl },
    //         IdentityConstants.ApplicationScheme, 
    //         IdentityConstants.ApplicationScheme);
    // }
    //
    // [AllowAnonymous, HttpGet("google/access-denied")]
    // public IActionResult AccessDenied()
    // {
    //     return Unauthorized(new { Message = "Access denied" });
    // }
    //
    // [AllowAnonymous, HttpGet("google/authenticate")]
    // public async Task<IActionResult> AuthenticateGoogleOidc(string returnUrl = "/")
    // {
    //     var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
    //     if (!result.Succeeded)
    //     {
    //         return RedirectToAction(nameof(AccessDenied));
    //     }
    //
    //     var claimsIdentity = result.Principal.Identities.FirstOrDefault();
    //     if (claimsIdentity == null)
    //     {
    //         return RedirectToAction(nameof(AccessDenied));
    //     }
    //     
    //     var email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
    //     var username = claimsIdentity.FindFirst(ClaimTypes.Name)?.Value;
    //     if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(username))
    //         return RedirectToAction(nameof(AccessDenied));
    //
    //     var user = await _userService.FindOrCreateForGoogleAuth(email, username);
    //     if (user == null || !user.CanLogin())
    //         return RedirectToAction(nameof(AccessDenied));
    //     
    //     var roles = await _userManager.GetRolesAsync(user);
    //     if (roles.Count > 0)
    //     {
    //         foreach (var role in roles)
    //         {
    //             claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
    //         }
    //     }
    //     
    //     await _signInManager.SignInAsync(user, isPersistent: true);
    //     
    //     return LocalRedirect(returnUrl);
    // }
}