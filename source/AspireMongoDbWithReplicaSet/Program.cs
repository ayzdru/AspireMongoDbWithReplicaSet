using AspireMongoDbWithReplicaSet.Core.Entities;
using AspireMongoDbWithReplicaSet.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddMongoDBClient(connectionName: "mongoDb");

builder.Services.AddDbContext<MoviesDbContext>((serviceProvider, options) =>
{
    var mongoClient = serviceProvider.GetRequiredService<IMongoClient>();
    options.UseMongoDB(mongoClient, "mongoDb");
});

var app = builder.Build();

// CRUD minimal APIs using the Movie entity directly (no DTOs)

// GET /movies
app.MapGet("/movies", async (MoviesDbContext db) =>
{
    var movies = await db.Movies.AsNoTracking().ToListAsync();
    return Results.Ok(movies);
})
.WithName("GetMovies");

// GET /movies/{id}
app.MapGet("/movies/{id}", async (string id, MoviesDbContext db) =>
{
    if (!ObjectId.TryParse(id, out var oid))
        return Results.BadRequest(new { error = "Invalid id format." });

    var movie = await db.Movies.AsNoTracking().FirstOrDefaultAsync(m => m._id == oid);
    return movie is null ? Results.NotFound() : Results.Ok(movie);
})
.WithName("GetMovieById");

// POST /movies
app.MapPost("/movies", async (Movie movie, MoviesDbContext db) =>
{
    // Ensure an id exists for the new entity
    if (movie._id == default)
        movie._id = ObjectId.GenerateNewId();
    await using var transaction = await db.Database.BeginTransactionAsync();
    await db.Movies.AddAsync(movie);
    await db.SaveChangesAsync();
    await transaction.CommitAsync();
    var location = $"/movies/{movie._id}";
    return Results.Created(location, movie);
})
.WithName("CreateMovie");

// PUT /movies/{id}
app.MapPut("/movies/{id}", async (string id, Movie updatedMovie, MoviesDbContext db) =>
{
    if (!ObjectId.TryParse(id, out var oid))
        return Results.BadRequest(new { error = "Invalid id format." });

    var movie = await db.Movies.FirstOrDefaultAsync(m => m._id == oid);
    if (movie is null)
        return Results.NotFound();

    // Update allowed properties (do not replace _id)
    movie.Title = updatedMovie.Title;
    movie.Rated = updatedMovie.Rated;
    movie.Plot = updatedMovie.Plot;

    db.Movies.Update(movie);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("UpdateMovie");

// DELETE /movies/{id}
app.MapDelete("/movies/{id}", async (string id, MoviesDbContext db) =>
{
    if (!ObjectId.TryParse(id, out var oid))
        return Results.BadRequest(new { error = "Invalid id format." });

    var movie = await db.Movies.FirstOrDefaultAsync(m => m._id == oid);
    if (movie is null)
        return Results.NotFound();

    db.Movies.Remove(movie);
    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("DeleteMovie");

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.Run();
