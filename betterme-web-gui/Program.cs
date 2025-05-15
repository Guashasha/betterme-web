using System.Text.Json;
using BetterMe.WebGui.Services; //  ⇠  make sure UsersApi namespace is imported

var builder = WebApplication.CreateBuilder(args);

// 🔌 1. Register every service first
builder.Services
    .AddRazorPages()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

builder.Services.AddHttpClient<UsersApi>(c =>
{
    c.BaseAddress = new Uri("http://localhost:6969/api/");
});

builder.Services.AddHttpClient("AuthProxy", c =>
{
    c.BaseAddress = new Uri("http://localhost:6968/");
});

// 🏗 2. Then build
var app = builder.Build();

// 🌐 3. Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.MapPost("/auth/login", async (HttpContext ctx, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("AuthProxy");
    var response = await client.PostAsJsonAsync("api/authentication/login",
        await ctx.Request.ReadFromJsonAsync<object>());

    ctx.Response.StatusCode = (int)response.StatusCode;
    await response.Content.CopyToAsync(ctx.Response.Body);
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapFallbackToPage("/Login");

app.Run();
