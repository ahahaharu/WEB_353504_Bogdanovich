using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.OpenApi;
using WEB_353504_Bogdanovich.API.Data;
using WEB_353504_Bogdanovich.Domain.Entities;
using WEB_353504_Bogdanovich.API.UseCases;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing; 

namespace WEB_353504_Bogdanovich.API.EndPoints;

public static class DishEndpoints
{
    public static void MapDishEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/Dish")
            .WithTags(nameof(Dish))
            .DisableAntiforgery() 
            .RequireAuthorization("admin");


        group.MapGet("/{id}", async Task<Results<Ok<Dish>, NotFound>> ([FromServices] AppDbContext db, int id) =>
        {
            var dish = await db.Dishes.AsNoTracking()
                .FirstOrDefaultAsync(model => model.Id == id);
            return dish is not null ? TypedResults.Ok(dish) : TypedResults.NotFound();
        })
        .AllowAnonymous() 
        .WithName("GetDishById")
        .WithOpenApi();

        group.MapPut("/{id}", async Task<Results<Ok, NotFound, BadRequest<string>>> (
            [FromServices] AppDbContext db,
            int id,
            [FromForm] string dish,
            [FromForm] IFormFile? file,
            [FromServices] IMediator mediator) =>
        {
            var updatedDish = System.Text.Json.JsonSerializer.Deserialize<Dish>(dish);
            if (updatedDish == null)
            {
                return TypedResults.BadRequest("Не удалось десериализовать объект блюда.");
            }

            if (file != null && file.Length > 0)
            {
                var imageUrl = await mediator.Send(new SaveImage(file));
                updatedDish.Image = imageUrl;
            }

            var affected = await db.Dishes
                .Where(model => model.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(m => m.Name, updatedDish.Name)
                    .SetProperty(m => m.Description, updatedDish.Description)
                    .SetProperty(m => m.Calories, updatedDish.Calories)
                    .SetProperty(m => m.Image, updatedDish.Image)
                    .SetProperty(m => m.CategoryId, updatedDish.CategoryId)
                );

            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("UpdateDish")
        .WithOpenApi();

        group.MapGet("/{category:alpha?}", async ([FromServices] IMediator mediator, string? category, int pageNo = 1) =>
        {
            var data = await mediator.Send(new GetListOfProducts(category, pageNo));
            return Results.Ok(data);
        })
        .AllowAnonymous() 
        .WithName("GetDishesWithPaginationAndFilter")
        .WithOpenApi();

        group.MapDelete("/{id}", async Task<Results<Ok, NotFound>> ([FromServices] AppDbContext db, int id) =>
        {
            var affected = await db.Dishes
                .Where(model => model.Id == id)
                .ExecuteDeleteAsync();
            return affected == 1 ? TypedResults.Ok() : TypedResults.NotFound();
        })
        .WithName("DeleteDish")
        .WithOpenApi();

        group.MapPost("/", async Task<Results<Created<Dish>, BadRequest<string>>> (
            [FromForm] string dish,
            [FromForm] IFormFile? file,
            [FromServices] AppDbContext db,
            [FromServices] IMediator mediator) =>
        {
            var newDish = System.Text.Json.JsonSerializer.Deserialize<Dish>(dish);
            if (newDish == null)
            {
                return TypedResults.BadRequest("Не удалось десериализовать объект блюда.");
            }

            var imageUrl = await mediator.Send(new SaveImage(file));
            newDish.Image = imageUrl;

            db.Dishes.Add(newDish);
            await db.SaveChangesAsync();

            return TypedResults.Created($"/api/Dish/{newDish.Id}", newDish);
        })
        .WithName("CreateDish")
        .WithOpenApi();

    
    }

}
