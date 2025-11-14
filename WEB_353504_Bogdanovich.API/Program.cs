using WEB_353504_Bogdanovich.API.Data;
using Microsoft.EntityFrameworkCore;
using WEB_353504_Bogdanovich.API.EndPoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// !!! ьюц 1: днаюбкемхе яепбхяю ANTI-FORGERY !!!
builder.Services.AddAntiforgery();

var connString = builder.Configuration.GetConnectionString("SqLite");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connString));

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddHttpContextAccessor();

var app = builder.Build();
await DbInitializer.SeedData(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthorization();

// !!! ьюц 2: днаюбкемхе MIDDLEWARE ANTI-FORGERY !!!
// дНКФЕМ ХДРХ ОНЯКЕ UseRouting/UseAuthorization
app.UseAntiforgery();

app.MapControllers();

app.MapDishEndpoints();

app.Run();