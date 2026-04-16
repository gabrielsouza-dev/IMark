using IMark.Shared.Interfaces;
using Microsoft.AspNetCore.Components;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;

namespace IMark.Shared.Services;

public class AuthTokenHandler : DelegatingHandler
{
    private readonly ITokenStorage _tokenStorage;
    private readonly JwtAuthStateProvider _jwtAuthStateProvider;
    private readonly NavigationManager _navigationManager;

    public AuthTokenHandler(ITokenStorage tokenStorage, JwtAuthStateProvider jwtAuthStateProvider, NavigationManager navigationManager)
    {
        _tokenStorage = tokenStorage;
        _jwtAuthStateProvider = jwtAuthStateProvider;
        _navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var publicPaths = new[] { "/api/auth/login" };
        var isPublic = publicPaths.Any(p => request.RequestUri?.PathAndQuery.StartsWith(p, StringComparison.OrdinalIgnoreCase) == true);

        if (isPublic)
            return await base.SendAsync(request, ct);

        var token = await _tokenStorage.GetTokenAsync();
        if (!string.IsNullOrEmpty(token) && !IsTokenExpired(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        else
        {
            await _jwtAuthStateProvider.NotifyLogoutAsync();
            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        }

        var response = await base.SendAsync(request, ct);

        return response;
    }

    private static bool IsTokenExpired(string token)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return jwt.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }
}