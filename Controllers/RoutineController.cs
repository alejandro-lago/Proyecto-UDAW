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
[Authorize] // Means the user has to be logged in, if not, returns HTTP 401
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
            .Include(r => r.DayOfWeek)          //  JOIN with DayOfWeek table
            .Include(r => r.RoutineExercises)   //  JOIN with RoutineExercise table
            .ThenInclude(re => re.Exercise)     //  Then JOIN RoutineExercises to Exercise
            .Where(r => r.UserId == user.Id)    //  WHERE clause, only this user's routines
            .ToListAsync();                     //  And execute the query
        
        var response = routines.Select(MapToResponse).ToList(); // Convert each Routine entity to a RoutineResponseDTO
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RoutineResponse>> GetById(int id)
    {
        var routine = await context.Routines
            .Include(r => r.DayOfWeek)
            .Include(r => r.RoutineExercises)
            .ThenInclude(re => re.Exercise)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (routine == null) return NotFound();
        return Ok(MapToResponse(routine));
    }

    [HttpPost]
    public async Task<ActionResult<RoutineResponse>> Create(RoutineRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var routineExercises = new List<RoutineExercise>();
        foreach (var e in request.Exercises)
        {
            routineExercises.Add(new RoutineExercise
            {
                ExerciseId = await ResolveExerciseId(e),
                OrderInRoutine = e.Order,
                Sets = e.Sets,
                Reps = e.Reps.ToString(),
                RestTimeSeconds = e.RestTimeSeconds,
                Notes = e.Notes
            });
        }

        var routine = new Routine
        {
            Name = request.Name,
            Description = request.Description ?? "",
            UserId = user.Id,
            DayOfWeekId = request.DayOfWeek,
            RoutineExercises = routineExercises
        };

        context.Routines.Add(routine);      //  Prepare the new routine to be inserted
        await context.SaveChangesAsync();   //  And insert it into the DB

        //  Load data to complete the answer
        //  Entry() gives me acces to EF Core's tracking info
        //  Reference() loads a single navigation property
        //  Collection() loads a list of navigation properties
        await context.Entry(routine).Reference(r => r.DayOfWeek).LoadAsync();
        await context.Entry(routine).Collection(r => r.RoutineExercises).LoadAsync();
        foreach(var re in routine.RoutineExercises) await context.Entry(re).Reference(r => r.Exercise).LoadAsync();

        //  CreatedAtAction it's a HTTP 201(Created) with a location header
        return CreatedAtAction(nameof(GetById), new { id = routine.Id }, MapToResponse(routine));
    }

    [HttpPut("id")]
    public async Task<ActionResult<RoutineResponse>> Update(int id, RoutineRequest request)
    {
        var routine = await context.Routines
            .Include(r => r.RoutineExercises)
            .FirstOrDefaultAsync(r => r.Id == id);

        if(routine == null) return NotFound();

        routine.Name = request.Name;
        routine.Description = request.Description ?? "";
        routine.DayOfWeekId = request.DayOfWeek;

        context.RoutineExercises.RemoveRange(routine.RoutineExercises);

        var routineExercises = new List<RoutineExercise>();
        foreach(var e in request.Exercises)
        {
           routineExercises.Add(new RoutineExercise
            {
                RoutineId = routine.Id,
                ExerciseId = await ResolveExerciseId(e),
                OrderInRoutine = e.Order,
                Sets = e.Sets,
                Reps = e.Reps.ToString(),
                RestTimeSeconds = e.RestTimeSeconds,
                Notes = e.Notes
            });
        }
        routine.RoutineExercises = routineExercises;

        await context.SaveChangesAsync();

        await context.Entry(routine).Reference(r => r.DayOfWeek).LoadAsync();
        foreach(var re in routine.RoutineExercises) await context.Entry(re).Reference(r => r.Exercise).LoadAsync();

        return Ok(MapToResponse(routine));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var routine = await context.Routines.FindAsync(id);
        if(routine == null) return NotFound();

        context.Routines.Remove(routine);
        await context.SaveChangesAsync();

        return NoContent();
    }

    // This method will convert Routines entities into a RoutineResponseDTO which will be sent into the front end
    private static RoutineResponse MapToResponse(Routine routine)
    {
        return new RoutineResponse
        {
          Id = routine.Id,
          Name = routine.Name,
          Description = routine.Description,
          CreatedAt = routine.CreatedAt,
          UserId = routine.UserId,
          DayOfWeekName = routine.DayOfWeek?.Name ?? "",
          DayOfWeekOrder = routine.DayOfWeek?.Order ?? 0,
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
        //  If the exercise exists, use it directly
        if(request.ExerciseId.HasValue) return request.ExerciseId.Value;

        if(!string.IsNullOrEmpty(request.ExternalApiId))
        {
            var existing = await context.Exercises
                .FirstOrDefaultAsync(e => e.ExternalApiId == request.ExternalApiId);

            if(existing != null) return existing.Id;

            var apiExercise = await apiService.GetExerciseByIdAsync(request.ExternalApiId);
            if(apiExercise != null)
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