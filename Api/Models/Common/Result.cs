using System.Diagnostics.CodeAnalysis;

namespace Api.Models.Common;

/// <summary>
/// The result of an operation.
/// </summary>
public class Result
{
    public bool IsSuccess { get; private init; }
    public Error? Error { get; private init; }

    public static Result Success() => new() { IsSuccess = true };

    public static Result Failure(Error error) =>
        new() { IsSuccess = false, Error = error };

    public static Result<T> Success<T>(T content)
        where T : class => Result<T>.Success(content);
}

/// <summary>
/// The result of an operation with content of type T
/// </summary>
/// <typeparam name="T">The type of the returned content.</typeparam>
public class Result<T>
    where T : class
{
    [MemberNotNullWhen(true, nameof(Content))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; private init; }
    public Error? Error { get; private init; }
    public T? Content { get; private set; }

    public static Result<T> Success(T content) =>
        new() { IsSuccess = true, Content = content };

    public static Result<T> Failure(Error error) =>
        new() { IsSuccess = false, Error = error };
}
