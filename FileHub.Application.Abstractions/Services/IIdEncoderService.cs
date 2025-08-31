namespace FileHub.Application.Abstractions.Services;

public interface IIdEncoderService<T>
{
    /// <summary>
    /// Encode the internal ID.
    /// </summary>
    /// <param name="id">Internal ID.</param>
    /// <returns>An external ID.</returns>
    public string? Encode(T id);

    /// <summary>
    /// Decode the external ID.
    /// </summary>
    /// <param name="id">External ID.</param>
    /// <returns>The associated internal ID.</returns>
    public T Decode(string id);
}
