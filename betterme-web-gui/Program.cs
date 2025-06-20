using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;      
using BetterMe.WebGui.Services;
using System.Net.Http.Headers;
using betterme_web_gui.Services;
using System.ComponentModel;
using static MultimediaService.MultimediaService;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRazorPages()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);
builder.Services.AddControllers();

// We need to add a Session for saving the token and accees it on Controllers
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".Betterme.Session";
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


//PON LOS URL EN LOS DOS ARCHIVOS DE APPSETTINGS.JSON
builder.Services.AddScoped<VerificationRequestsService>();

builder.Services.AddHttpClient<UsersApi>(c =>
    c.BaseAddress = new Uri("http://localhost:6969/api/")
);

builder.Services.AddHttpClient("AuthProxy", c =>
    c.BaseAddress = new Uri("http://localhost:6968/")
);

builder.Services.AddHttpClient("UsersProxy", c =>
    c.BaseAddress = new Uri("http://localhost:6969/")
);

builder.Services.AddHttpClient("VerificationRequestsAPI", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["API:VerificationRequestsUrl"]);
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddGrpcClient<MultimediaServiceClient>(o =>
{
  o.Address = new Uri("http://localhost:6979");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}


//Tenemos que hacer controladores, porque si ponemos todo aquí se harán más de 20 endpoints
//Guys this mapPost shits are like mmm contexts for the JS Scripts to like know where and wich api is calling
app.MapPost("/auth/login", async (HttpContext ctx, IHttpClientFactory factory) =>
{
    var payload = await ctx.Request.ReadFromJsonAsync<object>();
    var client  = factory.CreateClient("AuthProxy");
    var response = await client.PostAsJsonAsync("api/authentication/login", payload);
    ctx.Response.StatusCode = (int)response.StatusCode;
    
    //Save the token in the session
    string responseBody = await response.Content.ReadAsStringAsync();
    string token = responseBody.Split('\"')[3];
    ctx.Session.SetString("token",token);

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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.UseSession();
app.UseAuthorization();
app.MapControllers();

app.MapFallbackToPage("/Login")
   .WithMetadata(new HttpMethodMetadata(new[] { "GET" }));

app.Run();
