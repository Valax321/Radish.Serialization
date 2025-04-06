namespace Radish.Serialization;

public class SerializationContext
{
    public IPrimitiveSerializer<T>? GetPrimitiveSerializer<T>()
    {
        if (SerializerRegistry.TryFindSerializer<T>(out var p))
            return p;
        return null;
    }
}
