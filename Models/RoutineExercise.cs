using System;

namespace TfgApi.Models
{
    public class RoutineExercise
    {
        public int Id { get; set; }
        public int RoutineId { get; set; }
        public int ExerciseId { get; set; }
        public int OrderInRoutine { get; set; }
        public int Sets { get; set; } = 1;
        public string Reps { get; set; } = "10";
        public int RestTimeSeconds { get; set; } = 60;
        public string? Notes { get; set; }
        
        // Navigation properties
        public virtual Routine Routine { get; set; } = null!;
        public virtual Exercise Exercise { get; set; } = null!;
    }
}