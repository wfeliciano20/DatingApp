using System.Text;
using API.Data;
using API.Extensions;
using API.interfaces;
using API.middleware;
using API.services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

app.Run();
