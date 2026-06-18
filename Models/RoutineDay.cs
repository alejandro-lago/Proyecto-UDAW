namespace TfgApi.Models;

public class RoutineDay
{
    public int Id { get; set; }
    public int RoutineId { get; set; }
    public int DayOfWeekId { get; set; }

    public Routine Routine { get; set; } = null!;
    public DayOfWeek DayOfWeek { get; set; } = null!;
}
