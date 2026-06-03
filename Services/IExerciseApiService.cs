using TfgApi.Models;

namespace TfgApi.Services;

public interface IExerciseApiService
{
    Task<List<ExerciseItemDTO>> SearchExerciseAsync(string? name = null, string? muscle = null, int page = 1, int pageSize = 30);
    Task<ExerciseItemDTO?> GetExerciseByIdAsync(string exerciseId);
    //Task<ExerciseItemDTO?> GetRandomExerciseAsync();
}