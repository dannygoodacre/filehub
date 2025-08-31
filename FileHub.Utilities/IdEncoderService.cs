using System.Numerics;
using FileHub.Application.Abstractions.Services;
using Sqids;

namespace FileHub.Utilities;

internal class IdEncoderService<T>(SqidsEncoder<T> sqidsEncoder) : IIdEncoderService<T> where T : unmanaged, IBinaryInteger<T>, IMinMaxValue<T>
{
    public string? Encode(T id)
    {
        try
        {
            return sqidsEncoder.Encode(id);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public T Decode(string id)
    {
        var decoded = sqidsEncoder.Decode(id);

        return decoded.Count == 0
            ? default
            : decoded[0];
    }
}
