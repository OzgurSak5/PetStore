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

    [Fact]
    public async Task An_admin_can_create_a_pet()
    {
        var tokens = await LoginAsAdminAsync();
        var client = CreateClientWith(tokens.AccessToken);

        var response = await client.PostAsJsonAsync("/api/Pet", new
        {
            name = "Zeytin",
            breedId = 1,
            birthDate = "2024-06-01",
            gender = "Female",
            price = 5000,
            isVaccinated = true
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task A_customer_cannot_read_someone_elses_order()
    {
        var owner = await RegisterAndLoginAsync();
        var ownerClient = CreateClientWith(owner.AccessToken);

        var created = await ownerClient.PostAsJsonAsync(
            "/api/Order", new { petIds = new[] { 1 } });

        Assert.Equal(HttpStatusCode.Created, created.StatusCode);

        var order = await created.Content.ReadFromJsonAsync<OrderResponse>();

        var stranger = await RegisterAndLoginAsync();
        var strangerClient = CreateClientWith(stranger.AccessToken);

        var response = await strangerClient.GetAsync($"/api/Order/{order!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Repeated_failed_logins_lock_the_account()
    {
        var email = $"user-{Guid.NewGuid():N}@example.com";
        const string password = "Password123!";

        await _client.PostAsJsonAsync("/api/Auth/register", new { email, password });

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var failed = await _client.PostAsJsonAsync(
                "/api/Auth/login", new { email, password = "WrongPassword1" });

            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        }

        var blocked = await _client.PostAsJsonAsync(
            "/api/Auth/login", new { email, password });

        Assert.Equal(HttpStatusCode.TooManyRequests, blocked.StatusCode);
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

    private async Task<TokenResponse> LoginAsAdminAsync()
    {
        var login = await _client.PostAsJsonAsync("/api/Auth/login",
            new { email = "admin@petstore.local", password = "Admin123!" });

        return (await login.Content.ReadFromJsonAsync<TokenResponse>())!;    
    }
}