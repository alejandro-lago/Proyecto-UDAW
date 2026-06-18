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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User User { get; set; } = null!;
        public virtual ICollection<RoutineDay> RoutineDays { get; set; } = new List<RoutineDay>();
        public virtual ICollection<RoutineExercise> RoutineExercises { get; set; } = new List<RoutineExercise>();
    }
}
