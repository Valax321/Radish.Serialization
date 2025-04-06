namespace Radish.Serialization;

public interface IListNode : IDocumentNode
{
    IObjectNode AddChildObject(string tag);
    IValueNode AddChildValue();
    IListNode AddChildList();
}
