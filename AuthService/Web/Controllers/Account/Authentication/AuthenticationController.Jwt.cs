using System.Security.Claims;
using AuthService.Core.Entities;
using MassTransit.Internals;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace AuthService.Web.Controllers.Account.Authentication;

public sealed partial class AuthenticationController
{
    private static ClaimsIdentity Identity = new ClaimsIdentity();
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly IOpenIddictScopeManager _scopeManager;


    [HttpPost("~/connect/token"), Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        try
        {
            var openIdConnectRequest = HttpContext.GetOpenIddictServerRequest() ??
                                       throw new InvalidOperationException(
                                           "The OpenID Connect request cannot be retrieved.");

            Identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                OpenIddictConstants.Claims.Name, OpenIddictConstants.Claims.Role);
            User? user;
            AuthenticationProperties properties = new();

            if (openIdConnectRequest.IsClientCredentialsGrantType())
            {
                throw new NotImplementedException();
            }
            else if (openIdConnectRequest.IsPasswordGrantType())
            {
                user = await _userManager.Users.IgnoreQueryFilters()
                           .Where(u => u.UserName == openIdConnectRequest.Username).FirstOrDefaultAsync();
                if (user == null)
                {
                    return BadRequest(new OpenIddictResponse
                    {
                        Error = OpenIddictConstants.Errors.InvalidGrant,
                        ErrorDescription = "User does not exist"
                    });
                }

                // Manually verify password and sign in status
                if (!await _userManager.CheckPasswordAsync(user, openIdConnectRequest.Password))
                {
                    return BadRequest(new OpenIddictResponse
                    {
                        Error = OpenIddictConstants.Errors.InvalidGrant,
                        ErrorDescription = "Username or password is incorrect"
                    });
                }

                // Check if user is allowed to sign in
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    return BadRequest(new OpenIddictResponse
                    {
                        Error = OpenIddictConstants.Errors.InvalidGrant,
                        ErrorDescription = "User not allowed to login. Please confirm your email"
                    });
                }

                // Check if user is locked out
                if (_userManager.SupportsUserLockout && await _userManager.IsLockedOutAsync(user))
                {
                    return BadRequest(new OpenIddictResponse
                    {
                        Error = OpenIddictConstants.Errors.InvalidGrant,
                        ErrorDescription = "User is locked out"
                    });
                }

                // Reset the lockout count if supported
                if (_userManager.SupportsUserLockout)
                {
                    await _userManager.ResetAccessFailedCountAsync(user);
                }

                if (_userManager.SupportsUserLockout)
                {
                    await _userManager.ResetAccessFailedCountAsync(user);
                }

                Identity.SetScopes(openIdConnectRequest.GetScopes());

                Identity.SetResources(await _scopeManager.ListResourcesAsync(Identity.GetScopes()).ToListAsync());


                Identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, user.Id));
                Identity.AddClaim(new Claim(OpenIddictConstants.Claims.Audience, "Resourse"));
                Identity.AddClaim(new Claim(OpenIddictConstants.Claims.Email, user.Email));
                Identity.AddClaim(new Claim(OpenIddictConstants.Claims.Username, user.UserName));

                Identity.SetDestinations(GetDestinations);
            }
            else if (openIdConnectRequest.IsRefreshTokenGrantType())
            {
                throw new NotImplementedException();
            }
            else
            {
                return BadRequest(new
                {
                    error = OpenIddictConstants.Errors.UnsupportedGrantType,
                    error_description = "The specified grant type is not supported."
                });
            }

            var signInResult = SignIn(new ClaimsPrincipal(Identity), properties,
                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            return signInResult;
        }
        catch (Exception ex)
        {
            return BadRequest(new OpenIddictResponse()
            {
                Error = OpenIddictConstants.Errors.ServerError,
                ErrorDescription = "Invalid login attempt"
            });
        }
    }

    #region Private Methods

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        return claim.Type switch
        {
            OpenIddictConstants.Claims.Name or
                OpenIddictConstants.Claims.Subject
                => new[]
                {
                    OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken
                },

            _ => new[] { OpenIddictConstants.Destinations.AccessToken },
        };
    }

    #endregion
}