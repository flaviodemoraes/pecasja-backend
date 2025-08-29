using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using PecasJa.Backend.Api.Features.Authentication;
using Xunit;

namespace PecasJa.Backend.Api.Tests;

internal class AuthenticationControllerTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public AuthenticationControllerTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var command = new CreateUser.Command("test@example.com", "Password123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/register", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var command = new CreateUser.Command("duplicate@example.com", "Password123!");
        await _client.PostAsJsonAsync("/api/authentication/register", command); // First registration

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/register", command); // Second registration

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkAndToken()
    {
        // Arrange
        var registerCommand = new CreateUser.Command("loginuser@example.com", "Password123!");
        await _client.PostAsJsonAsync("/api/authentication/register", registerCommand);

        var loginCommand = new Login.Command("loginuser@example.com", "Password123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<Login.Result>();
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var registerCommand = new CreateUser.Command("wrongpass@example.com", "Password123!");
        await _client.PostAsJsonAsync("/api/authentication/register", registerCommand);

        var loginCommand = new Login.Command("wrongpass@example.com", "WrongPassword!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/authentication/login", loginCommand);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
