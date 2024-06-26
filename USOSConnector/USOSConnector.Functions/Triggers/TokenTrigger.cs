using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Caching.Memory;
using USOSConnector.Functions.Constants;
using USOSConnector.Functions.Extensions;
using USOSConnector.Functions.Services.JwtService;
using USOSConnector.Functions.Services.JwtService.Dtos;
using USOSConnector.Functions.Services.UsosService;
using USOSConnector.Functions.Triggers.Dtos;

namespace USOSConnector.Functions.Triggers;

public class TokenTrigger
{
    private readonly IUsosService _usosService;
    private readonly IJwtService _jwtService;
    private readonly IMemoryCache _cache;

    public TokenTrigger(
        IUsosService usosService,
        IJwtService jwtService,
        IMemoryCache cache)
    {
        _usosService = usosService;
        _jwtService = jwtService;
        _cache = cache;
    }

    [Function(nameof(GetToken))]
    public async Task<HttpResponseData> GetToken(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "token")] 
        HttpRequestData req,
        CancellationToken cancellationToken)
    {
        var token = req.Query["oauth_token"];

        var verifier = req.Query["oauth_verifier"];

        var cacheKey = req.Query["key"];

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(verifier) || string.IsNullOrEmpty(cacheKey))
        {
            return await req.CreateProblemResponseAsync(
                HttpStatusCode.BadRequest,
                "Token, verifier and key are required.");
        }

        var secret = _cache.Get<string>(cacheKey);
        
        if (string.IsNullOrEmpty(secret))
        {
            return await req.CreateProblemResponseAsync(
                HttpStatusCode.BadRequest,
                "Invalid key.");
        }

        var accessTokenResult = await _usosService.GetAccessTokenAsync(
            token, 
            verifier, 
            secret, 
            cancellationToken);
        
        var userInfoResult = await _usosService.GetCurrentUserInfoAsync(
            accessTokenResult.Token, 
            accessTokenResult.Secret, 
            cancellationToken);

        _cache.Remove(cacheKey);
        _cache.Set(
            userInfoResult.Id, 
            accessTokenResult.Secret, 
            TimeSpan.FromMinutes(Usos.AccessTokenExpiryMinutes));

        var userClaims = new UserClaimsDto
        {
            UserId = userInfoResult.Id,
            FirstName = userInfoResult.FirstName,
            LastName = userInfoResult.LastName,
            UsosToken = accessTokenResult.Token,
        };
        var jwtToken = _jwtService.GenerateToken(userClaims);

        var okResponse = req.CreateResponse(HttpStatusCode.OK);

        await okResponse.WriteAsJsonAsync(new TokenResponseDto
        { 
            Token = jwtToken,
            FirstName = userInfoResult.FirstName,
            LastName = userInfoResult.LastName
        });

        return okResponse;
    }
}