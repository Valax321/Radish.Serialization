namespace Radish.Serialization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class SerializedTypeAttribute(string tag) : Attribute
{
    public string Tag { get; } = tag;
}
