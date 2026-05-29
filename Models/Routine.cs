using System;
using System.Collections.Generic;

namespace TfgApi.Models
{
    public class Routine
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int DayOfWeekId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual DayOfWeek DayOfWeek { get; set; } = null!;
        public virtual ICollection<RoutineExercise> RoutineExercises { get; set; } = new List<RoutineExercise>();
    }
}