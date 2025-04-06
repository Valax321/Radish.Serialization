namespace Radish.Serialization.Serializers;

internal sealed class StringSerializer : IPrimitiveSerializer<string>
{
    public void Serialize(in string value, string name, IObjectNode parent)
    {
        var valueNode = parent.AddChildValue(name);
        valueNode.SetValue(value);
    }
}
