using TfgApi.Models;

namespace TfgApi.Services;

public interface IExerciseApiService
{
    Task<List<ExerciseItemDTO>> SearchExerciseAsync(string? name = null, string? bodyParts = null, int page = 1, int pageSize = 30);
    Task<List<ExerciseItemDTO>> GetAllExercisesAsync();
    Task<ExerciseItemDTO?> GetExerciseByIdAsync(string exerciseId);
}