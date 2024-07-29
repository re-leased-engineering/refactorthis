using System.Diagnostics.CodeAnalysis;

namespace RefactorThis.Domain.Shared;

public class Result
{
    public Result(bool isSuccess, string message)
    {
        IsSuccess = isSuccess;
        Message = message;
    }
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string Message{ get; }
    public static Result Failure(string message) => new(false, message);
    public static Result Success(string message) => new(true, message);
    public static Result Success() => new(true,string.Empty);
}
