namespace Radish.Serialization;

public interface IPreSerializeCallback
{
    void OnSerialize(SerializationContext context, IDocumentNode node);
}
