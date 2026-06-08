namespace ChipoBackend.Application.Common.Models;

public class Result<T>
{
    public bool Succeeded { get; init; }
    public T? Data { get; init; }
    public string? Error { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];

    public static Result<T> Success(T data) => new() { Succeeded = true, Data = data };
    public static Result<T> Failure(string error) => new() { Succeeded = false, Error = error };
    public static Result<T> Failure(IReadOnlyList<string> errors) => new() { Succeeded = false, Errors = errors };
}
