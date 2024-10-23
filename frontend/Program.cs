using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

string[] requiredVariables = { 
    "ipbackend_api",
    "ipbackend_auth", 
    "googleClientId", 
    "googleSecretId", 
    "facebookClientId", 
    "facebookSecretId" 
};

foreach (string variable in requiredVariables)
{
    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(variable)))
    {
        throw new Exception($"La variabile d'ambiente '{variable}' non è impostata.");
    }
}


var builder = WebApplication.CreateBuilder(args);

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
