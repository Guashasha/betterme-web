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

// 🏗 2. Then build
var app = builder.Build();

// 🌐 3. Middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.MapFallbackToPage("/Login");

app.Run();
