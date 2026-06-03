using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TfgApi.Data;
using TfgApi.Models;
using TfgApi.Services;

namespace TfgApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExerciseController : ControllerBase
{
    private readonly IExerciseApiService apiService;    //  Connection with the external API
    private readonly AppDbContext context;  //  Connection to my PostgreSQL

    public ExerciseController(IExerciseApiService apiService, AppDbContext context)
    {
        this.apiService = apiService;
        this.context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<Exercise>>> GetAll()
    {
        return await context.Exercises.ToListAsync();   //  ToListAsync is basically SELECT * FROM X and transform each row into objects
    }

    [HttpGet("{id}")]   //  Capturesthe number id as a parameter for the method
    public async Task<ActionResult<Exercise>> GetById(int id)
    {
        var exercise = await context.Exercises.FindAsync(id);
        if (exercise == null) return NotFound();
        return exercise;
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<ExerciseItemDTO>>> Search(
        [FromQuery] string? name,
        [FromQuery] string? muscle,
        [FromQuery] int page = 1)
    {
        var results = await apiService.SearchExerciseAsync(name, muscle, page);
        return Ok(results);
    }

    /*[HttpGet("random")]
    public async Task<ActionResult<ExerciseItemDTO>> Random()
    {
        var result = await apiService.GetRandomExerciseAsync();
        if (result == null) return NotFound("Could not fetch Random Exercise");
        return Ok(result);
    }*/
}