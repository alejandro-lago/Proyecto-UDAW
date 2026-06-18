using System.Text.Json;
using TfgApi.Models;

namespace TfgApi.Services;

public class ExerciseApiService : IExerciseApiService
{
    private readonly HttpClient httpClient;
    private readonly string contentRootPath;
    private static List<ExerciseItemDTO>? cachedExercises;
    private static List<ExerciseItemDTO>? localExercises;
    private static readonly SemaphoreSlim fetchLock = new(1, 1);
    private static bool triedExternal;

    private static readonly string[] AllBodyParts = ["chest", "back", "shoulders", "upper arms", "upper legs", "lower legs", "waist", "cardio"];

    private static readonly Dictionary<string, List<string>> MuscleToBodyParts = new()
    {
        ["abdominals"] = ["waist"],
        ["obliques"] = ["waist"],
        ["biceps"] = ["upper arms"],
        ["triceps"] = ["upper arms"],
        ["forearms"] = ["lower arms"],
        ["chest"] = ["chest"],
        ["pectorals"] = ["chest"],
        ["middle back"] = ["back"],
        ["lower back"] = ["back"],
        ["lats"] = ["back"],
        ["traps"] = ["shoulders", "back"],
        ["shoulders"] = ["shoulders"],
        ["quadriceps"] = ["upper legs"],
        ["hamstrings"] = ["upper legs"],
        ["glutes"] = ["upper legs"],
        ["adductors"] = ["upper legs"],
        ["abductors"] = ["upper legs"],
        ["calves"] = ["lower legs"],
        ["neck"] = ["neck"],
    };

    public ExerciseApiService(HttpClient httpClient, IHostEnvironment env)
    {
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = new Uri("https://oss.exercisedb.dev/api/v1/");
        this.contentRootPath = env.ContentRootPath;
    }

    public async Task<List<ExerciseItemDTO>> SearchExerciseAsync(string? name = null, string? bodyParts = null, int page = 1, int pageSize = 30)
    {
        if (!triedExternal) return await SearchExternalAsync(name, bodyParts, page, pageSize);

        var source = await GetAllExercisesAsync();
        var result = source.AsEnumerable();

        if (!string.IsNullOrEmpty(name))
            result = result.Where(e => e.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(bodyParts))
        {
            var parts = bodyParts.Split(',').Select(p => p.Trim().ToLower()).ToList();
            result = result.Where(e => e.BodyParts.Any(bp => parts.Contains(bp.ToLower())));
        }

        return result.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }

    private async Task<List<ExerciseItemDTO>> SearchExternalAsync(string? name, string? bodyParts, int page, int pageSize)
    {
        try
        {
            var queryParts = new List<string>();
            if (!string.IsNullOrEmpty(name)) queryParts.Add($"name={Uri.EscapeDataString(name)}");
            if (!string.IsNullOrEmpty(bodyParts)) queryParts.Add($"bodyParts={Uri.EscapeDataString(bodyParts)}");
            queryParts.Add($"page={page}");
            queryParts.Add($"pageSize={pageSize}");

            var response = await this.httpClient.GetFromJsonAsync<ExerciseSearchResponse>($"exercises?{string.Join("&", queryParts)}");
            if (response?.Data != null && response.Data.Count > 0)
                return response.Data;
        }
        catch { }

        triedExternal = true;
        return await GetAllExercisesAsync();
    }

    public async Task<List<ExerciseItemDTO>> GetAllExercisesAsync()
    {
        if (cachedExercises != null) return cachedExercises;

        var entered = await fetchLock.WaitAsync(0);
        if (!entered)
        {
            await fetchLock.WaitAsync();
            fetchLock.Release();
            return cachedExercises ?? [];
        }

        try
        {
            if (cachedExercises != null) return cachedExercises;

            if (!triedExternal)
            {
                var external = await TryFetchExternalAllAsync();
                if (external.Count > 0)
                {
                    cachedExercises = external;
                    return new List<ExerciseItemDTO>(cachedExercises);
                }
                triedExternal = true;
            }

            cachedExercises = LoadLocalExercises();
            return new List<ExerciseItemDTO>(cachedExercises);
        }
        finally
        {
            if (entered) fetchLock.Release();
        }
    }

    private async Task<List<ExerciseItemDTO>> TryFetchExternalAllAsync()
    {
        try
        {
            var results = new List<List<ExerciseItemDTO>>();
            foreach (var bp in AllBodyParts)
            {
                var r = await FetchByBodyPartAsync(bp);
                if (r.Count > 0) results.Add(r);
                await Task.Delay(300);
            }
            return results.SelectMany(r => r).DistinctBy(e => e.ExerciseId).ToList();
        }
        catch { return []; }
    }

    private async Task<List<ExerciseItemDTO>> FetchByBodyPartAsync(string bodyPart)
    {
        try
        {
            var response = await this.httpClient.GetFromJsonAsync<ExerciseSearchResponse>($"exercises?bodyParts={Uri.EscapeDataString(bodyPart)}&pageSize=30");
            return response?.Data ?? [];
        }
        catch { return []; }
    }

    private List<ExerciseItemDTO> LoadLocalExercises()
    {
        if (localExercises != null) return localExercises;

        var path = Path.Combine(contentRootPath, "Data", "exercises.json");
        if (!File.Exists(path)) return [];

        var json = File.ReadAllText(path);
        var raw = JsonSerializer.Deserialize<List<LocalExercise>>(json);
        if (raw == null) return [];

        localExercises = raw.Select(MapToDto).ToList();
        return localExercises;
    }

    private ExerciseItemDTO MapToDto(LocalExercise ex)
    {
        var bodyParts = ex.PrimaryMuscles
            .SelectMany(m => MuscleToBodyParts.TryGetValue(m.ToLower(), out var bp) ? bp : [])
            .Distinct()
            .ToList();

        if (bodyParts.Count == 0)
            bodyParts.Add("other");

        var gifUrl = ex.Images.Count > 0
            ? $"https://raw.githubusercontent.com/yuhonas/free-exercise-db/main/exercises/{ex.Images[0]}"
            : null;

        return new ExerciseItemDTO
        {
            ExerciseId = ex.Id,
            Name = ex.Name,
            BodyParts = bodyParts,
            Equipments = string.IsNullOrEmpty(ex.Equipment) ? [] : [ex.Equipment],
            TargetMuscles = ex.PrimaryMuscles,
            SecondaryMuscles = ex.SecondaryMuscles,
            GifUrl = gifUrl ?? "",
            Instructions = ex.Instructions
        };
    }

    public async Task<ExerciseItemDTO?> GetExerciseByIdAsync(string exerciseId)
    {
        if (!triedExternal)
        {
            try
            {
                var response = await this.httpClient.GetFromJsonAsync<ExerciseApiResponse>($"exercise/{exerciseId}");
                if (response?.Data != null) return response.Data;
            }
            catch { }
            triedExternal = true;
        }

        var all = await GetAllExercisesAsync();
        return all.FirstOrDefault(e => e.ExerciseId == exerciseId);
    }
}
