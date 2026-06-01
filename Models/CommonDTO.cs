namespace TfgApi.Models;

public class PagedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int) Math.Ceiling(TotalCount / (double) PageSize);
}

public class ApiErrorResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public List<string>? Errors { get; set; }
}