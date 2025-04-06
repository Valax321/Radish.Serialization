namespace Radish.Serialization;

public interface ISerializable<T>
{
    static void Serialize(T me, IDocumentNode parent, string name, SerializationContext context)
    {
        throw new NotImplementedException();
    }
}
