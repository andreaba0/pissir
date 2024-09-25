using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Controllo variabili d'ambiente
string[] requiredVariables = { 
    "listener_uri", 
    "redirectUriFB", 
    "redirectUriGO", 
    "ipbackend", 
    "googleClientId", 
    "googleSecretId", 
    "facebookClientId", 
    "facebookSecretId" 
};

foreach (string variable in requiredVariables)
{
    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(variable)))
    {
        Console.WriteLine($"Missing environment variable: {variable}");
        throw new Exception($"Missing environment variable: {variable}");
    }
}

var listenerUri = Environment.GetEnvironmentVariable("listener_uri");

var builder = WebApplication.CreateBuilder(args: new string[] { "--urls", listenerUri });

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseAuthentication();

app.MapRazorPages();

app.Run();
