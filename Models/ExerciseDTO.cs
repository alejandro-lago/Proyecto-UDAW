namespace TfgApi.Models;

public class ExerciseApiResponse
{
    public ExerciseItemDTO? Data { get; set; }
}

public class ExerciseSearchResponse
{
    public List<ExerciseItemDTO> Data { get; set; } = new();
}

public class ExerciseItemDTO
{
    public string ExerciseId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> BodyParts { get; set; } = new();
    public List<string> Equipments { get; set; } = new();
    public List<string> TargetMuscles { get; set; } = new();
    public List<string> SecondaryMuscles { get; set; } = new();
    public string GifUrl { get; set; } = string.Empty;
    public List<string> Instructions { get; set; } = new();
}