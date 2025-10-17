using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using WEB_353504_Bogdanovich.API.Data;
using WEB_353504_Bogdanovich.Domain.Entities;
using WEB_353504_Bogdanovich.API.UseCases;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WEB_353504_Bogdanovich.API.EndPoints;

public static class DishEndpoints
{
    public static void MapDishEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Dish").WithTags(nameof(Dish));

        // GET: /api/Dish - Получить все блюда с пагинацией
        group.MapGet("/", async ([FromServices] IMediator mediator, [FromQuery] int pageNo = 1) =>
        {
            var data = await mediator.Send(new GetListOfProducts(null, pageNo)); // Передаём pageNo из запроса
            return Results.Ok(data);
        })
        .WithName("GetAllDishesWithPagination")
        .WithOpenApi();

        // GET: /api/Dish/{id} - Получить блюдо по ID
        group.MapGet("/{id}", async Task<Results<Ok<Dish>, NotFound>> ([FromServices] AppDbContext db, int id) =>
        {
            var dish = await db.Dishes.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id);
            return dish is not null ? TypedResults.Ok(dish) : TypedResults.NotFound();
        })
        .WithName("GetDishById")
        .WithOpenApi();

        // PUT: /api/Dish/{id} - Обновить блюдо
        group.MapPut("/{id}", async Task<Results<Ok, NotFound>> ([FromServices] AppDbContext db, int id, [FromBody] Dish dish) =>
        {
            var affected = await db.Dishes
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Name, dish.Name)
                    .SetProperty(m => m.Description, dish.Description)
                    .SetProperty(m => m.Calories, dish.Calories)
                    .SetProperty(m => m.Image, dish.Image)
                    .SetProperty(m => m.CategoryId, dish.CategoryId)
                );
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateDish")
        .WithOpenApi();

        // GET: /api/Dish/{category:alpha?} - Получить блюда с пагинацией и фильтром
        group.MapGet("/{category:alpha?}", async ([FromServices] IMediator mediator, string? category, int pageNo = 1) =>
        {
            var data = await mediator.Send(new GetListOfProducts(category, pageNo));
            return Results.Ok(data);
        })
        .WithName("GetDishesWithPaginationAndFilter")
        .WithOpenApi();

        // DELETE: /api/Dish/{id} - Удалить блюдо
        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> ([FromServices] AppDbContext db, int id) =>
        {
            var affected = await db.Dishes
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteDish")
        .WithOpenApi();
    }
}