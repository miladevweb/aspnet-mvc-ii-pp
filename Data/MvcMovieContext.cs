using MvcPractice.Models;
using Microsoft.EntityFrameworkCore;

namespace MvcPractice.Data;

public class MvcMovieContext : DbContext
{
    public MvcMovieContext(DbContextOptions<MvcMovieContext> options) : base(options)
    {}

    public DbSet<Movie> Movie { get; set;}

    // This method is used to seed the database with initial data.
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movie>().HasData(
            new Movie()
            {
                Id = 1,
                Title = "The Shawshank Redemption",
                ReleaseDate = new DateTime(1994, 9, 10),
                Genre = "Drama",
                Price = 9.99M,
                Rating = "R"
            },
            new Movie()
            {
                Id = 2,
                Title = "The Godfather",
                ReleaseDate = new DateTime(1972, 3, 24),
                Genre = "Crime, Drama",
                Price = 14.99M,
                Rating = "S"
            }
        );
    }
}
