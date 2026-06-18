using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TfgApi.Data;
using TfgApi.Models;
using TfgApi.Services;

namespace TfgApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RoutineController : ControllerBase
{
    private readonly AppDbContext context;
    private readonly UserManager<User> userManager;
    private readonly IExerciseApiService apiService;

    public RoutineController(AppDbContext context, UserManager<User> userManager, IExerciseApiService apiService)
    {
        this.context = context;
        this.userManager = userManager;
        this.apiService = apiService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoutineResponse>>> GetAll()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var routines = await context.Routines
            .Include(r => r.RoutineDays).ThenInclude(rd => rd.DayOfWeek)
            .Include(r => r.RoutineExercises).ThenInclude(re => re.Exercise)
            .Where(r => r.UserId == user.Id)
            .ToListAsync();

        return Ok(routines.Select(MapToResponse).ToList());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoutineResponse>> GetById(int id)
    {
        var routine = await context.Routines
            .Include(r => r.RoutineDays).ThenInclude(rd => rd.DayOfWeek)
            .Include(r => r.RoutineExercises).ThenInclude(re => re.Exercise)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (routine == null) return NotFound();
        return Ok(MapToResponse(routine));
    }

    [HttpPost]
    public async Task<ActionResult<RoutineResponse>> Create(RoutineRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var routine = new Routine
        {
            Name = request.Name,
            Description = request.Description ?? "",
            UserId = user.Id,
            RoutineExercises = await BuildExercises(request.Exercises),
            RoutineDays = BuildRoutineDays(request.DayOfWeeks)
        };

        context.Routines.Add(routine);
        await context.SaveChangesAsync();

        await context.Entry(routine).Collection(r => r.RoutineDays).Query().Include(rd => rd.DayOfWeek).LoadAsync();
        foreach (var re in routine.RoutineExercises)
            await context.Entry(re).Reference(r => r.Exercise).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = routine.Id }, MapToResponse(routine));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<RoutineResponse>> Update(int id, RoutineRequest request)
    {
        var routine = await context.Routines
            .Include(r => r.RoutineDays)
            .Include(r => r.RoutineExercises)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (routine == null) return NotFound();

        routine.Name = request.Name;
        routine.Description = request.Description ?? "";

        context.RoutineDays.RemoveRange(routine.RoutineDays);
        routine.RoutineDays = BuildRoutineDays(request.DayOfWeeks);

        context.RoutineExercises.RemoveRange(routine.RoutineExercises);
        routine.RoutineExercises = await BuildExercises(request.Exercises, routine.Id);

        await context.SaveChangesAsync();

        await context.Entry(routine).Collection(r => r.RoutineDays).Query().Include(rd => rd.DayOfWeek).LoadAsync();
        foreach (var re in routine.RoutineExercises)
            await context.Entry(re).Reference(r => r.Exercise).LoadAsync();

        return Ok(MapToResponse(routine));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var routine = await context.Routines.FindAsync(id);
        if (routine == null) return NotFound();

        context.Routines.Remove(routine);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private static List<RoutineDay> BuildRoutineDays(List<int>? dayOfWeeks)
    {
        if (dayOfWeeks == null || dayOfWeeks.Count == 0) return new List<RoutineDay>();
        return dayOfWeeks.Where(d => d >= 1 && d <= 7).Distinct().Select(d => new RoutineDay
        {
            DayOfWeekId = d
        }).ToList();
    }

    private async Task<List<RoutineExercise>> BuildExercises(List<RoutineExerciseRequest> requests, int? routineId = null)
    {
        var list = new List<RoutineExercise>();
        foreach (var e in requests)
        {
            list.Add(new RoutineExercise
            {
                RoutineId = routineId ?? 0,
                ExerciseId = await ResolveExerciseId(e),
                OrderInRoutine = e.Order,
                Sets = e.Sets,
                Reps = e.Reps.ToString(),
                RestTimeSeconds = e.RestTimeSeconds,
                Notes = e.Notes
            });
        }
        return list;
    }

    private static RoutineResponse MapToResponse(Routine routine)
    {
        var dayNames = routine.RoutineDays
            .OrderBy(rd => rd.DayOfWeek.Order)
            .Select(rd => rd.DayOfWeek.Name)
            .ToList();

        var dayOrders = routine.RoutineDays
            .OrderBy(rd => rd.DayOfWeek.Order)
            .Select(rd => rd.DayOfWeek.Order)
            .ToList();

        return new RoutineResponse
        {
            Id = routine.Id,
            Name = routine.Name,
            Description = routine.Description,
            DayOfWeekName = dayNames.Count > 0 ? string.Join(", ", dayNames) : "Unassigned",
            DayOfWeekOrders = dayOrders,
            Exercises = routine.RoutineExercises.Select(re => new RoutineExerciseResponse
            {
                Id = re.Id,
                ExerciseId = re.ExerciseId,
                ExerciseName = re.Exercise?.Name ?? "",
                ExerciseGifUrl = re.Exercise?.GifUrl,
                Order = re.OrderInRoutine,
                Sets = re.Sets,
                Reps = int.TryParse(re.Reps, out var reps) ? reps : 0,
                RestTimeSeconds = re.RestTimeSeconds,
                Notes = re.Notes
            }).ToList()
        };
    }

    private async Task<int> ResolveExerciseId(RoutineExerciseRequest request)
    {
        if (request.ExerciseId.HasValue) return request.ExerciseId.Value;

        if (!string.IsNullOrEmpty(request.ExternalApiId))
        {
            var existing = await context.Exercises
                .FirstOrDefaultAsync(e => e.ExternalApiId == request.ExternalApiId);

            if (existing != null) return existing.Id;

            var apiExercise = await apiService.GetExerciseByIdAsync(request.ExternalApiId);
            if (apiExercise != null)
            {
                var exercise = new Exercise
                {
                    ExternalApiId = apiExercise.ExerciseId,
                    Name = apiExercise.Name,
                    Description = "",
                    BodyParts = apiExercise.BodyParts,
                    TargetMuscles = apiExercise.TargetMuscles,
                    SecondaryMuscles = apiExercise.SecondaryMuscles,
                    Equipments = apiExercise.Equipments,
                    GifUrl = apiExercise.GifUrl,
                    Instructions = apiExercise.Instructions
                };

                context.Exercises.Add(exercise);
                await context.SaveChangesAsync();
                return exercise.Id;
            }
        }

        throw new BadHttpRequestException("Either exerciseId or externalApiId must be provided");
    }
}
