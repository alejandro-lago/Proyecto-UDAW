using System.ComponentModel.DataAnnotations;

namespace TfgApi.Models;

public class RoutineRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public List<int>? DayOfWeeks { get; set; }

    public List<RoutineExerciseRequest> Exercises { get; set; } = new();
}

public class RoutineExerciseRequest
{
    public int? ExerciseId { get; set; }
    public string? ExternalApiId { get; set; }

    public int Order { get; set; }
    public int Sets { get; set; } = 3;
    public int Reps { get; set; } = 10;
    public int RestTimeSeconds { get; set; } = 60;
    public string? Notes { get; set; }
}

public class RoutineResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DayOfWeekName { get; set; } = string.Empty;
    public List<int> DayOfWeekOrders { get; set; } = new();
    public List<RoutineExerciseResponse> Exercises { get; set; } = new();
}

public class RoutineExerciseResponse
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public string ExerciseName { get; set; } = string.Empty;
    public string? ExerciseGifUrl { get; set; }
    public int Order { get; set; }
    public int Sets { get; set; }
    public int Reps { get; set; }
    public int RestTimeSeconds { get; set; }
    public string? Notes { get; set; }
}
