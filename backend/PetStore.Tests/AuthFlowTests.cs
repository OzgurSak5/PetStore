using System.Net;
using System.Net.Http.Json;
using PetStore.Domain.DTOs;
using Xunit;
using System.Net.Http.Headers;

namespace PetStore.Tests;

public class AuthFlowTests : IClassFixture<PetStoreApiFactory>
{
    private readonly HttpClient _client;
    private readonly PetStoreApiFactory _factory;

    public AuthFlowTests(PetStoreApiFactory factory)
    {
        _client = factory.CreateClient();
        _factory = factory;
    }

    [Fact]
    public async Task Register_then_login_returns_a_token_pair()
    {
        var email = $"user-{Guid.NewGuid():N}@example.com";
        var password = "Password123!";
        var register = await _client.PostAsJsonAsync("/api/Auth/register", new { email, password });

        Assert.Equal(HttpStatusCode.Created, register.StatusCode);

        var login = await _client.PostAsJsonAsync("/api/Auth/login", new { email, password });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);

        var tokens = await login.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.NotNull(tokens);
        Assert.False(string.IsNullOrWhiteSpace(tokens!.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(tokens.RefreshToken));
        Assert.Equal(900,tokens.ExpiresInSeconds);
    }

    [Fact]
    public async Task A_refresh_token_cannot_be_used_twice()
    {
        var tokens = await RegisterAndLoginAsync();

        var first = await _client.PostAsJsonAsync(
            "/api/Auth/refresh", new { refreshToken = tokens.RefreshToken });

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await _client.PostAsJsonAsync(
            "/api/Auth/refresh", new { refreshToken = tokens.RefreshToken });

        Assert.Equal(HttpStatusCode.Unauthorized, second.StatusCode);
    }

    [Fact]
    public async Task Me_without_a_token_is_rejected()
    {
        var response = await _client.GetAsync("/api/Auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task A_customer_cannot_create_a_pet()
    {
        var tokens = await RegisterAndLoginAsync();
        var client = CreateClientWith(tokens.AccessToken);

        var response = await client.PostAsJsonAsync("/api/Pet", new
        {
            name = "Test",
            breedId = 5,
            birthDate = "2024-01-01",
            gender = "Male",
            price = 1000,
            isVaccinated = true
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<TokenResponse> RegisterAndLoginAsync()
    {
        var email = $"user-{Guid.NewGuid():N}@example.com";
        const string password = "Password123!";

        await _client.PostAsJsonAsync("/api/Auth/register", new {email, password});
        var login = await _client.PostAsJsonAsync("/api/Auth/login", new { email, password });

        return (await login.Content.ReadFromJsonAsync<TokenResponse>())!;
    }

    private HttpClient CreateClientWith(string accessToken)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }
}