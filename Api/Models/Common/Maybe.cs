namespace Api.Models.Common;

/// <summary>
/// An optional value of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public class Maybe<T>
    where T : class
{
    private readonly T? _value;

    private Maybe(T? value)
    {
        _value = value;
    }

    public static Maybe<T> Some(T value) =>
        new(value ?? throw new ArgumentNullException(nameof(value)));

    public static Maybe<T> None() => new(null);

    public bool HasValue => _value != null;

    public T Value =>
        _value ?? throw new InvalidOperationException("Maybe does not have a value.");
}

public static class Maybe
{
    public static Maybe<T> Some<T>(T value)
        where T : class => Maybe<T>.Some(value);
}
