using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using AuthService.Core.Entities;
using AuthService.Core.Interfaces.Repositories;
using General.Auth;

namespace AuthService.Web.Middlewares;

public sealed class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    //todo убрать async
    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        if (context.User.Identity is { IsAuthenticated: false })
        {
            await _next(context);
            return;
        }

        if (context.User.IsInRole(Roles.RoleAdmin))
        {
            await _next(context);
            return;
        }

        var claimsIdentity = context.User.Identities.FirstOrDefault();
        if (claimsIdentity == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Требуется авторизация.");
            return;
        }

        var email = claimsIdentity.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Требуется авторизация.");
            return;
        }
        
        var user = await userRepository.GetByEmailAsync(email);
        if (user == null)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsync("Пользователь не найден");
            return;
        }

        if (!user.CanLogin())
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Пользователь был заблокирован или удален");
            return;
        }

        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            var authorizeAttributes =
                endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>() ?? Array.Empty<IAuthorizeData>();

            if (authorizeAttributes.Any())
                foreach (var authorizeData in authorizeAttributes)
                    if (authorizeData is AuthorizeAttribute && authorizeData.Policy == "DefaultWithNotConfirmedMail")
                    {
                        await _next(context);
                        return;
                    }
        }

        if (!user.EmailConfirmed)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Почта не подтверждена");
            return;
        }

        await _next(context);
    }
}