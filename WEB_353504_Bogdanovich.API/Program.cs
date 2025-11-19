using WEB_353504_Bogdanovich.API.Data;
using Microsoft.EntityFrameworkCore;
using WEB_353504_Bogdanovich.API.EndPoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WEB_353504_Bogdanovich.API.Models;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAntiforgery();

var connString = builder.Configuration.GetConnectionString("SqLite");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connString));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddHttpContextAccessor();

var authServer = builder.Configuration.GetSection("AuthServer").Get<AuthServerData>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
{

    o.MetadataAddress = $"{authServer.Host}/realms/{authServer.Realm}/.well-known/openid-configuration";
    o.Authority = $"{authServer.Host}/realms/{authServer.Realm}";

    o.Audience = "account";
    o.RequireHttpsMetadata = false;
});

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("admin", p => p.RequireRole("POWER-USER"));
});


var app = builder.Build();
await DbInitializer.SeedData(app);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapControllers();


app.MapDishEndpoints();


app.Run();