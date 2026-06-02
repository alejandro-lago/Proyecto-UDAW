using TfgApi.Models;

namespace TfgApi.Services;

public class ExerciseApiService : IExerciseApiService
{
    //  With  HttpClient, we can send request to URLs an get answers
    //  The "readonly" is because we are only going to set it in the constructor, readonly means only can be set when declared or in the constructor    
    private readonly HttpClient httpClient;

    public ExerciseApiService(HttpClient httpClient)
    {
        // HttpCLient is injected by ASP.NET, we don't create it ourselves, the system gives it to us already setted up
        this.httpClient = httpClient;
        this.httpClient.BaseAddress = new Uri("https://oss.exercisedb.dev/api/v1/");
    }

    //  "async" means this method can pause and wait for the internet without freezing the proyect
    //  "Task<List<...>>" is the returning type. Here returns a list of objects
    public async Task<List<ExerciseItemDTO>> SearchExerciseAsync( string? name = null, string? muscle = null, int page = 1, int pageSize = 30)
    {
        //  Here I build the URL query string step by step, I use a List of strings since is easier to add parts conditionally
        var queryParts = new List<string>();
        if (!string.IsNullOrEmpty(name)) queryParts.Add($"name={Uri.EscapeDataString(name)}");
        if (!string.IsNullOrEmpty(muscle)) queryParts.Add($"name={Uri.EscapeDataString(muscle)}");
        queryParts.Add($"page={page}");
        queryParts.Add($"pageSize={pageSize}");

        //  Here I join all the parts to create a proper query string
        var queryString = string.Join("&", queryParts);

        //  "await" means it will wait until it gets an answer, but without blocking the program
        //  "GetFromJsonAsync" sends GET request and then, transform the recieved JSON into a C# object
        //  and with the "ExerciseSearchResponse" gives the shape
        var response = await this.httpClient.GetFromJsonAsync<ExerciseSearchResponse>($"exercise?{queryString}");

        //  If the api fails getting a null response, it will return an empty list instead of crashing
        //  thanks to the "??" which means that "if it is null, use this instead" (null-coalescing operator)
        return response?.Data ?? new List<ExerciseItemDTO>();
    }

    public async Task<ExerciseItemDTO?> GetExerciseByIdAsync(string exerciseId)
    {
        var response = await this.httpClient.GetFromJsonAsync<ExerciseApiResponse>($"exercise/{exerciseId}");
        return response?.Data;
    }

    public async Task<ExerciseItemDTO?> GetRandomExerciseAsync()
    {
        var response = await this.httpClient.GetFromJsonAsync<ExerciseApiResponse>("exercise/random");
        return response?.Data;
    }
}