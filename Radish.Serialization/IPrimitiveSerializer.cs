namespace Radish.Serialization;

public interface IPrimitiveSerializer<T>
{
    void Serialize(in T value, string name, IObjectNode parent);
}
