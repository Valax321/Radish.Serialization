namespace Radish.Serialization;

public interface IObjectNode : IDocumentNode
{
    IObjectNode AddChildObject(string name, string tag);
    IListNode AddChildList(string name);
    IValueNode AddChildValue(string name);
}
