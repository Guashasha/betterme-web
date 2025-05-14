namespace BetterMe.WebGui.Services;

using System.Net.Http.Json;
using BetterMe.WebGui.Dtos;

public class UsersApi
{
    private readonly HttpClient _http;
    public UsersApi(HttpClient http) => _http = http;

    public Task<UserDto?> GetByIdAsync(string id, CancellationToken ct = default) =>
        _http.GetFromJsonAsync<UserDto>($"users/{id}", ct);
}