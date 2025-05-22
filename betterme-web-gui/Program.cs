using System.Text.Json;
using Microsoft.AspNetCore.Routing;      
using BetterMe.WebGui.Services;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// 1️⃣ Register services
builder.Services
    .AddRazorPages()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

builder.Services.AddHttpClient<UsersApi>(c =>
    c.BaseAddress = new Uri("http://localhost:6969/api/")
);

builder.Services.AddHttpClient("AuthProxy", c =>
    c.BaseAddress = new Uri("http://localhost:6968/")
);

builder.Services.AddHttpClient("UsersProxy", c =>
    c.BaseAddress = new Uri("http://localhost:6969/")
);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

//Guys this mapPost shits are like mmm contexts for the JS Scripts to like know where and wich api is calling
app.MapPost("/auth/login", async (HttpContext ctx, IHttpClientFactory factory) =>
{
    var payload = await ctx.Request.ReadFromJsonAsync<object>();
    var client  = factory.CreateClient("AuthProxy");
    var response = await client.PostAsJsonAsync("api/authentication/login", payload);
    ctx.Response.StatusCode = (int)response.StatusCode;
    await response.Content.CopyToAsync(ctx.Response.Body);
});

app.MapPost("/users", async (HttpContext ctx, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient("UsersProxy");
    var forward = new HttpRequestMessage(HttpMethod.Post, "api/users")
    {
        Content = new StreamContent(ctx.Request.Body)
    };
    forward.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
    var response = await client.SendAsync(forward, ctx.RequestAborted);
    ctx.Response.StatusCode = (int)response.StatusCode;
    await response.Content.CopyToAsync(ctx.Response.Body);
});

// IMPORTANT !!
//Still missing the logic to "restrict" users to going to pages that they are not authorized, like for they to not put on the nav search bar like https://localhost/UserInfo 

app.MapRazorPages();

app.MapFallbackToPage("/Login")
   .WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

app.Run();
