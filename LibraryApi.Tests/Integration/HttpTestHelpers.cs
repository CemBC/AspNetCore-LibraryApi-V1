using LibraryApi.DTOs.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace LibraryApi.Tests.Integration;

internal static class HttpTestHelpers
{
    public static async Task<AuthResponse> LoginAsync(
        this HttpClient client,
        string email,
        string password = "Secret123!")
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/Auth/login",
            new LoginRequest
            {
                Email = email,
                Password = password
            });

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    public static void UseBearerToken(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
