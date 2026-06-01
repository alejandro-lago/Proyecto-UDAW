namespace TfgApi.Models;

public class UserProfileResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Bio { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
}