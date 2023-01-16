using API.Data;
using API.Entities;
using API.Extensions;
using API.middleware;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Add out method Extension
builder.Services.AddApplicationServices(builder.Configuration);
// Add Identity Extension
builder.Services.AddIdentityServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();
// Configure the HTTP request pipeline.

app.MapControllers();

using var scope= app.Services.CreateScope();
var services = scope.ServiceProvider;

try{
    var context = services.GetRequiredService<DataContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    await context.Database.MigrateAsync();
    await Seed.SeedUsers(userManager);
}catch(Exception ex){
  var logger = services.GetService<ILogger<Program>>();
  logger.LogError(ex,"An error occurred while migrating");
}

app.Run();
