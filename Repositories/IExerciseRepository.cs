using TfgApi.Models;

namespace TfgApi.Repositories;

//  Repository CRUD for Exercises
public interface IExerciseRepository
{

    Task<List<Exercise>> GetAllAsync();
    Task<Exercise?> GetByIdAsync(int id);
    //  We need both internal and external to check if it's already saved an API exercise
    Task<Exercise?> GetByExternalApiIdAsync(string externalApiId);
    Task AddAsync(Exercise exercise);
    Task UpdateAsync(Exercise exercise);
    //  Checks if an exercise already exists in our Database (useful to avoid duplicate entries)
    Task<bool> ExistsByExternalApiIdAsync(string externalApiId);
}