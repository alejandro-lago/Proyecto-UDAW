using System.ComponentModel.DataAnnotations;

namespace TfgApi.Models;

public class RoutineRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public int DayOfWeek { get; set; }

    public List<RoutineExerciseRequest> Exercises { get; set; } = new();
}

public class RoutineExerciseRequest
{
    [Required]
    public int ExerciseId { get; set; }
    
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
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string DayOfWeekName { get; set; } = string.Empty;
    public int DayOfWeekOrder { get; set; }
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
    public string? Notes { get; set;}
}