using Microsoft.EntityFrameworkCore;
using TfgApi.Data;
using TfgApi.Models;

namespace TfgApi.Repositories;

public class ExerciseRepository : IExerciseRepository
{
    //  AppDbContext is my connection to PostgreSQL
    private readonly AppDbContext context;

    //  As before, I inject the context by ASP.NET so EF Core manages the connection automatically
    public ExerciseRepository(AppDbContext context)
    {
        this.context = context;
    }

    public async Task AddAsync(Exercise exercise)
    {
        await context.Exercises.AddAsync(exercise);
        await context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByExternalApiIdAsync(string externalApiId)
    {
        //  It returns true if there is at least one row that matches. This way is fastest than getting all rows and counting them
        return await context.Exercises.AnyAsync(e => e.ExternalApiId == externalApiId);
    }

    public async Task<List<Exercise>> GetAllAsync()
    {
        return await context.Exercises.ToListAsync();
    }

    public async Task<Exercise?> GetByExternalApiIdAsync(string externalApiId)
    {
        return await context.Exercises.FirstOrDefaultAsync(e => e.ExternalApiId == externalApiId);
    }

    public async Task<Exercise?> GetByIdAsync(int id)
    {
        return await context.Exercises.FindAsync(id);
    }

    public async Task UpdateAsync(Exercise exercise)
    {
        await context.Exercises.AddAsync(exercise);
        await context.SaveChangesAsync();
    }
}