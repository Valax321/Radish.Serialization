namespace Radish.Serialization;

public interface IValueNode : IDocumentNode
{
    void SetValue<T>(in T value);
}
