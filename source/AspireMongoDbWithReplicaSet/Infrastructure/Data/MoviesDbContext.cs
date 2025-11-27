using AspireMongoDbWithReplicaSet.Core.Entities;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace AspireMongoDbWithReplicaSet.Infrastructure.Data
{
    public class MoviesDbContext : DbContext
    {
        public DbSet<Movie> Movies { get; init; }
        public MoviesDbContext(DbContextOptions options)
        : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Movie>().ToCollection("movies");
        }
    }
}
